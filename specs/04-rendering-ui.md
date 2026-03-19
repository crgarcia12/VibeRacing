# **DUST RACING 2D - COMPREHENSIVE GRAPHICS RENDERING SPECIFICATION**

## **1. RENDERING PIPELINE & ARCHITECTURE**

### **1.1 Overview**
VibeRacing uses a multi-layered OpenGL rendering system with:
- **Engine**: Built on MCGLScene (custom engine framework)
- **OpenGL Support**: Both Legacy (v1.20) and Modern (v1.30+) shader pipelines
- **Platform Support**: Desktop OpenGL + OpenGL ES (GLES) with precision qualifiers
- **Resolution Flexibility**: Configurable window and fullscreen resolutions
- **Framebuffer Pipeline**: Dual framebuffer objects (main + shadow) for advanced rendering

### **1.2 Projection & Camera Configuration**
```
View Angle:        22.5°
Z-Near Plane:      10.0
Z-Far Plane:       10,000.0
Default Scene:     1024 x 768 pixels (configurable)
METERS_PER_UNIT:   0.05f
```

### **1.3 Framebuffer Architecture**

#### **Primary Framebuffer (m_fbo)**
- Dimensions: Matches viewport resolution (m_hRes × m_vRes)
- Attachments: Color + Depth buffer
- Purpose: Main scene rendering (track + objects)
- Rendering Stages:
  1. Scene objects (MCRenderGroup::Objects)
  2. Shadows (ObjectShadows)
  3. Particles
  4. HUD overlays

#### **Shadow Framebuffer (m_shadowFbo)**
- Dimensions: Matches viewport resolution
- Attachments: Color + Depth buffer (copied from primary)
- Purpose: Shadow map generation via depth blitting
- Blending: Depth buffer blit from primary → shadow
- Transparency: Rendered with GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA

### **1.4 Rendering Sequence**

```
1. renderObjects()
   ├─ FBO bind
   ├─ glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT)
   ├─ Scene track
   ├─ Menu
   └─ World objects (MCRenderGroup::Objects)

2. renderHud()
   ├─ Shadow pass (if FRAMEBUFFER_BLITS enabled)
   │  ├─ shadowFbo bind
   │  ├─ Depth blit from primary
   │  ├─ ObjectShadows render
   │  └─ ParticleShadows render
   ├─ Particle pass
   ├─ HUD pass
   └─ Common HUD pass

3. renderScreen()
   ├─ FBO texture → screen
   └─ Fade value applied (0.0-1.0)
```

### **1.5 Render Groups (MCRenderGroup)**
- **Objects**: Main scene geometry (track tiles, cars, obstacles)
- **ObjectShadows**: Dynamic shadow mapping
- **Particles**: Dust, smoke, collision effects
- **ParticleShadows**: Shadow information for particles
- **Depth Testing**: Disabled for overlay rendering

---

## **2. SHADER SYSTEM**

### **2.1 Shader Programs Registry**

| Handle | Vertex | Fragment | Purpose |
|--------|--------|----------|---------|
| **default** | Engine | Engine | Standard 3D objects |
| **defaultSpecular** | Engine | Engine | Specular highlights |
| **defaultShadow** | Engine | Engine | Shadow rendering |
| **text** | Engine | Engine | HUD text |
| **textShadow** | Engine | Engine | Text with shadows |
| **car** | Custom | Custom | Vehicle rendering |
| **menu** | Custom | Custom | Menu items |
| **tile2d** | Custom | Default | 2D track tiles |
| **tile3d** | Custom | Custom | 3D track tiles |
| **fbo** | Custom | Custom | Framebuffer composite |

### **2.2 Custom Shaders: CAR SHADER**

#### **Vertex Shader (carVsh)**
```glsl
#version 120 (or 130 for GL30)

Attributes:
  vec3  inVertex       (vertex position)
  vec3  inNormal       (normal vector)
  vec2  inTexCoord     (texture coordinates)
  vec4  inColor        (vertex color)

Uniforms:
  vec4  scale          (default: 1,1,1,1)
  mat4  vp             (view-projection matrix)
  mat4  v              (view matrix)
  mat4  model          (model matrix)
  vec4  dd             (diffuse direction, default: 1,1,1,1)
  vec4  dc             (diffuse color, default: 1,1,1,1)
  vec4  sd             (specular direction, default: 1,1,1,1)
  vec4  sc             (specular color, default: 1,1,1,1)
  vec4  ac             (ambient color, default: 1,1,1,1)
  float sCoeff         (specular coefficient, default: 1.0)

Operations:
  ✓ Normal mapping via 3x3 matrix from model
  ✓ Eye-space lighting calculations
  ✓ Specular reflection using dot(reflect(L,N),V)
  ✓ Color blending: (inColor * ac.rgb * ac.a) + (sc.rgb * specularity)
```

#### **Fragment Shader (carFsh)**
```glsl
Textures:
  sampler2D tex0       (color/diffuse map)
  sampler2D tex1       (sky/environment map)
  sampler2D tex2       (normal map)

Uniforms:
  vec4  dd             (diffuse direction)
  vec4  dc             (diffuse color)
  float dCoeff         (diffuse coefficient, default: 1.0)

Operations:
  ✓ Discard if texColor.a < 0.1 (alpha testing)
  ✓ Normal unpacking: normalize(normal.xyz - vec3(0.5,0.5,0.5)) * 2.0
  ✓ Diffuse intensity: dot(-ddRotated.xyz, N) * dc.a
  ✓ Reflection calculation: 0.33 * ((1-R) + G + (1-B))
  ✓ Cubic reflection: refl *= refl * refl
  ✓ Final color blend:
    80% = (sky * reflection) + (texColor * (1-reflection))
    20% = (dc.rgb * diffuse * dCoeff) + vColor
```

### **2.3 Menu Shader**

#### **Vertex Shader (menuVsh)**
```glsl
Attributes:
  vec3  inVertex
  vec2  inTexCoord
  vec4  inColor

Uniforms:
  vec4  color       (color multiplier, default: 1,1,1,1)
  vec4  scale       (scale multiplier, default: 1,1,1,1)
  mat4  vp          (view-projection)
  mat4  model       (model matrix)

Output: vColor * color, scaled vertices
```

#### **Fragment Shader (menuFsh)**
```glsl
Uniforms:
  float fade        (fade value 0.0-1.0, default: 1.0)

Operations:
  ✓ Sample texture at texCoord0
  ✓ Discard if texColor.a < 0.1
  ✓ Output: vColor * texColor * fade
```

### **2.4 Framebuffer Shader (fboVsh/fboFsh)**
**Purpose**: Composite framebuffer texture to screen with fade effects

```glsl
// Vertex: Simple pass-through
gl_Position = vec4(inVertex, 1)
texCoord0 = inTexCoord

// Fragment: 
gl_FragColor = texture2D(tex0, texCoord0) * fade
```

### **2.5 Tile Shaders**

#### **Tile Vertex Shader (tileVsh)**
```glsl
Uniforms:
  vec4  dd          (diffuse direction)
  vec4  dc          (diffuse color)
  vec4  ac          (ambient color)
  float dCoeff      (diffuse coefficient)

Calculation:
  di = dot(dd.xyz, -inNormal) * dc.a
  vColor = inColor * (ac.rgb * ac.a + dc.rgb * di * dCoeff)
```

#### **Tile3D Fragment Shader (tile3dFsh)**
```glsl
Samplers:
  tex0, tex1, tex2  (color variants)

Logic:
  ✓ Layer blending based on (r+b) vs g channels
  ✓ If (r+b) <= g: blend(color1, color2, g)
  ✓ Else if r==0 && b==0: use color1
  ✓ Final: color0 * vColor
```

---

## **3. UI OVERLAYS SYSTEM**

### **3.1 Overlay Hierarchy**

```
OverlayBase (abstract)
├─ UpdateableIf
└─ Renderable

Concrete Overlays:
├─ TimingOverlay (extends OverlayBase, QObject)
├─ MessageOverlay (extends OverlayBase, QObject)
├─ CrashOverlay (extends OverlayBase)
├─ StartlightsOverlay (extends OverlayBase)
└─ CarStatusView (extends Renderable)
```

### **3.2 TIMING OVERLAY**

#### **Dimensions & Layout**
- **Font Size**: Text glyphs 15×15 pixels (timing info), 20×20 pixels (lap/position)
- **Speed Font**: 40×40 pixels (main), 20×20 pixels (label)
- **Shadow Offset**: +2 pixels X, -2 pixels Y
- **Position Reference**: Bottom-right corner for timing info

#### **Rendered Information** (Right-aligned, bottom)

| Element | Y-Position | Color | Font Size | Content |
|---------|-----------|-------|-----------|---------|
| **LAP** | height - (1 × 15) | WHITE (1,1,1) | 20×20 | "LAP: X/Y" |
| **POS** | height - (2 × 15) | YELLOW (1,1,0) | 20×20 | "POS: 1st/2nd/3rd..." |
| **SPEED** | h (top-left) | Dynamic* | 40×40 | Speed value |
| **KM/H** | h + (2 × text height) | WHITE | 20×20 | "KM/H" label |
| **LAP TIME** | height - (2 × 15) | Dynamic** | 15×15 | "LAP: MM:SS.mmm" |
| **LAST LAP** | height - (3 × 15) | WHITE | 15×15 | "L: MM:SS.mmm" |
| **RECORD** | height - (4 × 15) | WHITE | 15×15 | "R: MM:SS.mmm" |
| **CAR STATUS** | Top-right + 10px | N/A | N/A | Visual indicator |

#### **Color Scheme**
- **WHITE**: `RGB(1.0, 1.0, 1.0)` - Base text color
- **RED**: `RGB(1.0, 0.0, 0.0)` - Speed > 200 km/h, slow lap
- **GREEN**: `RGB(0.0, 1.0, 0.0)` - Fast lap (better than last)
- **YELLOW**: `RGB(1.0, 1.0, 0.0)` - Position info

#### **Speed Indicator Logic**
```cpp
if (speed < 100)     → WHITE
else if (speed < 200) → YELLOW  
else                 → RED
```

#### **Lap Time Color Logic**
```
if (raceCompleted || lastLapTime == -1 || currentTime == lastTime)  → WHITE
else if (currentTime < lastLapTime)                                 → GREEN (ahead!)
else                                                                 → RED (slower)
```

#### **Blink Animation** (Record achievment notifications)
- **Trigger**: Lap or race record beaten
- **Duration**: 10 intervals × 250ms = 2.5 seconds
- **Pattern**: Toggle visibility every 250ms
- **Elements**: lapRecordTime, raceTime, carStatus

#### **Position Rendering** (Position strings)
```
Position Texts: ["---", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th"]
Lap Diff Display: "+N LAP" or "+N LAPS" if behind leader
```

#### **Car Status View Positioning**
```
Position: (width - carStatusView.width, carStatusView.height + 10)
```

### **3.3 MESSAGE OVERLAY**

#### **Configuration**
- **Glyph Size**: 20×20 pixels
- **Shadow Offset**: +2 pixels X, -2 pixels Y
- **Max Display Time**: 180 frames (default, configurable)
- **Animation**: Smooth Y-coordinate interpolation
- **Vertical Spacing**: One glyph height between messages

#### **Alignment Modes**

| Mode | Starting Y | Y-Increment | Purpose |
|------|-----------|------------|---------|
| **Bottom** | GLYPH_HEIGHT (20) | +20 | Rising stack |
| **Top** | height - 2×GLYPH_HEIGHT | -20 | Falling stack |

#### **Animation Logic**
```cpp
Initial Message Y:        -1 (uninitialized)
Target Y Offset:          Calculated per alignment mode
Y Interpolation:          m.y = m.y + (targetY - m.y) / 4
Removal Trigger:          timeShown >= maxTime/2
Removal Target Y:         -20 (off-screen)
Final Removal:            timeShown >= maxTime
```

#### **Rendering Centering**
```
X Position: width / 2 - (text.width / 2)  // Center horizontally
```

### **3.4 CRASH OVERLAY**

#### **Visual Properties**
- **Surface**: "crashOverlay" texture
- **Alpha Blending**: Enabled (GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
- **Depth Testing**: Disabled (glDisable GL_DEPTH_TEST)
- **Size**: Full screen (width × height)
- **Position**: Screen center (width/2, height/2, 0)

#### **Crash Detection & Animation**
```cpp
Trigger:           Car.hadHardCrash() == true
Initial Alpha:     0.75f (75% opacity)
Decay Function:    alpha *= 0.97f (3% per frame decay)
Fade Out Threshold: alpha < 0.01f → disable overlay
```

#### **Color Blending**
```cpp
MCGLColor: (1.0, 1.0, 1.0, alpha)  // White flash that fades
```

### **3.5 CAR STATUS VIEW**

#### **Components**
```cpp
Body Surface:   "tireStatusIndicatorBody" texture
Tire Surface:   "tireStatusIndicatorTires" texture
Dimensions:     height = body.height, width = tires.width
Alpha:          0.9f (90% opacity)
```

#### **Color Mapping** (Dynamic based on car condition)
```cpp
Damage Level Indicator:
  Red Channel:   1.0f (always full)
  Green Channel: damageLevel (0.0-1.0)
  Blue Channel:  damageLevel (0.0-1.0)
  Alpha:         0.9f

Tire Wear Indicator:
  Red Channel:   1.0f (always full)
  Green Channel: tireWearLevel (0.0-1.0)
  Blue Channel:  tireWearLevel (0.0-1.0)
  Alpha:         0.9f

Range: Full color when healthy → Red/Magenta when damaged/worn
```

#### **Positioning**
```
Default Position: (0, 0)
Manual Set: setPos(x, y)
Typical HUD Placement: Top-right corner, offset 10px from edges
```

---

## **4. MINIMAP SYSTEM**

### **4.1 Minimap Architecture**

#### **Initialization Parameters**
```cpp
Car & carToFollow           // Car to center minimap on
const MapBase & trackMap    // Track tile matrix
int x, int y                // Minimap center position
size_t size                 // Minimap width/height in pixels
```

#### **Tile Sizing Algorithm**
```cpp
m_tileW = size / trackMap.cols()
m_tileH = size / trackMap.rows()

// Ensure square tiles
if (m_tileW > m_tileH)
    m_tileW = m_tileH
else
    m_tileH = m_tileW
```

#### **Map Centering**
```cpp
// Even columns
if (trackMap.cols() % 2 == 0)
    initX = x - trackMap.cols() * m_tileW / 2 + m_tileW / 4
else
    initX = x - trackMap.cols() * m_tileW / 2

initY = y - trackMap.rows() * m_tileH / 2
```

#### **Marker Size Calculation**
```cpp
markerSize = max(Scene.width * 0.01f, m_tileH * 0.75f)
```

### **4.2 Minimap Rendering Layers**

#### **Layer 1: Map Tiles**
```cpp
For each tile in track:
  ├─ Shader: "menu"
  ├─ Color: (1.0, 1.0, 1.0) white
  ├─ Size: m_tileH × m_tileW
  └─ Source: tile.previewSurface()
```

#### **Layer 2: Car Markers**

| Car Type | Color | RGB | Alpha | Details |
|----------|-------|-----|-------|---------|
| **Followed** | Car Color | From car.surface | 0.9 | Current player car |
| **Leader** | Green | (0.1, 0.9, 0.1) | 0.9 | 1st place |
| **Last Place** | Red | (0.9, 0.1, 0.1) | 0.9 | Last position |
| **Others** | Gray | (0.2, 0.2, 0.2) | 0.9 | Other AI/players |

#### **Marker Rendering Order** (Layering Priority)
```
1. Back: Other AI cars (gray)
2. Middle: Last place car (red)
3. Middle: Leader car (green)
4. Front: Followed car (colored)
```

#### **Marker Scaling**
```cpp
// Scale to minimap coordinate system
markerPos = minimap.center + (car.location * minimap.size.x / trackWidth) - (minimap.size * 0.5f)
```

### **4.3 Minimap Surface Properties**
- **Shader**: "menu" (uses menuVsh/menuFsh)
- **Alpha Blending**: Enabled
- **Texture Filtering**: GL_LINEAR (smooth scaling)
- **Tile Rotation**: Supported (per tile.rotation())

---

## **5. START LIGHTS SYSTEM**

### **5.1 Start Lights State Machine**

#### **State Sequence**
```
Init → Appear → FirstRow → SecondRow → ThirdRow → Go → Disappear → End
```

#### **Timing Configuration**
```cpp
stepsPerState = 60        // frames per state
Timer interval = 1000 / stepsPerState = ~16.7ms
Total animation duration ≈ 8 states × 1 second ≈ 8 seconds
```

### **5.2 Animation States Details**

| State | Duration | Lit Rows | Action | Message | Input |
|-------|----------|----------|--------|---------|-------|
| **Init** | 60 fr | N/A | Position lights | None | Disabled |
| **Appear** | 60 fr | 0 | Slide up (1/3 speed) | None | Disabled |
| **FirstRow** | 60 fr | 1 | Static | "3" | Disabled |
| **SecondRow** | 60 fr | 2 | Static | "2" | Disabled |
| **ThirdRow** | 60 fr | 3 | Static | "1" | Disabled |
| **Go** | 60 fr | 0 | Glow fade → fade out | "GO!!!" | **ENABLED** |
| **Disappear** | 20 fr | 0 | Slide down (1/3 speed) | None | Enabled |
| **End** | N/A | N/A | Stop | None | Enabled |

### **5.3 Position Calculation**

#### **Start Position**
```cpp
Initial Y: pos.y = height * 3 / 2  (below screen)
Target Y:  pos.y = height / 2      (center screen)
```

#### **Animation Type**
```cpp
MCVectorAnimation:
  From:     (width/2, 3*height/2, 0)
  To:       (width/2, height/2, 0)
  Duration: stepsPerState / 3 frames
```

### **5.4 Light Grid Layout**

#### **Grid Dimensions**
```
Columns: 8 lights per row
Rows:    3 rows (lit progressively)
```

#### **Light Sprites**
- **startLightOn**: Lit light (default color)
- **startLightOnCorner**: Lit light at corners (rotated 0°/90°/180°/270°)
- **startLightOff**: Unlit light (dimmed, alpha transparent)
- **startLightOffCorner**: Unlit corner light
- **startLightGlow**: Glow effect (scaled)

#### **Light Rendering Pattern**
```cpp
// Corners use rotated variants
if (row == 0 && col == 0)         → render at 0°
else if (row == rows-1 && col == 0)   → render at 90°
else if (row == rows-1 && col == 7)   → render at 180°
else if (row == 0 && col == 7)     → render at 270°
else                                → render regular
```

#### **Positioning Formula**
```cpp
x = model.pos().x - (8-1) * lightOn.width() / 2
y = model.pos().y - (rows-1) * lightOn.height() / 2
height = rows * lightOn.height()

For each light (row, col):
  pos = (x + col*width, y + height - row*height, 0)
```

### **5.5 Visual Effects**

#### **Glow Animation**
```cpp
// FirstRow/SecondRow/ThirdRow
glowScale = model.glowScale()   // Dynamic scale factor

// Go state
glowScale *= 0.75f per frame    // Rapid decay
Alpha fade: alpha *= 0.98f       // Gradual transparency
```

#### **Glow Rendering**
```cpp
startLightGlow.setScale(glowScale, glowScale, 1.0f)
startLightGlow.setColor(1.5f, 0.25f, 0.25f, 0.4f)  // Red-tinted glow
```

#### **Disappear Animation**
```cpp
startLightOff/OffCorner colors: (1.0, 1.0, 1.0, m_alpha)
Fade: alpha *= 0.98f per frame
```

### **5.6 Depth Control**
```cpp
glDisable(GL_DEPTH_TEST)  // Always render on top
```

---

## **6. GRAPHICS FACTORY**

### **6.1 Generated Graphics**

#### **Number Surface (Car Numbers)**
```cpp
Pixmap Size:      32×32 pixels
Rendered Size:    9×9 pixels (dynamic at render time)
Font:             Bold, pixel-sized (height=32)
Text Color:       Black (0,0,0) on white background
Rendering Pos:    Centered
Mip Filter:       GL_LINEAR
Mag Filter:       GL_LINEAR
Handle:           "Number0", "Number1", etc.
```

#### **Minimap Marker**
```cpp
Pixmap Size:      64×64 pixels (transparent)
Rendered Size:    9×9 pixels (dynamic)
Shape:            White ellipse/circle
Border:           2-pixel margin from edges
Mip Filter:       GL_LINEAR
Mag Filter:       GL_LINEAR
Handle:           "Minimap"
Qt Color:         Qt::white, drawn as ellipse
```

---

## **7. FONT SYSTEM**

### **7.1 Font Factory Configuration**

#### **Font Properties**
```cpp
Family:           "DejaVu Sans"
Style Hint:       QFont::Monospace
Pixel Size:       64 pixels
Bold:             true
```

#### **Glyph Grid Layout**
```
Grid Dimensions:  8 columns × N rows
Total Glyphs:     Russian (33) + ASCII (0-255) = ~300+
Slot Size:        Calculated per glyph width + 4px margin height
```

#### **Glyph Density Compensation**
```cpp
xDensity:         0.75f  // Horizontal scaling (75% of slot)
yDensity:         1.0f   // Vertical scaling (100% of slot)
```

### **7.2 Character Support**

#### **Russian Characters**
```
Uppercase: А-Я (32 letters) + Ё
Lowercase: а-я (32 letters) + ё
Special:   №
```

#### **ASCII Support**
```
Range:     0-255 (full ASCII)
Includes:  Digits, letters, punctuation, symbols
```

#### **Fallback Characters** (Diacritics → Base characters)
```cpp
Á→A, á→a, Č→C, č→c, Ď→D, ď→d, Ě→E, ě→e,
Í→I, ì→i, í→i, Ň→N, ň→n, Ó→O, ò→o, ó→o,
Ř→R, ř→r, Š→S, š→s, Ť→T, ť→t, Ú→U, ú→u,
Ů→U, ů→u, ù→u, Ý→Y, ý→y, Ž→Z, ž→z
```

### **7.3 Font Texture Generation**

#### **Texture Size Calculation**
```
slotWidth:       max(glyphWidth) across all chars
slotHeight:      max(fontMetrics.height) + 4px margin
textureWidth:    8 * slotWidth
textureHeight:   (glyphCount / 8) * slotHeight
```

#### **Texture Properties**
```cpp
Format:          RGBA
Background:      Qt::transparent
Text Color:      White (255,255,255)
Rendering:       Qt::AlignCenter per glyph
Filtering:       GL_LINEAR (smooth scaling)
```

#### **Glyph Coordinate System**
```cpp
For glyph at grid position (i, j):
  x0 = i * textureW / cols
  y0 = (rows - j) * textureH / rows       // Inverted Y
  x1 = (i + 1) * textureW / cols
  y1 = (rows - j - 1) * textureH / rows
```

### **7.4 Font Loading Pipeline**
```
1. Load font from data/fonts/DejaVuSans-Bold.ttf (if not system fonts)
2. Register with QFontDatabase.addApplicationFontFromData()
3. Create texture font via MCAssetManager.textureFontManager()
4. Generate glyph data for all characters
5. Create texture surface with calculated dimensions
6. Store in MCAssetManager for HUD text rendering
```

---

## **8. HUD TEXT RENDERING**

### **8.1 Text Properties**
- **Font**: DejaVu Sans (Bold)
- **Engine**: MCTextureText with MCTextureFont
- **Shadow**: Enabled with offset (2, -2)
- **Antialiasing**: Hardware-accelerated (GL_LINEAR filtering)

### **8.2 Depth Ordering (Z-Values)**

All overlays render with:
```cpp
glDisable(GL_DEPTH_TEST)  // UI always on top
```

**Rendering Order** (Front to Back):
1. **HUD Elements** (Timing, Messages, Status)
2. **Crash Overlay** (semi-transparent flash)
3. **Minimap** (if split-screen)
4. **Start Lights** (countdown sequence)
5. **Game World** (track, cars, particles)
6. **Shadows** (rendered in shadow FBO)

---

## **9. COLOR PALETTE**

### **UI Colors**
```cpp
WHITE           (1.0, 1.0, 1.0)         // Standard text
BLACK           (0.0, 0.0, 0.0)         // Text on white
RED             (1.0, 0.0, 0.0)         // Warnings, slow
GREEN           (0.0, 1.0, 0.0)         // Good, fast
YELLOW          (1.0, 1.0, 0.0)         // Caution, neutral
LIGHT_RED       (0.9, 0.1, 0.1)         // Last place marker
LIGHT_GREEN     (0.1, 0.9, 0.1)         // Leader marker
GRAY            (0.2, 0.2, 0.2)         // Other cars
GLOW_RED        (1.5, 0.25, 0.25, 0.4)  // Start lights glow
```

### **Alpha Transparency**
```
Overlay Base Alpha:        1.0f (fully opaque)
Crash Overlay Initial:     0.75f (75%)
Minimap Markers:           0.9f (90%)
Start Light Glow:          0.4f (40%)
Car Status View:           0.9f (90%)
```

---

## **10. RENDERING OPTIMIZATION**

### **10.1 FBO Blitting**
```cpp
#ifdef DISABLE_FRAMEBUFFER_BLITS
  // Fallback: inline shadow rendering
#else
  // Optimized: separate FBO blit
  QOpenGLFramebufferObject::blitFramebuffer(
    shadowFbo, mainFbo, GL_DEPTH_BUFFER_BIT
  )
#endif
```

### **10.2 Framebuffer Object (FBO) Properties**
- **Attachment**: QOpenGLFramebufferObject::Depth
- **Blending**: Alpha blending for shadow pass
- **Texture Binding**: Shadow texture to slot 0
- **Resolution**: Matches render target (dynamic)

### **10.3 Material Configuration**
```cpp
// Shadow pass
material.setTexture(shadowFbo.texture(), 0)
material.setAlphaBlend(true, GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA)
shadowColor = (1, 1, 1, 0.5f)

// Screen pass
material.setTexture(mainFbo.texture(), 0)
material.setAlphaBlend(false)
screenColor = default
```

### **10.4 Fade Animation System**
- **Variable**: Renderer.m_fadeValue (0.0-1.0)
- **Application**: Multiplied in fragment shaders (uniform fade)
- **Typical Use**: Scene transitions, menu fading

---

## **11. PIXEL SIZE REFERENCE TABLE**

| Element | Width | Height | Notes |
|---------|-------|--------|-------|
| **Timing Text** | Variable | 15px | Speed/lap time |
| **Position Text** | Variable | 20px | "POS: 1st" |
| **Speed Display** | Variable | 40px | Main speed number |
| **KM/H Label** | Variable | 20px | Speed unit |
| **Message Text** | Variable | 20px | Centered overlay |
| **Car Number** | 9px | 9px | Rendered at 32×32 src |
| **Minimap Marker** | Dynamic | Dynamic | 1% scene width, min 0.75 × tileH |
| **Start Light** | Variable | Variable | Based on layout grid |
| **Car Status** | tires.width | body.height | Dynamic sizing |

---

## **12. CONSTANTS & MAGIC NUMBERS**

```cpp
// Timing Overlay
GLYPH_W_TIMES = 15
GLYPH_H_TIMES = 15
GLYPH_W_POS = 20
GLYPH_H_POS = 20

// Message Overlay
GLYPH_WIDTH = 20
GLYPH_HEIGHT = 20
DEFAULT_MESSAGE_MAX_TIME = 180 frames

// Start Lights
STARTLIGHTS_STEPS_PER_STATE = 60
STARTLIGHTS_GRID_COLS = 8
STARTLIGHTS_GRID_ROWS = 3

// Font
FONT_PIXEL_SIZE = 64px
FONT_GLYPH_GRID = 8 columns
FONT_SLOT_MARGIN = 4px (height)
FONT_X_DENSITY = 0.75
FONT_Y_DENSITY = 1.0

// Graphics Factory
NUMBER_PIXMAP_SIZE = 32×32px
NUMBER_RENDER_SIZE = 9×9px
MINIMAP_MARKER_PIXMAP = 64×64px
MINIMAP_MARKER_RENDER = 9×9px
MINIMAP_MARKER_BORDER = 2px

// Crash Overlay
CRASH_ALPHA_INITIAL = 0.75f
CRASH_ALPHA_DECAY = 0.97f per frame
CRASH_ALPHA_MIN = 0.01f

// Car Status
CAR_STATUS_ALPHA = 0.9f

// Minimap
MINIMAP_MARKER_SIZE = max(scene.width * 0.01, tileH * 0.75)
MINIMAP_TILE_SQUARE = true

// Message Animation
MESSAGE_Y_INTERPOLATION = (targetY - y) / 4
MESSAGE_REMOVAL_TRIGGER = timeShown >= maxTime / 2
MESSAGE_REMOVAL_Y = -20

// Blink Animation (Records)
BLINK_INTERVAL = 250ms
BLINK_COUNT = 10 intervals
```

---

## **13. TEXTURE/SURFACE REFERENCES**

### **Required Surfaces (Asset Manager)**
```
Core HUD:
  - "tireStatusIndicatorBody"     (Car status background)
  - "tireStatusIndicatorTires"    (Tire wear indicator)
  - "crashOverlay"                (Crash flash texture)

Start Lights:
  - "startLightOn"                (Lit light)
  - "startLightOnCorner"          (Lit corner)
  - "startLightOff"               (Unlit light)
  - "startLightOffCorner"         (Unlit corner)
  - "startLightGlow"              (Glow effect)

Generated:
  - "Number0" through "NumberN"   (Car numbers)
  - "Minimap"                     (Marker circle)
  - "DejaVu Sans"                 (Font texture)
```

---

## **14. SHADER VARIABLE MAPPING**

| Uniform | Type | Range | Purpose |
|---------|------|-------|---------|
| vp | mat4 | N/A | View-Projection matrix |
| v | mat4 | N/A | View matrix |
| model | mat4 | N/A | Model matrix |
| scale | vec4 | Any | Object scale XYZ |
| dd | vec4 | 0-1 | Diffuse direction |
| dc | vec4 | 0-1 | Diffuse color + alpha |
| sd | vec4 | 0-1 | Specular direction |
| sc | vec4 | 0-1 | Specular color |
| ac | vec4 | 0-1 | Ambient color + alpha |
| sCoeff | float | 0-128 | Specular power |
| dCoeff | float | 0-1 | Diffuse coefficient |
| fade | float | 0-1 | Global fade value |
| color | vec4 | 0-1 | Tint/color multiplier |
| tex0, tex1, tex2 | sampler2D | N/A | Texture units |

---

## **15. RENDERING PIPELINE SUMMARY DIAGRAM**

```
Game Loop
  ↓
Renderer::renderNow()
  ├─ renderObjects()
  │  ├─ FBO bind
  │  ├─ Clear buffers
  │  ├─ Render track
  │  ├─ Render menu
  │  └─ Render world objects
  │
  ├─ renderHud()
  │  ├─ [Optional] Shadow pass
  │  │  ├─ Shadow FBO bind
  │  │  ├─ Depth blit
  │  │  ├─ Render shadows
  │  │  └─ Release shadow FBO
  │  │
  │  ├─ Main FBO bind
  │  ├─ Render particles
  │  ├─ Render HUD overlays
  │  │  ├─ TimingOverlay
  │  │  ├─ MessageOverlay
  │  │  ├─ CrashOverlay
  │  │  ├─ StartlightsOverlay
  │  │  └─ Minimap
  │  └─ Release FBO
  │
  └─ renderScreen()
     ├─ FBO texture → screen
     ├─ Apply fade uniform
     └─ Swap buffers
```

---

**Document Revision**: v1.0  
**Last Updated**: Based on source code analysis  
**Engine**: Qt/OpenGL with MCGL framework  
**Game**: VibeRacing by Jussi Lind
