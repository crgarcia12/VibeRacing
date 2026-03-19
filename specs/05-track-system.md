# **DUST RACING 2D - COMPREHENSIVE TECHNICAL SPECIFICATION**

## **Document Overview**
This specification documents the core AI, Route, Input Handling, and Event Management systems of the Dust Racing 2D C++ game engine. All constants, thresholds, angles, distances, and control parameters used in AI decisions are enumerated below.

---

## **1. AI SYSTEM (ai.hpp / ai.cpp)**

### **1.1 Class Architecture**

**Class: AI**
- **Purpose**: Implements artificial intelligence for computer-controlled cars
- **File Location**: `src/game/ai.hpp` / `src/game/ai.cpp`

**Member Variables**:
- `Car& m_car` - Reference to the car controlled by this AI
- `std::shared_ptr<Race> m_race` - Reference to the current race
- `std::shared_ptr<Track> m_track` - Reference to the current track
- `float m_lastDiff` - Previous angle difference for PID control (used in steering)
- `size_t m_lastTargetNodeIndex` - Index of last processed target node
- `MCVector2dF m_randomTolerance` - Random offset applied to target coordinates to introduce variation

**Constructor**: 
```cpp
AI(Car& car, std::shared_ptr<Race> race)
```

### **1.2 Update Loop**

**Method**: `void update(bool isRaceCompleted)`
- **Called**: Every frame
- **Parameters**: `isRaceCompleted` - Boolean flag indicating if the race lap is complete
- **Logic Flow**:
  1. Checks if track is set
  2. Detects when target node changes → triggers `setRandomTolerance()` to introduce AI variation
  3. Calls `steerControl()` with current target node from race
  4. Calls `speedControl()` based on current track tile and race completion state
  5. Updates `m_lastTargetNodeIndex` for next frame comparison

### **1.3 Steering Control (AI::steerControl)**

**Method Signature**: `void steerControl(TargetNodeBasePtr targetNode)`

**Algorithm**:
1. **Target Calculation**:
   - Converts target node location to 3D float coordinates
   - Applies random tolerance offset: `target -= MCVector3dF(m_car.location() + MCVector3dF(m_randomTolerance))`
   - Random tolerance is computed as: `MCRandom::randomVector2d() * TrackTileBase::width() / 8`

2. **Angle Computation**:
   - Calculates target angle: `angle = atan2(target.y, target.x)` (converted to degrees)
   - Current car angle: `cur = (int)m_car.angle() % 360`
   - Angle difference: `diff = angle - cur`

3. **Angle Normalization** (Loop to ensure -180 ≤ diff ≤ 180):
   - If `diff > 180`: `diff -= 360`
   - If `diff < -180`: `diff += 360`

4. **PID Controller Implementation**:
   ```
   control = diff * 0.025 + (diff - m_lastDiff) * 0.025
   ```
   - **Proportional gain**: 0.025 (acts on current angle difference)
   - **Derivative gain**: 0.025 (acts on rate of change)
   - Note: Derivative term prevents overshooting

5. **Control Signal Limiting**:
   - Initial magnitude: `control = |control|`
   - Maximum control value: **1.5** (capped)
   - Steering threshold: **maxDelta = 3.0 degrees**

6. **Steering Application**:
   - If `diff < -3.0`: `m_car.steer(Car::Steer::Right, control)` (turn right)
   - If `diff > 3.0`: `m_car.steer(Car::Steer::Left, control)` (turn left)
   - Otherwise: No steering applied (within threshold)

7. **State Persistence**:
   - `m_lastDiff = diff` (stored for next frame's derivative term)

**Key Constants**:
| Parameter | Value | Description |
|-----------|-------|-------------|
| Proportional Gain | 0.025 | P term of PID controller |
| Derivative Gain | 0.025 | D term of PID controller |
| Max Control | 1.5 | Maximum steering force applied |
| Steering Dead Zone | ±3.0° | Angle difference threshold below which no steering is applied |
| Random Tolerance Scale | TrackTileBase::width() / 8 | Multiplier for random offset in target position |

### **1.4 Speed Control (AI::speedControl)**

**Method Signature**: `void speedControl(TrackTile& currentTile, bool isRaceCompleted)`

**Algorithm**:
1. **Initialization**:
   - `accelerate = true` (default to accelerating)
   - `brake = false`
   - `absSpeed = m_car.absSpeed()`
   - `scale = 0.9` (multiplier applied to all speed thresholds)

2. **Brake Hint Handling** (`TrackTile::ComputerHint::Brake`):
   - If `absSpeed > 14.0 * 0.9 = 12.6`: Set `brake = true`

3. **Hard Brake Hint Handling** (`TrackTile::ComputerHint::BrakeHard`):
   - If `absSpeed > 9.5 * 0.9 = 8.55`: Set `brake = true`

4. **Corner Detection** (90° corners: `TrackTile::TileType::Corner90`):
   - If `absSpeed > 7.0 * 0.9 = 6.3`: Set `accelerate = false`

5. **Corner Detection** (45° corners: `TrackTile::TileType::Corner45Left` or `TrackTile::TileType::Corner45Right`):
   - If `absSpeed > 8.3 * 0.9 = 7.47`: Set `accelerate = false`

6. **Cool-Down Lap** (when `isRaceCompleted == true`):
   - If `absSpeed > 5.0`: Set `accelerate = false`
   - No acceleration phase, just deceleration to cool down

7. **Acceleration Phase** (when `isRaceCompleted == false`):
   - If `absSpeed < 3.6 * 0.9 = 3.24`: 
     - Force `accelerate = true`
     - Force `brake = false`

8. **Control Application**:
   - If `brake == true`: 
     - `m_car.setAcceleratorEnabled(false)`
     - `m_car.setBrakeEnabled(true)`
   - Else if `accelerate == true`:
     - `m_car.setAcceleratorEnabled(true)`
     - `m_car.setBrakeEnabled(false)`
   - Else:
     - Both disabled (coasting)

**Speed Thresholds**:
| Condition | Threshold | Description |
|-----------|-----------|-------------|
| Brake Hint | 12.6 units | Speed above which braking is applied on brake hints |
| Hard Brake Hint | 8.55 units | Speed above which braking is applied on hard brake hints |
| 90° Corner | 6.3 units | Speed above which acceleration stops in sharp corners |
| 45° Corner | 7.47 units | Speed above which acceleration stops in gentle corners |
| Cool-Down Threshold | 5.0 units | Absolute speed for cool-down lap deceleration |
| Minimum Acceleration Speed | 3.24 units | Speed below which forced acceleration occurs |
| Scale Factor | 0.9 | Reduces AI max speeds (perceived as "stupid" per code comments) |

### **1.5 Random Tolerance Mechanism**

**Method**: `void setRandomTolerance()`

**Purpose**: Adds realistic variation to AI steering by randomizing the target point slightly

**Calculation**:
```cpp
m_randomTolerance = MCRandom::randomVector2d() * TrackTileBase::width() / 8
```

- **Trigger**: Called whenever `getCurrentTargetNodeIndex()` changes (i.e., new waypoint reached)
- **Randomness**: 2D random vector with magnitude scaled to `TrackTileBase::width() / 8`
- **Effect**: Moves the effective target node by up to ±(TrackTileBase::width() / 8) units in random directions
- **Recomputation**: Fresh random tolerance generated every time a new target node is reached

---

## **2. ROUTE SYSTEM (route.hpp / route.cpp)**

### **2.1 Class Architecture**

**Class: Route**
- **Purpose**: Defines the racing route as an ordered sequence of target waypoints
- **File Location**: `src/common/route.hpp` / `src/common/route.cpp`
- **Data Structure**: `std::vector<TargetNodeBasePtr> m_route`

**Typedef**: 
```cpp
typedef std::vector<TargetNodeBasePtr> RouteVector
```

### **2.2 Route Construction**

**Method**: `bool push(TargetNodeBasePtr node)`
- **Purpose**: Add a target node to the route sequentially
- **Logic**:
  1. Sets node index: `node->setIndex(static_cast<int>(m_route.size()))`
  2. Appends to route vector: `m_route.push_back(node)`
  3. Checks if route is closed: `return isClosed()`
- **Returns**: `true` if the route forms a closed loop

**Method**: `bool isClosed() const`
- **Purpose**: Determine if route forms a closed racing loop
- **Validation**:
  - Requires at least 2 nodes: `m_route.size() > 1`
  - Calculates difference between first and last node:
    - `dx = |m_route[0]->x() - m_route.back()->x()|`
    - `dy = |m_route[0]->y() - m_route.back()->y()|`
  - **Closure Threshold**: `32 units` (pixels)
  - Returns: `(dx < 32) && (dy < 32)`

**Method**: `void buildFromVector(RouteVector& routeVector)`
- **Purpose**: Construct route from unordered vector of target nodes
- **Algorithm**:
  1. Clear existing route: `clear()`
  2. Sort nodes by index: `std::sort(..., [](lhs, rhs) { return lhs->index() < rhs->index(); })`
  3. Filter and add valid nodes:
     - Only nodes with `index >= 0` are included
     - Non-null nodes only
  4. Assign sequential indices via `push()` method

### **2.3 Route Access Methods**

| Method | Return Type | Description |
|--------|-------------|-------------|
| `get(size_t index)` | `TargetNodeBasePtr` | Get target node at given index (asserts bounds) |
| `numNodes()` | `size_t` | Return number of waypoints in route |
| `getAll(RouteVector& vec)` | `void` | Copy all nodes to provided vector |
| `begin()` / `end()` | `Iterator` | Standard iterator access |
| `cbegin()` / `cend()` | `ConstIterator` | Const iterator access |
| `clear()` | `void` | Empty the route |

### **2.4 Route Geometry**

**Method**: `double geometricLength() const`
- **Purpose**: Calculate total distance traveled if following the route
- **Calculation**:
  ```
  length = sum of distances between consecutive nodes + distance from last to first
  distance = sqrt((dx)² + (dy)²)
  ```
- **Logic**:
  1. If fewer than 2 nodes: return 0
  2. Loop through consecutive node pairs: calculate Euclidean distance
  3. Add closing distance (last node back to first node)
- **Return Type**: `double` (high precision)

---

## **3. TARGET NODE BASE SYSTEM (targetnodebase.hpp / targetnodebase.cpp)**

### **3.1 Class Architecture**

**Class: TargetNodeBase**
- **Purpose**: Base class for freely placeable target waypoints in the editor
- **File Location**: `src/common/targetnodebase.hpp` / `src/common/targetnodebase.cpp`
- **Use Case**: Defines race route waypoints, collectibles, checkpoints, etc.

**Typedef**: 
```cpp
using TargetNodeBasePtr = std::shared_ptr<TargetNodeBase>
```

### **3.2 Member Variables**

| Member | Type | Description |
|--------|------|-------------|
| `m_location` | `QPointF` | World coordinates (x, y) |
| `m_size` | `QSizeF` | Size in pixels (derived from track tile) |
| `m_index` | `int` | Position in route sequence (-1 = uninitialized) |
| `m_next` | `TargetNodeBasePtr` | Pointer to next waypoint |
| `m_prev` | `TargetNodeBasePtr` | Pointer to previous waypoint |

### **3.3 Initialization**

**Constructor**: `TargetNodeBase()`
```cpp
m_size = QSize(TrackTileBase::height(), TrackTileBase::width())
m_index = -1
m_next = nullptr
m_prev = nullptr
```
- **Default Size**: Initialized to track tile dimensions
- **Default Index**: -1 (indicates node not yet assigned to route)

### **3.4 Core Accessors**

| Method | Signature | Purpose |
|--------|-----------|---------|
| `location()` | `QPointF` | Get world coordinates |
| `setLocation(QPointF)` | `void` | Set world coordinates |
| `index()` | `int` | Get route sequence index |
| `setIndex(int)` | `void` | Set route sequence index |
| `size()` | `QSizeF` | Get bounding size |
| `setSize(QSizeF)` | `void` | Set bounding size |
| `next()` | `TargetNodeBasePtr` | Get next node pointer |
| `setNext(TargetNodeBasePtr)` | `void` | Set next node pointer |
| `prev()` | `TargetNodeBasePtr` | Get previous node pointer |
| `setPrev(TargetNodeBasePtr)` | `void` | Set previous node pointer |

---

## **4. INPUT HANDLING SYSTEM (inputhandler.hpp / inputhandler.cpp)**

### **4.1 Class Architecture**

**Class: InputHandler**
- **Purpose**: Centralized handler for player input across multiple players
- **File Location**: `src/game/inputhandler.hpp` / `src/game/inputhandler.cpp`

### **4.2 Action Enumeration**

**Enum: InputHandler::Action**
```cpp
enum class Action : int {
    Left = 0,      // Car steering left / menu left
    Right,         // Car steering right / menu right  
    Up,            // Car accelerate / menu up
    Down,          // Car brake / menu down
    EndOfEnum      // Sentinel value (= 4)
}
```

### **4.3 Data Structure**

**Internal Storage**:
```cpp
typedef std::vector<std::bitset<static_cast<int>(Action::EndOfEnum)>> ActionVector
ActionVector m_playerActions  // Size = number of players, each holds 4 bits
```

- **Multi-Player Support**: Up to `maxPlayers` (typically 2)
- **Bit Representation**: Each action stored as single bit (true/false)
- **Memory Efficient**: Bitset provides compact boolean array

### **4.4 Constructor**

**Method**: `InputHandler(size_t maxPlayers)`
- **Purpose**: Initialize input handler for specified number of players
- **Initialization**:
  ```cpp
  m_playerActions(maxPlayers, std::bitset<4>())  // All bits = false
  ```

### **4.5 Core Methods**

| Method | Signature | Purpose |
|--------|-----------|---------|
| `setActionState(playerIdx, action, state)` | `void` | Set action pressed/released for player |
| `getActionState(playerIdx, action)` | `bool` | Query if action is pressed for player |
| `reset()` | `void` | Clear all player actions |
| `setEnabled(bool)` | `void static` | Enable/disable input globally |
| `enabled()` | `bool static` | Check if input is enabled |

### **4.6 Gating Logic**

**In getActionState()**:
```cpp
return InputHandler::m_enabled && m_playerActions[playerIndex][action]
```
- Input must be **globally enabled** AND action must be **pressed**
- Global enable allows pause/menu to disable all controls simultaneously

---

## **5. EVENT HANDLING SYSTEM (eventhandler.hpp / eventhandler.cpp)**

### **5.1 Class Architecture**

**Class: EventHandler (extends QObject)**
- **Purpose**: Converts OS-level events (keyboard, mouse) to game actions
- **File Location**: `src/game/eventhandler.hpp` / `src/game/eventhandler.cpp`
- **Framework**: Qt-based (uses QKeyEvent, QMouseEvent, QTimer)

### **5.2 Inner Class: ActionMapping**

```cpp
class ActionMapping {
    int m_player;                    // Player index (0 or 1)
    InputHandler::Action m_action;   // Action type (Up/Down/Left/Right)
    
    ActionMapping(int player, InputHandler::Action action);
    int player() const;
    InputHandler::Action action() const;
}
```
- **Purpose**: Maps keyboard keys to player actions
- **Usage**: Stored in `m_keyToActionMap`

### **5.3 Default Key Bindings**

**Initial Mappings** (hardcoded in constructor):

**Player 0 (First Player)**:
| Key | KeyCode | Scan Code | Action | Native Code |
|-----|---------|-----------|--------|-------------|
| Left Arrow | Qt::Key_Left | - | Left | - |
| Right Arrow | Qt::Key_Right | - | Right | - |
| Up Arrow | Qt::Key_Up | - | Up | - |
| Down Arrow | Qt::Key_Down | - | Down | - |
| Right Shift | KeyCodes::RSHIFT | - | Up | 62 |
| Right Ctrl | KeyCodes::RCTRL | - | Down | 105 |

**Player 1 (Second Player)**:
| Key | KeyCode | Scan Code | Action | Native Code |
|-----|---------|-----------|--------|-------------|
| W | Qt::Key_W | - | Up | - |
| A | Qt::Key_A | - | Left | - |
| S | Qt::Key_S | - | Down | - |
| D | Qt::Key_D | - | Right | - |
| Left Shift | KeyCodes::LSHIFT | - | Up | 50 |
| Left Ctrl | KeyCodes::LCTRL | - | Down | 37 |

### **5.4 Key Codes Enumeration**

**Namespace: KeyCodes** (keycodes.hpp)

| Constant | USB Scan Code | Description |
|----------|---------------|-------------|
| `LSHIFT` | 50 | Left Shift key |
| `RSHIFT` | 62 | Right Shift key |
| `LCTRL` | 37 | Left Control key |
| `RCTRL` | 105 | Right Control key |
| `LALT` | 64 | Left Alt key |
| `RALT` | 92 | Right Alt key |

### **5.5 Key Event Handling**

**Method**: `bool handleKeyPressEvent(QKeyEvent* event)`
- **Logic**:
  - If in Game state: calls `handleGameKeyPressEvent()`
  - If in Menu state: calls `handleMenuKeyPressEvent()`

**Method**: `bool handleGameKeyPressEvent(QKeyEvent* event)`
1. Calls `applyMatchingAction(event, true)` (press = true)
2. Special keys (always active):
   - `Qt::Key_Escape` or `Qt::Key_Q`: Quit game
   - `Qt::Key_P`: Toggle pause

**Method**: `bool handleGameKeyReleaseEvent(QKeyEvent* event)`
1. Calls `applyMatchingAction(event, false)` (press = false)

**Method**: `bool applyMatchingAction(QKeyEvent* event, bool press)`

**Algorithm**:
1. Ignores auto-repeat events: `if (!event->isAutoRepeat())`
2. Looks up key mapping by two methods (in priority order):
   - **First**: By native scan code: `event->nativeScanCode()`
   - **Second**: By Qt key code: `event->key()`
3. If found:
   - Extracts player and action from mapping
   - Calls: `m_inputHandler.setActionState(player, action, press)`
   - Returns `true` (event consumed)
4. If not found and press == true:
   - Checks for special keys (Escape, Q, P)
   - Otherwise returns `false` (event not consumed)

### **5.6 Menu Key Handling**

**Method**: `bool handleMenuKeyPressEvent(QKeyEvent* event)`

**Two Modes**:

**A) Capture Mode** (`m_captureMode == true`)
- Purpose: Allow user to remap keys
- Logic:
  1. Get native scan code from event
  2. Call `mapKeyToAction(m_capturePlayer, m_captureAction, scanCode)`
  3. If successful: disable capture mode and pop menu
  4. Key is remapped and saved to settings

**B) Normal Mode** (`m_captureMode == false`)
- Navigation:
  - `Qt::Key_Left`: Menu left + "menuClick" sound
  - `Qt::Key_Right`: Menu right + "menuClick" sound
  - `Qt::Key_Up`: Menu up + "menuClick" sound
  - `Qt::Key_Down`: Menu down + "menuClick" sound
- Selection:
  - `Qt::Key_Return` or `Qt::Key_Enter`: Select item
    - If menu done: "menuBoom" sound
    - Otherwise: "menuClick" sound
- Exit:
  - `Qt::Key_Escape` or `Qt::Key_Q`: Exit menu
    - Emits `gameExited()` if completely done
    - Emits "menuClick" sound

### **5.7 Mouse Event Handling**

**Method**: `bool handleMousePressEvent(QMouseEvent* event, int screenWidth, int screenHeight, bool mirrorY)`
- Only active in menu state
- Converts screen coordinates (with optional Y-axis mirroring)
- Delegates to `MTFH::MenuManager::instance().mousePress()`
- Emits "menuClick" sound on successful click

**Method**: `bool handleMouseReleaseEvent(QMouseEvent* event, int screenWidth, int screenHeight, bool mirrorY)`
- Only active in menu state
- Converts screen coordinates
- Delegates to `MTFH::MenuManager::instance().mouseRelease()`
- Emits "menuBoom" sound if menu is done

**Method**: `bool handleMouseMoveEvent(QMouseEvent* event)`
- Restarts cursor visibility timer (3000ms)
- Emits `cursorRevealed()` signal
- Always returns `true`

### **5.8 Key Remapping System**

**Method**: `bool mapKeyToAction(int player, InputHandler::Action action, int key)`

**Preconditions**:
- `key != 0` (valid key code)
- `key != Qt::Key_Escape` (reserved)
- `key != Qt::Key_Q` (reserved)
- `key != Qt::Key_P` (reserved)

**Algorithm**:
1. Iterate through existing mappings
2. Find any existing mapping for this `player` + `action` pair
3. Remove old mapping if found
4. Insert new mapping: `m_keyToActionMap[key] = { player, action }`
5. Save to persistent settings: `Settings::instance().saveKeyMapping(player, action, key)`
6. Return `true` if successful

**Validation**: Returns `false` if key is reserved or invalid (0)

### **5.9 Key Mapping Storage**

**Data Structure**:
```cpp
typedef std::map<int, ActionMapping> KeyToActionMap
KeyToActionMap m_keyToActionMap  // Maps key code → player/action pair
```

- **Key**: Key code (native scan code or Qt key code)
- **Value**: ActionMapping (player index + action type)
- **Persistence**: Loaded/saved via Settings singleton

### **5.10 UI State Management**

**Capture Mode Variables**:
- `bool m_captureMode` - Whether waiting for key input to remap
- `InputHandler::Action m_captureAction` - Which action to remap
- `int m_capturePlayer` - Which player (0 or 1)

**Method**: `void enableCaptureMode(InputHandler::Action action, int player)`
- Assertion: `player == 0 || player == 1`
- Sets capture mode flags
- UI prompts user to press a key

**Method**: `void disableCaptureMode()`
- Clears `m_captureMode` flag
- Exits key capture state

### **5.11 Cursor Management**

**Timer**: `QTimer m_mouseCursorTimer`
- **Interval**: 3000 ms (3 seconds)
- **Single Shot**: Yes (fires once, then stops)
- **Behavior**: 
  - Restarts on mouse move
  - When timer expires: emits `cursorHid()` signal
  - Hides mouse cursor after 3 seconds of inactivity

### **5.12 Signals (Qt Events)**

| Signal | Parameters | Purpose |
|--------|------------|---------|
| `pauseToggled()` | None | User pressed P key |
| `gameExited()` | None | User exited from menu |
| `soundRequested(QString, bool)` | handle, loop | Request sound effect or music |
| `cursorRevealed()` | None | Mouse moved (show cursor) |
| `cursorHid()` | None | Cursor inactivity timeout (hide cursor) |

---

## **6. CONTROL FLOW INTEGRATION**

### **6.1 AI Update Pipeline**

```
Game Loop (each frame)
  ├─ EventHandler processes OS events
  │  └─ Updates InputHandler with player actions
  ├─ AI::update(isRaceCompleted)
  │  ├─ Check for target node change
  │  │  └─ If changed: setRandomTolerance()
  │  ├─ steerControl(targetNode)
  │  │  ├─ Calculate angle to target
  │  │  ├─ Apply PID steering (gains: 0.025 / 0.025)
  │  │  └─ Apply steering to car (max control: 1.5)
  │  └─ speedControl(currentTile, isRaceCompleted)
  │     ├─ Check tile hints (Brake / BrakeHard)
  │     ├─ Check corner types (Corner90 / Corner45)
  │     └─ Apply acceleration/brake commands
  └─ Car physics update
     └─ Apply steering and acceleration
```

### **6.2 Input Path (Player to Car)**

```
Keyboard/Gamepad Event
  ├─ EventHandler.handleKeyPressEvent()
  │  └─ applyMatchingAction(event, true)
  │     └─ m_inputHandler.setActionState(player, action, true)
  ├─ InputHandler stores action state
  └─ Car reads action state each frame
     └─ Player controls car directly (no AI involved)
```

### **6.3 Route Progression**

```
Race Start
  ├─ Track.trackData().route() initialized
  ├─ Route.push() called for each target node → assigns indices
  ├─ Route.isClosed() validates racing loop (threshold: 32 units)
  └─ Race tracks currentTargetNodeIndex for each car
  
During Race
  ├─ Car crosses target node
  │  └─ Race increments getCurrentTargetNodeIndex()
  ├─ AI detects index change
  │  ├─ Calls setRandomTolerance()
  │  └─ Steer toward new target node
  └─ Repeat until lap complete
```

---

## **7. CRITICAL CONSTANTS SUMMARY TABLE**

| Category | Constant | Value | Units | Usage |
|----------|----------|-------|-------|-------|
| **AI Steering** | Proportional Gain | 0.025 | - | PID P-term |
| | Derivative Gain | 0.025 | - | PID D-term |
| | Max Control Signal | 1.5 | - | Maximum steering force |
| | Steering Dead Zone | ±3.0 | degrees | Below which no steering applied |
| **AI Speed** | Brake Hint Threshold | 12.6 | units | Speed to trigger brake hint |
| | Hard Brake Threshold | 8.55 | units | Speed to trigger hard brake |
| | Corner90 Threshold | 6.3 | units | Speed limit in 90° corners |
| | Corner45 Threshold | 7.47 | units | Speed limit in 45° corners |
| | Cool-Down Threshold | 5.0 | units | Deceleration after race finish |
| | Min Acceleration Speed | 3.24 | units | Speed below which forced acceleration |
| | Speed Scale Factor | 0.9 | - | Multiplier reducing AI max speeds |
| **Route** | Closure Threshold | 32 | units | Distance to detect route closure |
| **Random Tolerance** | Scale Base | TrackTileBase::width()/8 | units | Random offset magnitude |
| **Cursor** | Inactivity Timer | 3000 | ms | Time before cursor auto-hide |
| **Key Codes** | LSHIFT | 50 | - | USB scan code |
| | RSHIFT | 62 | - | USB scan code |
| | LCTRL | 37 | - | USB scan code |
| | RCTRL | 105 | - | USB scan code |
| | LALT | 64 | - | USB scan code |
| | RALT | 92 | - | USB scan code |

---

## **8. IMPLEMENTATION NOTES**

### **8.1 AI Characteristics**
- **Deterministic**: PID-based steering ensures consistent behavior
- **Reactive**: Responds to current target node and track conditions
- **Fallible**: Reduced speed (0.9 scale) and random tolerance make AI beatable
- **Smooth**: Derivative term in PID prevents jerky steering
- **Tile-Aware**: Respects track tile hints for intelligent braking

### **8.2 Route System Properties**
- **Order-Dependent**: Route indices critical for progression
- **Cyclic**: First and last nodes must be within 32 units to form closed loop
- **Geometric**: Length calculation enables statistics/analytics
- **Flexible**: buildFromVector() allows unordered construction

### **8.3 Input Architecture**
- **Multi-Player**: Supports 2 simultaneous players with separate key bindings
- **Remappable**: All game keys can be reassigned and persisted
- **Hierarchical**: Menu vs. Game key handling distinct
- **Pausable**: Global enable/disable prevents all input during pause

### **8.4 Performance Considerations**
- AI update O(1) per car
- Route access O(1) (vector-based indexing)
- Input lookup O(log n) for map-based key search
- No dynamic allocations in update loops

---

**END OF SPECIFICATION**
