# DUSTRACING2D MENU SYSTEM - VISUAL & LAYOUT SPECIFICATION

## VISUAL HIERARCHY & SCREEN LAYOUTS

### Main Menu Visual Layout
`
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║                    [DUSTRACE BACKGROUND]                    ║
║                    (50% Dimmed Overlay)                      ║
║                                                              ║
║                                                              ║
║                           PLAY                              ║
║                           (Yellow if focused)               ║
║                                                              ║
║                           HELP                              ║
║                           (Yellow if focused)               ║
║                                                              ║
║                          CREDITS                            ║
║                           (Yellow if focused)               ║
║                                                              ║
║                          SETTINGS                           ║
║                           (Yellow if focused)               ║
║                                                              ║
║                           QUIT                              ║
║                           (Yellow if focused)               ║
║                                                              ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝

Layout Properties:
- Vertical list, centered
- Item height: 1/8 of screen
- 5 items total
- Glyph size: 40px
- Focus: YELLOW with 1.25x scale
- Text animation: subtle sine wave pulse
- Shadow: offset (2px right, 2px down)
- Navigation: Up/Down arrow keys or W/S
- Select: Enter key
`

---

### Track Selection Visual Layout
`
╔══════════════════════════════════════════════════════════════╗
║  [X]                                                      [>]║
║                                                              ║
║       ┌─────────────────────────────────────┐               ║
║       │                                     │               ║
║       │      [TRACK TILE PREVIEW]          │               ║
║       │      Grid 8x8 squares               │               ║
║       │      (Color or Desaturated)         │               ║
║       │                                     │               ║
║       │                                     │               ║
║       └─────────────────────────────────────┘               ║
║                                                              ║
║                  ★★★★★☆☆☆☆☆                           ║
║                  (Star Rating - Yellow/Grey)                ║
║                                                              ║
║                    TRACK NAME                               ║
║                    (30px glyph)                             ║
║                                                              ║
║              Laps: 5                                         ║
║              Length: 1234 meters                            ║
║              Lap Record: 01:23.456                          ║
║              Race Record: 12:34.567                         ║
║                                                              ║
║              [Or lock icon if locked]                       ║
║              "Finish in TOP-6 to unlock!"                   ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝

Layout Properties:
- Centered item (width/2, height/2)
- Item width: 1/2 screen, Item height: 1/2 screen
- Tiles: maintain square aspect, centered
- Title: centered below tiles, 30px glyph
- Stars: centered below title (10 stars across)
- Properties: right-aligned beneath stars
- Lock icon: centered on item
- Animation: slide in from edges (1000px distance)
- Navigation: Left/Right arrows for tracks
- Select: Enter key to start race
`

---

### Settings Menu Visual Layout
`
╔══════════════════════════════════════════════════════════════╗
║  [X]                                                         ║
║                                                              ║
║                    GAME MODE >                              ║
║                    (One Player / Two Player / Time Trial)   ║
║                                                              ║
║                    SOUNDS >                                 ║
║                    (On / Off)                               ║
║                                                              ║
║                      GFX >                                  ║
║                    (Resolution / FPS / VSync / Split)       ║
║                                                              ║
║                    CONTROLS >                               ║
║                    (Key configuration)                      ║
║                                                              ║
║                      RESET >                                ║
║                    (Times / Positions / Tracks)             ║
║                                                              ║
║                                                              ║
║              [Item height: 1/10 screen]                     ║
║              [Glyph size: 20px]                            ║
║                                                              ║
║              [Navigation: Up/Down arrows]                   ║
║              [Right arrow: open submenu]                    ║
║              [Left arrow/Esc: return]                       ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝

Layout Properties:
- Full width items
- Item height: 1/10 screen
- 5 items in reversed order (bottom to top)
- Glyph size: 20px
- ">" indicator suggests submenu
- Focus: YELLOW with animation
- Slide in: from right (pushEnter)
- Slide out: to left (pushExit)
`

---

### Difficulty Menu Visual Layout
`
╔══════════════════════════════════════════════════════════════╗
║  [X]                                                         ║
║                                                              ║
║                      HARD                                   ║
║                                                              ║
║                     MEDIUM                                  ║
║                                                              ║
║                      EASY                                   ║
║                                                              ║
║              [Item height: 1/8 screen]                     ║
║              [Glyph size: 40px]                            ║
║              [Centered, vertical list]                      ║
║                                                              ║
║              [Selection auto-defaults to]                   ║
║              [current difficulty setting]                   ║
║                                                              ║
║              [Select item → Lap Count menu]                ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
`

---

### Lap Count Menu Visual Layout
`
╔══════════════════════════════════════════════════════════════╗
║  [X]              Choose lap count              [<]  [>]    ║
║                                                              ║
║           1        3        5        10                      ║
║                                                              ║
║           20       50       100                              ║
║                                                              ║
║         [Item width: ~1/9 screen]                           ║
║         [Item height: ~1/9 screen]                          ║
║         [Horizontal list layout]                            ║
║         [Glyph size: 40px]                                 ║
║         [Centered, bold]                                    ║
║                                                              ║
║         [Title: 30px glyph, centered]                       ║
║         [Navigation: Left/Right arrows]                     ║
║         [Select item → Track Selection menu]                ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
`

---

### Confirmation Dialog Visual Layout
`
╔══════════════════════════════════════════════════════════════╗
║                                                              ║
║                  Are you sure?                              ║
║                  (CYAN text, 20px glyph)                    ║
║                                                              ║
║                   OK          CANCEL                        ║
║                  (Yellow)     (White)                       ║
║                 (20px glyph) (20px glyph)                   ║
║                                                              ║
║              [Horizontal layout]                            ║
║              [Item width: 1/4 screen each]                  ║
║              [Item height: full screen]                     ║
║              [Default focus: CANCEL (right)]                ║
║              [Left/Right arrows to switch]                  ║
║              [Enter to confirm]                             ║
║                                                              ║
╚══════════════════════════════════════════════════════════════╝
`

---

## ANIMATION CURVES

### Text Item Animation (Sine Wave)
`
       ┌─────────┐
       │         │
    ┌──┘         └──┐
    │               │
────┘               └────

Animation:
- Angle increments: 0.010 radians/frame
- Size variance: ±5% of base size
- Formula: size = baseSize + sin(angle) * baseSize * 0.05
- On focus: multiply by 1.25 (25% enlargement)
- Creates subtle "breathing" effect while focused
`

### Position Animation (Cubic Easing)
`
Start (x0, y0)  ────────► Target (x1, y1)

Interpolation over 15 frames:
Frame 0:   0.0000  (start)
Frame 1:   0.0007
Frame 2:   0.0056
...
Frame 14:  0.9526
Frame 15:  1.0000  (end)

Cubic curve creates acceleration/deceleration effect
`

### Track Selection Slide Animation
`
Previous track             New track
(center)                   (off-screen right)
   |                          |
   |  Slide out left          |  Slide in from right
   |  1000px distance         |  1000px distance
   ↓                          ↓
   └──────────────────────────→

Duration: 15 frames (cubic easing)
Both tracks visible during transition
`

---

## COLOR STATE TRANSITIONS

### Text Item State Machine
`
NORMAL STATE:
  Color: WHITE (1.0, 1.0, 1.0, 1.0)
  Scale: 1.0x
  │
  ├─→ On focus/selection ──→ FOCUSED STATE:
      │                       Color: YELLOW (1.0, 1.0, 0.0, 1.0)
      │                       Scale: 1.25x (with animation pulse)
      │                       Animation: sine wave ±5%
      │
      └─→ On release ──→ SELECTED STATE:
                          Color: RED (1.0, 0.0, 0.0, 1.0)
                          Scale: 1.0x

Track Selection Stars:
UNEARNED:
  Color: GREY (0.75, 0.75, 0.75, 1.0)
  │
  └─→ If best_position ≤ star_number ──→ EARNED:
                                          Color: YELLOW (1.0, 1.0, 0.0, 1.0)
                                          With glow overlay

Track Tiles:
LOCKED:
  Color: GREY (0.5, 0.5, 0.5, 1.0)  [50% desaturated]
  Icon: Lock symbol centered
  │
  └─→ After unlock ──→ UNLOCKED:
                        Color: WHITE (1.0, 1.0, 1.0, 1.0)
                        No lock icon
`

---

## SHADOW & DEPTH EFFECTS

### Text Shadow System
`
Rendered text at (x, y)
Rendered shadow at (x + 2px, y - 2px)

Shadow offset = (2, -2) consistently across all menus
Creates subtle depth/elevation effect

Shadow rendering:
- Offset from primary text
- Typically darker/dimmed color
- Improves readability on varied backgrounds
`

### Menu Background Overlay
`
Original background:
  Full brightness

Overlay applied:
  RGB(0.5, 0.5, 0.5) × background
  Result: 50% brightness reduction
  Creates darkened "focus" effect on menu

Effect:
┌──────────────────────────┐
│   Dimmed Background      │
│  ┌────────────────────┐  │
│  │ Full Brightness    │  │
│  │   Menu Items       │  │
│  │ (Readable)         │  │
│  └────────────────────┘  │
└──────────────────────────┘
`

---

## MOUSE ITEM POSITIONING

### Quit Button [X]
`
Location: Top-right corner
Size: 40x40 pixels
Glyph: "X"
Behavior: Click to close menu

Position: (x + width - 40, y + 40)
`

### Previous Button [<]
`
Location: Left side (center Y)
Size: 40x40 pixels
Glyph: "<"
Behavior: Click for previous item/page

Used in: Track selection, horizontal menus
Position: (x + 20, y + height/2 - 20)
`

### Next Button [>]
`
Location: Right side (center Y)
Size: 40x40 pixels
Glyph: ">"
Behavior: Click for next item/page

Used in: Track selection, horizontal menus
Position: (x + width - 60, y + height/2 - 20)
`

---

## DYNAMIC TEXT SIZING

### Height-Based Scaling
`
// Credits and Help menus scale with screen height
glyphSize = 20 * (screenHeight / 640)

Examples:
- 640px height:   20px * (640/640) = 20px
- 720px height:   20px * (720/640) = 22.5px
- 1080px height:  20px * (1080/640) = 33.75px
- 1440px height:  20px * (1440/640) = 45px

Maintains readable proportions across resolutions
`

### Fixed Sizing (Most Menus)
`
Main menu:       40px
Settings:        20px
Track details:   20px
Titles/labels:   30px
Buttons:         20px

Consistent sizing regardless of screen height
`

---

## LAYOUT GRID SYSTEM

### Main/Settings Menu Grid (Vertical)
`
┌────────────────────────────────────┐
│ (1/8 screen height)                │
│ ITEM 1                             │
├────────────────────────────────────┤
│ (1/8 screen height)                │
│ ITEM 2                             │
├────────────────────────────────────┤
│ (1/8 screen height)                │
│ ITEM 3                             │
├────────────────────────────────────┤
│ (1/8 screen height)                │
│ ITEM 4                             │
├────────────────────────────────────┤
│ (1/8 screen height)                │
│ ITEM 5                             │
└────────────────────────────────────┘

Settings uses: 1/10 screen height instead
`

### Track Selection Grid (Centered)
`
┌────────────────────────────────────┐
│  (margin)                   [X]    │
│  ┌─────────────────────────┐       │
│  │  (1/2 screen width)     │       │
│  │  (1/2 screen height)    │       │
│  │  [TRACK ITEM]           │       │
│  │  Centered               │       │
│  └─────────────────────────┘       │
│  (margin)                          │
└────────────────────────────────────┘
`

### Lap Count Grid (Horizontal)
`
┌─────────────────────────────────────┐
│ (margin)        Title        (margin)│
│ [<] [1] [3] [5] [10] [20] [50] [100] [>]
│                                     │
│ Each item: ~1/9 screen width/height │
│ All centered horizontally           │
└─────────────────────────────────────┘
`

---

## FOCUS INDICATION METHODS

### Primary: Color Change
`
Normal   → White (1.0, 1.0, 1.0, 1.0)
Focused  → Yellow (1.0, 1.0, 0.0, 1.0)
Selected → Red (1.0, 0.0, 0.0, 1.0)
`

### Secondary: Size Animation
`
Normal   → 1.0x scale
Focused  → 1.25x scale
         + sine wave pulse (±5%)
`

### Tertiary: Shadow Enhancement
`
All items render with consistent (2, -2) shadow
Shadow makes text "pop" from background
More visible when item is focused
`

---

## SCREEN TRANSITION EFFECTS

### Menu Push (Enter Submenu)
`
1. Current menu slides left
   Position: (0, 0) → (-width, 0) over 15 frames

2. New menu slides in from right
   Position: (width, 0) → (0, 0) over 15 frames

3. Both animations simultaneous (parallel)
   Cubic easing (exp=3)
   Duration: ~250ms at 60fps
`

### Menu Pop (Return from Submenu)
`
1. Current menu slides right
   Position: (0, 0) → (width, 0) over 15 frames

2. Previous menu slides in from left
   Position: (-width, 0) → (0, 0) over 15 frames

3. Both animations simultaneous (parallel)
   Cubic easing (exp=3)
   Duration: ~250ms at 60fps
`

---

## TEXT ALIGNMENT & POSITIONING

### Horizontal Alignment
`
Centered:   x = container.x + (container.width - text.width) / 2
Left:       x = container.x + margin
Right:      x = container.x + container.width - margin - text.width
`

### Vertical Alignment
`
Centered:   y = container.y + (container.height - text.height) / 2
Top:        y = container.y + margin
Bottom:     y = container.y + container.height - margin - text.height
`

### Track Item Specific
`
Title:      centered below tile preview
            y = center.y + height/2 + text.height

Stars:      centered above title
            y = center.y - height/2 + star.height/2

Properties: right-aligned below stars
            x = center.x
            y = center.y - height/2 (and descending)
`

---

## PERFORMANCE & RENDERING NOTES

### Cached Rendering
- Fonts cached via MCAssetManager
- Surfaces loaded once, reused
- Shader program "menu" compiled once

### Per-Frame Updates
- Position animations interpolate each frame
- Text angle animation updates each frame
- Color state changes immediate on focus change
- No texture regeneration needed

### Memory Optimization
- All menus pre-allocated in MenuManager
- No dynamic menu creation during gameplay
- MenuStack uses vector of pointers
- AnimationCurve uses pre-calculated table

---

## RESPONSIVE DESIGN

### Resolution Scaling
`
Items sized as screen fraction:
- Item height = screen_height / constant
- Item width = screen_width / constant

Text sizes:
- Fixed for most menus (40, 20 pixels)
- Scaled for Help/Credits (20 * height/640)

Maintains aspect ratio across resolutions
`

---

## ACCESSIBILITY FEATURES

### Keyboard Navigation
- Arrow keys: directional movement
- Enter: select/confirm
- Esc/Q: exit menu
- Tab: cycle focus (if implemented)

### Visual Feedback
- Color changes (white → yellow → red)
- Size animations (1.0x → 1.25x)
- Shadow effects for depth
- Glow effects on stars

### Mouse Support
- Click items to focus/select
- Click mouse buttons (Quit, Prev, Next)
- Hover effects with focus indication

---

## END OF VISUAL & LAYOUT SPECIFICATION
