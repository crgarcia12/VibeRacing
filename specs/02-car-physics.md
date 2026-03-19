---

## **2. PHYSICS CONSTANTS & PARAMETERS**

### **2.1 Force & Acceleration Parameters**

| Parameter | Value | Unit | Description |
|-----------|-------|------|-------------|
| **Default Engine Power** | 200,000 | Watts | Standard power for human/first car |
| **Default Drag (Quadratic)** | 2.5 | Dimensionless | Quadratic drag coefficient |
| **Max Force (Acceleration)** | mass × 0.75 × gravity | N | Peak acceleration force |
| **Sliding Detection Threshold** | 7.5 | Speed Units | Min speed to detect sliding |
| **Sliding Angle Threshold** | 0.25 | Normalized Vector | Max normal component for slide |

### **2.2 Speed Conversion & Calculation**

| Formula | Value | Notes |
|---------|-------|-------|
| **Speed to km/h** | absSpeed × 3.6 × 2.75 | Internal conversion multiplier: 9.9x |
| **Min Acceleration Velocity** | 0.001 | Speed threshold for acceleration calc |
| **Min Tire Velocity** | 0.1 | Threshold for tire physics updates |
| **Max Tire Spin Velocity** | 4.5 | Upper bound for spin calculation |

### **2.3 Tire Spin Effect System**

**Spin Effect Activation Conditions:**
- Accelerator enabled AND
- Gear = Forward AND
- Velocity > 0.1 speed units AND
- Velocity < 4.5 speed units AND
- Player is human (AI disabled)

**Spin Coefficient Calculation:**
```
spinCoeff = 0.025 + 0.975 * (velocity / 4.5)^2
Range: 0.025 (min) to 1.0 (max)
```

**Skidding Flag:** Set to true during spin effect activation

### **2.4 Speed-Based Features**

| Feature | Threshold | Condition |
|---------|-----------|-----------|
| **Brake Light Visibility** | > 0 km/h | Shown when braking AND speed > 0 |
| **Tire Reverse Logic** | < 25 km/h | Speed < 25 reverses direction when braking |
| **Pit Stop Trigger** | < 25 km/h | Car stops at pit when speed drops below |
| **Hard Crash Detection** | ≥ 3.5 | Damage threshold for hard crash flag |

---

## **3. TIRE PHYSICS SYSTEM**

### **3.1 Tire Configuration**

**Four Independent Tires:**
- Front Left Tire
- Front Right Tire
- Rear Left Tire
- Rear Right Tire

**Surface Asset:** All tires use "frontTire" surface

### **3.2 Tire Friction Parameters**

| Tire Position | Track Friction | Off-Track Friction | Off-Track Factor |
|----------------|-----------------|-------------------|------------------|
| **Front** | 0.85 | 0.68 (0.85 × 0.8) | 0.8 |
| **Rear** | 0.95 | 0.76 (0.95 × 0.8) | 0.8 |

**Calculation:** `offTrackFriction = onTrackFriction × 0.8`

### **3.3 Tire Force Physics**

**Lateral Friction Force:**
```cpp
MCVector2dF tire = (cos(angle+90°), sin(angle+90°))  // Tire normal
MCVector2dF v = velocity.normalized()
MCVector2dF projection = projection(v, tire)
impulse = projection × friction × spinCoeff × gravity × mass
```

**Force Clamping:**
```cpp
maxImpulse = mass × 7.0 × tireWearFactor
impulse.clamp(maxImpulse)
```

**Braking Force (when isBraking = true):**
```cpp
brakingImpulse = v × 0.5 × friction × gravity × mass × tireWearFactor
Applied parallel to tire motion
```

### **3.4 Tire State Variables**

| Variable | Type | Range | Default | Purpose |
|----------|------|-------|---------|---------|
| m_isOffTrack | bool | true/false | false | Track surface detection |
| m_friction | float | 0-1 | 0.85/0.95 | Normal friction coefficient |
| m_offTrackFriction | float | 0-1 | 0.68/0.76 | Dirt friction coefficient |
| m_spinCoeff | float | 0.025-1.0 | 1.0 | Tire spin multiplier |

### **3.5 Velocity Clamping**

- **Velocity normalization:** Clamped to 0.999 instead of fully normalized to avoid numerical artifacts

---

## **4. GEARBOX LOGIC**

### **4.1 Gear States**

```cpp
enum class Gear {
    Neutral,    // No gear engaged
    Forward,    // Driving forward
    Reverse,    // Driving backward
    Stop        // Stopped (transitional state)
};
```

### **4.2 Gearbox State Machine**

**Gearbox Update Logic (based on `speedInKmh` and control inputs):**

| Condition | Result Gear | Notes |
|-----------|-------------|-------|
| **Both accelerator AND brake** | Neutral | Competing inputs → neutral |
| **Brake enabled (no accelerator)** | Stop/Reverse | See reverse transition below |
| **Accelerator enabled (no brake)** | Forward (if not moving) | Engages forward from neutral/stop |
| **Neither accelerator nor brake** | Neutral | Default state |

**Reverse Gear Transition:**
- When braking (speedInKmh == 0):
  1. First frame: Shift to `Stop` gear, set `m_stopCounter = 0`
  2. Subsequent frames: Increment `m_stopCounter`
  3. After counter > 30 (~500ms): Shift to `Reverse` gear

**Stop Counter Timing:** ~0.5 seconds delay before reverse engagement

### **4.3 Gearbox Member Variables**

| Variable | Type | Default | Purpose |
|----------|------|---------|---------|
| m_gear | Gear | Neutral | Current gear state |
| m_acceleratorEnabled | bool | false | Accelerator input flag |
| m_brakeEnabled | bool | false | Brake input flag |
| m_stopCounter | int | 0 | Counter for reverse delay |

---

## **5. CAR FACTORY & INSTANTIATION**

### **5.1 Car Creation Function**

```cpp
std::unique_ptr<Car> CarFactory::buildCar(size_t index, size_t carCount, Game & game)
```

**Parameters:**
- `index` - Car number (0 to carCount-1)
- `carCount` - Total number of cars in race
- `game` - Game instance reference

### **5.2 Power Distribution (Computer Players)**

**Human Player (index 0 or index 1 with 2-player mode):**
- Power: 200,000 W (constant)
- Drag: 2.5 (constant)
- Acceleration Friction: `0.55 × difficultyProfile.accelerationFrictionMultiplier(true)`

**Computer Players:**
- Power formula: `200,000 / 2 + (index + 1) × 200,000 / carCount`
- Acceleration Friction: `(0.3 + 0.4 × (index + 1) / carCount) × accelerationFrictionMultiplier(false)`
- Drag: 2.5 (constant)

**Power Distribution Example (8 cars):**
| Car # | Type | Power Formula | Power (W) |
|-------|------|---------------|-----------|
| 0 | Human | 200,000 | 200,000 |
| 1 | AI | 100k + 1×25k = | 125,000 |
| 2 | AI | 100k + 2×25k = | 150,000 |
| 3 | AI | 100k + 3×25k = | 175,000 |
| 4 | AI | 100k + 4×25k = | 200,000 |
| 5 | AI | 100k + 5×25k = | 225,000 |
| 6 | AI | 100k + 6×25k = | 250,000 |
| 7 | AI | 100k + 7×25k = | 275,000 |

**Result:** Variance from 0.625× to 1.375× human power

### **5.3 Car Selection Logic**

1. **Player 1 (index 0):** Always human-controlled
2. **Player 2 (index 1):** Human if `game.hasTwoHumanPlayers()`
3. **Indices 2+:** Computer-controlled if `game.hasComputerPlayers()`
4. **Returns null** if no valid car type

### **5.4 Color Assignment Strategy**

Colors are assigned sequentially from the `carImageMap` (12 colors), with fallback to "carYellow" if index > 11.        

---

## **6. BRIDGE MECHANICS**

### **6.1 Bridge Structure**

**Bridge Components:**
- Main bridge body (from mesh object "bridge")
- Two rails (from "wallLong" surface)
- Under-bridge trigger zone

### **6.2 Rail Configuration**

| Rail Parameter | Value | Unit | Notes |
|---|---|---|---|
| **Rail Z-Height** | 16 | Units | Constant RAIL_Z value |
| **Rail Y-Displacement** | 42% of TrackTile height | Calculated | `(TrackTile::height() × 42) / 100` |
| **Left Rail Offset** | -railYDisplacement | Units | Negative Y |
| **Right Rail Offset** | +railYDisplacement | Units | Positive Y |
| **Surface Material** | asphalt | Surface ID | Bridge deck surface |
| **Restitution** | 0.9 | Elasticity | High bounce coefficient |

### **6.3 Bridge Collision System**

**Static Maps (Class-level):**
```cpp
static ObjectStatusMap m_onBridgeStatusMap      // Objects on top of bridge
static ObjectStatusMap m_underBridgeStatusMap    // Objects under bridge
```

**Object Status Map Type:** `std::map<MCObject *, std::set<Bridge *>>`

### **6.4 Bridge Physics Behavior**

**When Car Collides with Bridge (Top Surface):**
1. Check if object NOT under bridge
2. Add bridge to `m_onBridgeStatusMap[object]`
3. Set collision layer to `Layers::Collision::BridgeRails`
4. Raise object: `z = bridge.location.z + RAIL_Z (16)`
5. Log: "Raising object"

**When Car Separates from Bridge:**
1. Remove bridge from `m_onBridgeStatusMap[object]`
2. If no bridges remain in map:
   - Set collision layer to 0 (default)
   - Disable sleep prevention
   - Lower object: `z = 0`
   - Log: "Lowering object"

**When Car Under Bridge (Trigger Zone):**
1. Check if object NOT on bridge (m_onBridgeStatusMap)
2. Add bridge to `m_underBridgeStatusMap[object]`
3. Log: "Object under bridge"

**When Car Exits Bridge Trigger:**
1. Remove bridge from `m_underBridgeStatusMap[object]`
2. Log: "Object not under bridge"

### **6.5 Under-Bridge Trigger Zone**

**Trigger Dimensions:**
```cpp
width = TrackTile::width() - TrackTile::width() / 8
height = TrackTile::height() + TrackTile::height() / 8
```

**Trigger Properties:**
- Collision layer: -1 (trigger layer)
- Physics object: false
- Trigger object: true
- Mass: 0 (stationary)

### **6.6 Bridge State Reset**

```cpp
void Bridge::reset() {
    m_onBridgeStatusMap.clear()
    m_underBridgeStatusMap.clear()
}
```

---

## **7. PIT STOP SYSTEM**

### **7.1 Pit Object Properties**

| Property | Value | Notes |
|----------|-------|-------|
| **Physics Object** | false | Static pit zone |
| **Trigger Object** | true | Collision trigger |
| **Mass** | 1 (stationary) | Immovable |
| **Shadow** | Disabled | No shadow rendering |
| **Surface** | Configurable | Passed via constructor |

### **7.2 Pit Detection Logic**

**Pit Stop Activation Sequence:**

1. **Collision Detection:**
   - Monitor only **human player** cars
   - Store cars in `m_possiblyPittingCars` set

2. **Speed Check (onStepTime):**
   - Each frame, check speed of cars in pit zone
   - Trigger condition: `speedInKmh < 25 km/h`

3. **Pit Stop Signal:**
   - Emit Qt signal: `pitStop(Car & car)`
   - Remove car from tracking set
   - Car is now considered "in pit"

### **7.3 Pit State Management**

```cpp
std::set<Car *> m_possiblyPittingCars  // Actively monitored cars
```

**Collision Event Handler:**
```cpp
if (event.collidingObject().typeId() == MCObject::typeId("car"))
    if (Car& car = dynamic_cast; car.isHuman())
        m_possiblyPittingCars.insert(&car)
```

**Separation Event Handler:**
```cpp
if (event.separatedObject().typeId() == MCObject::typeId("car"))
    if (Car& car = dynamic_cast; car.isHuman())
        m_possiblyPittingCars.erase(&car)
```

**OnStepTime Update:**
```cpp
for each car in m_possiblyPittingCars:
    if car.speedInKmh() < 25:
        emit pitStop(car)
        erase car from set
```

### **7.4 Pit Reset**

```cpp
void Pit::reset() {
    m_possiblyPittingCars.clear()
}
```

---

## **8. OFF-TRACK DETECTION**

### **8.1 Detection System Overview**

**Detector Class:** `OffTrackDetector`
- Monitors **front tires only** (left and right)
- Updates track reference dynamically
- Manages tile boundary calculations

### **8.2 Tile Boundary Limits**

**Tile Limit Calculations:**
```cpp
m_tileWLimit = TrackTile::width() / 2 - TrackTile::width() / 10
             = 0.4 × TrackTile::width()

m_tileHLimit = TrackTile::height() / 2 - TrackTile::height() / 10
              = 0.4 × TrackTile::height()
```

**Boundary Reduction:** 10% inset from each tile edge

### **8.3 Off-Track Detection by Tile Type**

**Algorithm Overview:**
```cpp
bool isOffTrack(MCVector2dF tire, const TrackTile& tile)
```

#### **8.3.1 Non-Asphalt Tiles**
- **Condition:** `!tile.hasAsphalt()`
- **Result:** Immediately return `true` (off-track)

#### **8.3.2 Straight Tracks (0° or 180°)**
- **Tile Types:** `Straight`, `Finish`
- **Rotation Check:** `(tile.rotation() + 90) % 180 == 0` (vertical orientation)
  - Off-track if: `tire.y > tile.y + m_tileHLimit || tire.y < tile.y - m_tileHLimit`
- **Rotation Check:** `tile.rotation() % 180 == 0` (horizontal orientation)
  - Off-track if: `tire.x > tile.x + m_tileWLimit || tire.x < tile.x - m_tileWLimit`

#### **8.3.3 45° Corner (Male)**
- **Tile Type:** `Straight45Male`
- **Calculation:**
  ```cpp
  diff = tire - tile.location
  rotatedDiff = rotatedVector(diff, tile.rotation - 45)
  Off-track if: rotatedDiff.y > m_tileHLimit || rotatedDiff.y < -m_tileHLimit
  ```

#### **8.3.4 45° Corner (Female)**
- **Tile Type:** `Straight45Female`
- **Calculation:**
  ```cpp
  diff = tire - tile.location
  rotatedDiff = rotatedVector(diff, 360 - tile.rotation - 45)
  Off-track if: rotatedDiff.y < m_tileHLimit  (lower boundary only)
  ```

#### **8.3.5 90° Corners**
- **Tile Type:** `Corner90`
- **Method:** Circular annulus approximation
- **Calculation:**
  ```cpp
  rotatedCorner = tile.location + rotatedVector(
    (TrackTileBase::width()/2, TrackTileBase::height()/2),
    tile.rotation + 270
  )
  diff = tire - rotatedCorner
  r1 = TrackTileBase::width() / 10          (inner radius)
  r2 = TrackTileBase::width() - width/20    (outer radius)

  Off-track if: diff.lengthSquared() < r1² OR diff.lengthSquared() > r2²
  ```

**90° Corner Radius Detail:**
- Inner radius: 10% of track width
- Outer radius: 95% of track width

### **8.4 Off-Track State Management**

**Car Properties Updated:**
```cpp
void Car::setLeftSideOffTrack(bool state)   // Left tire status
void Car::setRightSideOffTrack(bool state)  // Right tire status
```

**Composite States:**
```cpp
bool Car::isOffTrack() {
    return leftSideOffTrack() || rightSideOffTrack()
}
```

### **8.5 Integration with Tire Physics**

**Off-Track Effects:**
1. Friction multiplier: 0.8× (reduces grip)
2. Tire wear rate: +10% factor
3. Visual feedback: Tire marked as off-track for rendering
4. Braking wear: +5% factor while braking and accelerating

---

## **9. DAMAGE & WEAR SYSTEMS**

### **9.1 Damage System**

#### **9.1.1 Damage Capacity**

| Property | Initial Value | Min | Max |
|----------|---------------|-----|-----|
| **m_damageCapacity** | 100 | 0 | 100 |
| **m_initDamageCapacity** | 100 | - | - |

#### **9.1.2 Collision Damage Calculation**

**Source:** `CarPhysicsComponent::addImpulse()`

```cpp
if (Game::instance().difficultyProfile().hasBodyDamage() && isCollision) {
    float damageMultiplier = car.isHuman() ? 0.5 : 0.25
    damage = damageMultiplier × impulse.lengthFast()
    car.addDamage(damage)
}
```

**Damage Factors:**
- Human players: 0.5× damage multiplier (2× damage)
- AI players: 0.25× damage multiplier (4× resistance)

#### **9.1.3 Hard Crash Detection**

- **Damage Threshold:** ≥ 3.5 units
- **Flag:** `m_hadHardCrash` set to true
- **Query Function:** `hadHardCrash()` returns true once then resets flag

#### **9.1.4 Damage Factor Application**

**Damage affects acceleration:**
```cpp
damageFactor() = 0.7 + (damageCapacity / initialCapacity) × 0.3
                = 0.7 to 1.0  (0% = 70% power, 100% = 100% power)

appliedForce = baseForce × damageFactor()
```

**Damage Levels:**
- No damage (100% capacity): 1.0× power multiplier
- 50% damage (50% capacity): 0.85× power multiplier
- Total damage (0% capacity): 0.7× power multiplier

#### **9.1.5 Damage State Functions**

```cpp
float damageFactor() const      // Current power multiplier (0.7-1.0)
float damageLevel() const       // Normalized 0-1 (1.0 = no damage, 0.0 = total)
bool hasDamage() const          // true if capacity < 100
void addDamage(float damage)    // Decrease capacity, minimum 0
void resetDamage()              // Restore capacity to 100
```

### **9.2 Tire Wear System**

#### **9.2.1 Tire Wear Capacity**

| Property | Initial Value | Min | Max |
|----------|---------------|-----|-----|
| **m_tireWearOutCapacity** | 100 | 0 | 100 |
| **m_initTireWearOutCapacity** | 100 | - | - |

#### **9.2.2 Tire Wear Factors**

**Wear Rate Multipliers (applied when conditions met):**

| Condition | Wear Factor | Notes |
|-----------|------------|-------|
| **Braking + Accelerating** | 0.05 | 5% per physics step |
| **Off-track (left/right)** | 0.10 | 10% per physics step |
| **Normal driving** | 0.0 | No wear |

**Wear Calculation:**
```cpp
wearOut = velocity.lengthFast() × step(ms) × factor / 1000
Capacity decreased by: wearOut
```

#### **9.2.3 Tire Wear Conditions**

Tire wear only applied if:
- `game.difficultyProfile().hasTireWearOut()` is enabled
- Car is human-controlled

#### **9.2.4 Tire Wear Factor (Physics Multiplier)**

**Affects tire grip force clamping:**
```cpp
tireWearFactor() = 0.5 + (tireWearOutCapacity × 0.5 / initialCapacity)
                 = 0.5 to 1.0  (0% = 50% grip, 100% = 100% grip)
```

**Applied to:**
- Tire impulse clamping: `maxImpulse = mass × 7.0 × tireWearFactor()`
- Braking force: `brakingImpulse × tireWearFactor()`

#### **9.2.5 Tire Wear State Functions**

```cpp
float tireWearFactor() const    // Physics multiplier (0.5-1.0)
float tireWearLevel() const     // Normalized 0-1 (1.0 = no wear)
bool hasTireWear() const        // true if capacity < 100
void resetTireWear()            // Restore capacity to 100
```

### **9.3 Combined Damage & Wear Effects**

**Progressive Performance Degradation:**

| Damage/Wear Level | Acceleration | Grip | Combined Effect |
|-------------------|------------------|------|-------------------|
| 100% / 100% | 1.0× | 1.0× | Optimal |
| 75% / 75% | 0.925× | 0.875× | Slightly slower |
| 50% / 50% | 0.85× | 0.75× | Noticeably slower |
| 25% / 25% | 0.775× | 0.625× | Very slow |
| 0% / 0% | 0.7× | 0.5× | Severely degraded |

---

## **APPENDIX: KEY ENUMS & TYPES**

### **A.1 Steering Enum**
```cpp
enum class Steer {
    Neutral,  // No steering input
    Left,     // Steer left
    Right     // Steer right
};
```

### **A.2 Gearbox Enum**
```cpp
enum class Gear {
    Neutral,  // Neutral - no acceleration
    Forward,  // Forward gear
    Reverse,  // Reverse gear
    Stop      // Transitional state before reverse
};
```

### **A.3 TrackTile Type Enum**
```cpp
enum class TileType {
    Straight,           // Horizontal/vertical straight
    Finish,             // Finish line (straight variant)
    Straight45Male,     // 45° corner (convex)
    Straight45Female,   // 45° corner (concave)
    Corner90            // 90° corner (circular approximation)
};
```

---

## **SUMMARY TABLE: ALL NUMERIC CONSTANTS**

| Constant | Value | Unit | Category |
|----------|-------|------|----------|
| Front tire friction | 0.85 | - | Tire Physics |
| Rear tire friction | 0.95 | - | Tire Physics |
| Off-track friction factor | 0.8 | - | Tire Physics |
| Max steering angle | 15.0 | degrees | Car Control |
| Tire steering offset | 5.0 | degrees | Car Animation |
| Max tire spin velocity | 4.5 | speed units | Tire Physics |
| Min tire spin velocity | 0.1 | speed units | Tire Physics |
| Tire clamp force | 7.0 | × mass | Tire Physics |
| Braking force factor | 0.5 | - | Tire Physics |
| Damage multiplier (human) | 0.5 | - | Damage |
| Damage multiplier (AI) | 0.25 | - | Damage |
| Hard crash threshold | 3.5 | impulse units | Damage |
| Damage factor range | 0.7-1.0 | - | Damage |
| Tire wear (braking+accel) | 0.05 | per step | Tire Wear |
| Tire wear (off-track) | 0.10 | per step | Tire Wear |
| Tire wear factor range | 0.5-1.0 | - | Tire Wear |
| Pit stop speed threshold | 25 | km/h | Pit System |
| Reverse delay counter | 30 | frames (~500ms) | Gearbox |
| Speed conversion | 3.6 × 2.75 | multiplier | Speed |
| Default car mass | 1500 | kg | Physics |
| Default car power | 5000 | W (base) | Physics |
| Default restitution | 0.05 | - | Physics |
| Default rolling friction | 0.1 | - | Physics |
| Default acceleration friction | 0.75 | - | Physics |
| MoI multiplier | 2.5 | × mass | Physics |
| Factory default power | 200,000 | W | Car Factory |
| Factory default drag | 2.5 | - | Car Factory |
| Bridge rail Z-height | 16 | units | Bridge |
| Rail Y-displacement | 42% | of tile height | Bridge |
| Bridge restitution | 0.9 | - | Bridge |
| Corner 90 inner radius | width/10 | - | Off-Track |
| Corner 90 outer radius | width - width/20 | - | Off-Track |
| Tile boundary inset | 10% | of dimension | Off-Track |
| Under-bridge trigger height | height × 1.125 | relative | Bridge |
| Under-bridge trigger width | width × 0.875 | relative | Bridge |
| Speed to KmH multiplier | 9.9 | final | Speed |
| Sliding speed threshold | 7.5 | speed units | Physics |
| Sliding angle threshold | 0.25 | normalized | Physics |
| Human steering smoothing | 0.15 | factor | Car Control |
| Velocity clamp (not normalize) | 0.999 | - | Tire Physics |

---

**END OF SPECIFICATION DOCUMENT**

This comprehensive reverse-engineering specification covers all aspects of the DustRacing2D game physics system, includi
ing exact numeric constants, physics formulas, state machines, and behavioral mechanics. All values are extracted directl
ly from the source code with full traceability.
