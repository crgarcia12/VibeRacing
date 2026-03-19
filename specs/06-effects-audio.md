# DustRacing2D - Effects & Audio Specification

## 1. PARTICLE EFFECTS SYSTEM

### 1.1 Particle Types & Factory

The `ParticleFactory` is a singleton managing 9 particle types with pre-allocated object pools:

| Particle Type     | Pre-allocated | Surface Asset | Shadow | Alpha Blend |
|-------------------|--------------|---------------|--------|-------------|
| DamageSmoke       | 500          | "smoke"       | No     | Yes         |
| SkidSmoke         | 500          | "smoke"       | No     | Yes         |
| Smoke             | 500          | "smoke"       | No     | Yes         |
| OffTrackSmoke     | 500          | "smoke"       | No     | Yes         |
| OnTrackSkidMark   | 500          | "skid"        | No     | Yes         |
| OffTrackSkidMark  | 500          | "skid"        | No     | Yes         |
| Sparkle           | 500          | "sparkle"     | No     | Yes         |
| Leaf              | 100          | "leaf"        | Yes    | No          |
| Mud               | 500          | "mud"         | Yes    | No          |

### 1.2 Detailed Particle Parameters

#### DamageSmoke
- Z offset: +10 units
- Initial Size: 12
- Lifetime: 3000 ms
- Color: RGBA(0.1, 0.1, 0.1, 0.25) — dark gray, semi-transparent
- Animation: FadeOutAndExpand
- Rotation: Random 0–360°
- Velocity: car velocity + random Z-axis * 0.2
- Trigger: Car damage ≤ 0.3 (70%+ damage)

#### SkidSmoke
- Z offset: +5 units
- Initial Size: 6
- Lifetime: 3000 ms
- Color: RGBA(1.0, 1.0, 1.0, 0.1) — white, very transparent
- Animation: FadeOutAndExpand
- Velocity: car velocity * 0.25 + random Z * 0.1
- Trigger: Braking/skidding on track, speed 5–200 km/h

#### Smoke (Generic)
- Z offset: +10 units
- Initial Size: 12
- Lifetime: 3000 ms
- Color: RGBA(0.75, 0.75, 0.75, 0.15) — light gray
- Animation: FadeOutAndExpand
- Velocity: car velocity + random Z * 0.1

#### OffTrackSmoke (Dirt cloud)
- Z offset: +10 units
- Initial Size: 15
- Lifetime: 3000 ms
- Color: RGBA(0.6, 0.4, 0.0, 0.25) — brown/tan
- Animation: FadeOut
- Emission Rate: Every 2 frames
- Trigger: Car wheels off-track at speed ≥ 5 km/h

#### OnTrackSkidMark
- Z offset: +1 unit
- Initial Size: 8
- Lifetime: 50000 ms (effectively permanent)
- Color: RGBA(0.1, 0.1, 0.1, 0.25) — dark gray
- Animation: FadeOut (very slow)
- Velocity: Zero (stationary)
- Density: 1 mark per 8 pixels of travel
- Speed range: 5–200 km/h

#### OffTrackSkidMark
- Z offset: +1 unit
- Initial Size: 8
- Lifetime: 50000 ms
- Color: RGBA(0.2, 0.1, 0.0, 0.25) — darker brown
- Animation: FadeOut
- Velocity: Zero (stationary)
- Min speed: 5 km/h

#### Sparkle (Collision sparks)
- Z offset: at contact point
- Initial Size: 2 + random * 2 (range 2–4)
- Lifetime: 1500 ms
- Color: RGBA(1.0, 1.0, 1.0, 0.33) — white, semi-transparent
- Animation: Shrink
- Velocity: car velocity * (0.75–1.0 random) + Z+4
- Acceleration: world gravity * 0.5
- Emission Count: 10 particles per collision
- Trigger: Car-to-car or hard object collisions

#### Leaf (Tree collision)
- Z offset: at contact point
- Initial Size: 5
- Lifetime: 3000 ms
- Color: RGBA(0.0, 0.75, 0.0, 0.75) — green
- Animation: Shrink
- Rotation: Random 0–360°, angular velocity = (random - 0.5) * 5.0 rad/s
- Velocity: car velocity * 0.1 + Z+2 + random 3D * 0.5
- Acceleration: Z-9.8 (gravity)
- Emission Count: 1 per tree collision

#### Mud (Off-track spray)
- Z offset: none
- Initial Size: 12
- Lifetime: 3000 ms
- Color: RGBA(1.0, 1.0, 1.0, 0.5) — white, semi-transparent
- Animation: Shrink
- Velocity: car velocity * 0.5 + Z+4
- Acceleration: world gravity
- Emission Rate: Every 5 frames
- Min speed: 15 km/h (higher threshold)

### 1.3 Skid Mark Constants
- SKID_MARK_DENSITY = 8 pixels
- NEW_SKID_LIMIT = 32 pixels (= SKID_MARK_DENSITY * 4)
- Below 32px: interpolated angle from previous position
- Above 32px: current car angle

### 1.4 Speed Thresholds
| Effect           | Min Speed | Max Speed |
|-----------------|-----------|-----------|
| SkidSmoke        | 5 km/h    | 200 km/h  |
| OnTrackSkidMark  | 5 km/h    | 200 km/h  |
| OffTrackSkidMark | 5 km/h    | —         |
| OffTrackSmoke    | 5 km/h    | —         |
| Mud              | 15 km/h   | —         |
| DamageSmoke      | —         | —         |

---

## 2. SOUND EFFECTS SYSTEM

### 2.1 Architecture
`CarSoundEffectManager` extends `AudioSource`. Uses OpenAL with OGG and WAV support.

### 2.2 Per-Car Sound Handles
```
engineSoundHandle  — continuous looping engine
hitSoundHandle     — car collision impact
skidSoundHandle    — tire skid/sliding
```

### 2.3 Engine Sound — Dynamic Pitch
- virtualRev = speed * 50
- Gear ratios: [1.0, 0.8, 0.6, 0.5, 0.4, 0.3] (6 gears)
- effRev = virtualRev * gearRatios[gear]
- pitch = 1.0 + effRev / 5000
- Upshift trigger: effRev > 3000 RPM
- Downshift trigger: effRev < 1000 RPM
- RPM range: 0–15000

### 2.4 Skid Sound
- Trigger: car.isSliding() == true
- Cooldown timer: 100 ms

### 2.5 Collision Sound Categories
| Collision With            | Sound Handle |
|--------------------------|--------------|
| Car-to-car               | hitSoundHandle |
| Grandstand, Tree, Rock   | hitSoundHandle |
| Wall, Bridge Rail, WallLong | "carHit2"  |
| Banner, Brake, Crate, Plant, Tire | "carHit3" |

- Collision detection threshold: relative velocity > 4.0 units/sec
- Collision cooldown: 500 ms

### 2.6 Audio Files
Located in `src/game/audio/`:
- `audiosource.cpp/hpp` — base audio emitter
- `audioworker.cpp/hpp` — audio processing thread
- `openaldata.cpp/hpp` — OpenAL data management
- `openaldevice.cpp/hpp` — OpenAL device interface
- `openaloggdata.cpp/hpp` — OGG/Vorbis handler
- `openalsource.cpp/hpp` — individual sound source
- `openalwavdata.cpp/hpp` — WAV handler

---

## 3. TREE RENDERING

### 3.1 Physics
- Collision shape: Circle, radius = 8 units
- Mass: 1.0, Restitution: 0.25
- Type ID: "tree"

### 3.2 Branch Generation
- Root (i=0): surface "treeRoot", has shadow "branchShadowEnabled"
- Upper branches: surface "treeBranch", random 0–360° rotation, no shadow
- Position: Z = branchHeight * (i + 1)
- branchHeight = treeHeight / branches
- Scale taper: scale = r0 - (r0 - r1) / branches * i  (linear r0→r1)
- Upper branch XY offset: random 2D * 5 units

---

## 4. CHECKERED FLAG

- Surface asset: "checkeredFlag"
- Flag width: 32 px, height: 24 px
- Vertical spacing from bottom: 20 px
- Position: (width/2, height - 24/2 - 20) — center-bottom of screen
- Rendering: static overlay, no animation, 0° rotation

---

## 5. INTRO SEQUENCE

- Background surface: "intro"
- Shader: "menu"
- Background fill color: RGBA(1.0, 1.0, 1.0, 1.0) — white
- Background scaled to fit screen width, aspect ratio preserved
- Version text: "v" + VERSION_STRING
- Version text position: top-left, offset by glyph height
- Version text glyph size: 20 px wide, height/32 px tall
- Version text color: RGB(0.75, 0.75, 0.75) — light gray
- Font: MCTextureFont (scalable)

---

## 6. FADE ANIMATION SYSTEM

### 6.1 Fade In
- `beginFadeIn(preDelayMSec, msec, postDelayMSec)`
- fadeValue: 0.0 → 1.0
- step = 1.0 / (msec / updateFps)
- Signals: `fadeValueChanged(float)`, `fadeInFinished()`

### 6.2 Fade Out
- `beginFadeOut(preDelayMSec, msec, postDelayMSec)`
- fadeValue: 1.0 → 0.0
- step = 1.0 / (msec / updateFps)
- Signals: `fadeValueChanged(float)`, `fadeOutFinished()`

### 6.3 Fade Out Flash
- `beginFadeOutFlash(preDelayMSec, msec, postDelayMSec)`
- Starts at fadeValue = 10.0 (super-bright flash), decays to 0.0
- step = 10.0 / (msec / updateFps)

### 6.4 Timing Constants
| Constant              | Value   |
|-----------------------|---------|
| Default Update FPS    | 60      |
| Timer interval        | 16.67 ms |
| Hit sound cooldown    | 500 ms  |
| Skid sound cooldown   | 100 ms  |
| Mud emission counter  | 5 frames |
| Smoke emission counter| 2 frames |
