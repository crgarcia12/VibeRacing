---

## **3. STATE MACHINE**

### 3.1 Game States (StateMachine::State enum)

| State | Purpose | Transitions |
|-------|---------|-------------|
| **Init** | Initialization | → DoIntro |
| **DoIntro** | Intro sequence (3000ms) | → Menu |
| **Menu** | Main menu | → MenuTransitionOut |
| **MenuTransitionIn** | Fade-in (2000ms, 0 delay) | → Menu |
| **MenuTransitionOut** | Fade-out flash (2000ms) | → GameTransitionIn |
| **GameTransitionIn** | Race fade-in (2000ms) | → DoStartlights |
| **GameTransitionOut** | Race fade-out (2000ms or 10000ms+10000ms) | → MenuTransitionIn |
| **DoStartlights** | Start light animation | → Play |
| **Play** | Active racing | → GameTransitionOut (on finish) |

### 3.2 Fade Parameters
Format: `(initial_color, duration_ms, delay_ms)`

**MenuTransitionIn**:
- `fadeInRequested(0, 2000, 0)`

**MenuTransitionOut**:
- `fadeOutFlashRequested(0, 2000, 0)` (flash effect)

**GameTransitionIn**:
- `fadeInRequested(0, 2000, 0)`

**GameTransitionOut**:
- If race finished: `fadeOutRequested(10000, 10000, 0)` (10sec fade + 10sec delay)
- Otherwise: `fadeOutRequested(0, 2000, 0)` (normal 2sec fade)

### 3.3 State Machine Signals
- `fadeInRequested(colorValue, durationMs, delayMs)`
- `fadeOutRequested(colorValue, durationMs, delayMs)`
- `fadeOutFlashRequested(colorValue, durationMs, delayMs)`
- `soundsStopped()`
- `startlightAnimationRequested()`
- `renderingEnabled(bool)`
- `exitGameRequested()`

### 3.4 State Callbacks
- `endFadeIn()`: Transitions based on current state
- `endFadeOut()`: Transitions based on current state
- `endStartlightAnimation()`: Moves to Play state
- `finishRace()`: Moves to GameTransitionOut + sets `m_raceFinished = true`

---

## **4. RACE MANAGEMENT SYSTEM**

### 4.1 Race Initialization
```cpp
Race::Race(Game& game, size_t numCars)
  ├─ m_humanPlayerIndex1 = 0
  ├─ m_humanPlayerIndex2 = 1
  ├─ m_numCars = numCars
  ├─ m_lapCount = 5 (default)
  ├─ m_unlockLimit = 6 (cars must finish ≤6th to unlock next track)
  ├─ m_timing = std::make_shared<Timing>(numCars)
  ├─ m_offTrackMessageTimer.setInterval(30000ms) (30 sec between off-track messages)
  └─ createStartGridObjects() (one per car)
```

### 4.2 Start Grid Object Placement
Grid objects are positioned based on **finish line rotation**:

**Rotation Cases: 90° or -270° (facing right)**
- Y offset per car: `(i % 2) * tileHeight/3 - tileHeight/6`
- X offset per car: `(i/2) * spacing + (i%2) * oddOffset`
- Grid placed at: `x - gridOffset, y` (gridOffset = tileWidth/12)

**Rotation Cases: 270° or -90° (facing left)**
- Y offset per car: `(i % 2) * tileHeight/3 - tileHeight/6`
- X offset per car: `-(i/2) * spacing + (i%2) * oddOffset`
- Grid placed at: `x + gridOffset, y`

**Rotation Case: 0° (facing up)**
- X offset per car: `(i % 2) * tileWidth/3 - tileWidth/6`
- Y offset per car: `-(i/2) * spacing + (i%2) * oddOffset`
- Grid placed at: `x, y + gridOffset`

**Rotation Cases: 180° or -180° (facing down)**
- X offset per car: `(i % 2) * tileWidth/3 - tileWidth/6`
- Y offset per car: `(i/2) * spacing + (i%2) * oddOffset`
- Grid placed at: `x, y - gridOffset`

**Spacing Constants**:
- `oddOffset = TrackTile::width() / 8`
- `gridOffset = TrackTile::width() / 12`
- `spacing = 0.75 * TrackTile::width()`

**Special Logic**: Bridge avoidance - if car would spawn on bridge tile, offset further along route.

### 4.3 Car Positioning During Race

**Route Progress Tracking** (per car):
```cpp
struct CarStatus {
    size_t currentTargetNodeIndex = 0;  // Active checkpoint
    size_t nextTargetNodeIndex = 0;     // Next checkpoint index
    size_t prevTargetNodeIndex = 0;     // Previous checkpoint
    size_t routeProgression = 0;        // Overall progress
    size_t position = 1;                // Race position (1-based)
};
```

**Checkpoint Detection**:
- Tolerance (normal): `0`
- Tolerance (finish line): `TrackTile::height() / 20`
- Car must be within checkpoint bounding box + tolerance to register

### 4.4 Race Progression & Scoring

**Lap Completion Logic**:
- When car reaches checkpoint 0 AND previous checkpoint was route.numNodes()-1 → lap complete
- Only human players trigger lap records (AI excluded)
- Lap time = `current_race_time - previous_race_time`

**Position Calculation**:
1. Sort by `routeProgression` (descending)
2. For tied progression, sort by order within checkpoint

**Race Completion**:
- **OnePlayerRace**: Race ends when human player finishes all laps
- **TwoPlayerRace**: Race ends when both human players finish
- **Duel**: Race ends when both human players finish
- **TimeTrial**: Race ends when human completes all laps

**Checkered Flag Trigger**:
- Enabled when leader's lap = `lapCount - 1` (last lap)
- AND leader's checkpoint index ≥ `9 * route.numNodes() / 10` (90% progress)
- Plays "bell" sound

**Winner Finish**:
- When leader's lap == lapCount
- Plays "cheering" sound (except TimeTrial)
- Sets race completion flag for all remaining cars

### 4.5 Off-Track Detection & Messages

**Off-Track Limits**:
- `OFF_TRACK_LIMIT = 60 ticks` (1 second at 60 FPS)
- `OFF_TRACK_MESSAGE_INTERVAL = 30000ms` (30 seconds between messages)

**Messages** (randomized 50/50):
- `"You must stay on track!"`
- `"Watch your tires!"`

### 4.6 Stuck Car Detection

**Stuck Limit**: `60 * 5 = 300 ticks` (5 seconds at 60 FPS)

**Recovery**:
- Move car to previous checkpoint + random offset (±TrackTile::width()/4)
- Reset physics component
- Uses `std::mt19937` engine with `std::uniform_real_distribution`

### 4.7 Best Position Tracking & Track Unlocking

**Unlock Conditions**:
- Finish position ≤ 6 (m_unlockLimit)
- Unlocks next track in sequence
- Message: `"A new track unlocked!"`

**Best Position Save**:
- Only tracked for human players
- Saved per track, lap count, and difficulty

**Failure Message**:
- `"Better luck next time.."`

### 4.8 Pit Stop System

**Pit Stop Logic** (race.cpp):
```
if (lap > 0) {  // No pit stop on lap 0
    Message: "Pit stop!"
    Sound: "pit" at car location
    Emit: tiresChanged(car)
    resetDamage()
    resetTireWear()
}
```

---

## **5. GAME MODES**

### 5.1 Game Mode Enum (Game::Mode)
```cpp
enum class Mode {
    OnePlayerRace,   // 1 human + computer players
    TwoPlayerRace,   // 2 human + computer players
    TimeTrial,       // 1 human player, no AI
    Duel            // 2 human players, no AI
};
```

### 5.2 Mode Logic
**hasTwoHumanPlayers()**:
- Returns true for `TwoPlayerRace` or `Duel`

**hasComputerPlayers()**:
- Returns true for `TwoPlayerRace` or `OnePlayerRace`

### 5.3 Split Screen Modes
```cpp
enum class SplitType {
    Horizontal,
    Vertical
};
```

---

## **6. DIFFICULTY SYSTEM**

### 6.1 Difficulty Levels (DifficultyProfile::Difficulty)
```cpp
enum class Difficulty {
    Easy = 0,
    Medium = 1,
    Hard = 2
};
```

**Default**: `Medium`

### 6.2 Difficulty Features
**Tire Wear**: Always enabled (`true`)
**Body Damage**: Always enabled (`true`)

### 6.3 Acceleration Friction Multipliers

| Difficulty | Human | AI (Computer) | AI Formula |
|------------|-------|---------------|-----------|
| Easy | 0.70 | 0.595 | 0.70 × 0.85 |
| Medium | 0.85 | 0.7225 | 0.85 × 0.85 |
| Hard | 1.0 | 0.9 | 1.0 × 0.9 |

Formula: `acceleration_multiplier = human_multiplier * (isHuman ? 1.0 : 0.85/0.9)`

---

## **7. SETTINGS & CONFIGURATION**

### 7.1 Settings Storage (QSettings)
**Configuration Group**: `"Config"`

**Stored Settings**:
| Key | Type | Default | Getter | Setter |
|-----|------|---------|--------|--------|
| `"hRes"` | int | 0 | `loadResolution()` | `saveResolution()` |
| `"vRes"` | int | 0 | `loadResolution()` | `saveResolution()` |
| `"fullScreen"` | bool | true | `loadResolution()` | `saveResolution()` |
| `"difficulty"` | int | 0 (Easy) | `loadDifficulty()` | `saveDifficulty()` |
| `"fps"` | int | 60 | `loadValue("fps")` | (dynamic) |
| `"lapCount"` | int | 5 | `loadValue("lapCount")` | `setLapCount()` |
| `"sounds"` | bool | true | `loadValue("sounds")` | (AudioWorker) |
| `"screen"` | int | 0 | `loadValue("screen")` | `--screen` arg |
| `"vsync"` | int | 1 (on) | `loadVSync()` | `saveVSync()` |

### 7.2 Key Mapping Storage
Format: `"{ACTION_STRING}_{PLAYER}"` where:
- Actions: `"IA_UP"`, `"IA_DOWN"`, `"IA_LEFT"`, `"IA_RIGHT"`
- Player: `0` or `1`

Example: `"IA_UP_0"` for Player 0 Up action

---

## **8. DATABASE PERSISTENCE**

### 8.1 Database Location
**Path**: `QStandardPaths::AppDataLocation + "/" + Config::Game::SQLITE_DATABASE_FILE_NAME`

**Typical locations**:
- Linux: `~/.local/share/[AppName]/`
- macOS: `~/Library/Application Support/[AppName]/`
- Windows: `C:\Users\[User]\AppData\Local\[AppName]\`

### 8.2 SQLite Tables

#### **Table 1: lap_record**
```sql
CREATE TABLE lap_record (
    track_name TEXT,
    version INTEGER,
    time INTEGER
)
```
**Indexed by**: `track_name + version`
**Contains**: Fastest lap time per track
**Version**: `TRACK_SET_VERSION = 1`

#### **Table 2: race_record**
```sql
CREATE TABLE race_record (
    track_name TEXT,
    version INTEGER,
    lap_count INTEGER,
    difficulty INTEGER,
    time INTEGER
)
```
**Indexed by**: `track_name + version + lap_count + difficulty`
**Contains**: Fastest race time per track/lap/difficulty
**Version**: `TRACK_SET_VERSION = 1`

#### **Table 3: best_position**
```sql
CREATE TABLE best_position (
    track_name TEXT,
    version INTEGER,
    lap_count INTEGER,
    difficulty INTEGER,
    position INTEGER
)
```
**Indexed by**: `track_name + version + lap_count + difficulty`
**Contains**: Best finishing position (1-based)
**Version**: `TRACK_SET_VERSION = 1`

#### **Table 4: track_unlock**
```sql
CREATE TABLE track_unlock (
    track_name TEXT,
    version INTEGER,
    lap_count INTEGER,
    difficulty INTEGER
)
```
**Indexed by**: `track_name + version + lap_count + difficulty`
**Contains**: Tracks unlocked by finishing in top 6

**Version**: `TRACK_SET_VERSION = 1`

### 8.3 Thread Safety
**Mutex Lock**: `std::mutex m_mutex` (mutable, used for const methods)
All database operations are wrapped in `std::lock_guard<std::mutex>`

### 8.4 Database API

**Lap Records**:
- `saveLapRecord(track, msecs)`: INSERT or UPDATE
- `loadLapRecord(track)`: Returns `std::pair<int, bool>` (time, exists)
- `resetLapRecords()`: DELETE all

**Race Records**:
- `saveRaceRecord(track, msecs, lapCount, difficulty)`: INSERT or UPDATE
- `loadRaceRecord(track, lapCount, difficulty)`: Returns `std::pair<int, bool>`
- `resetRaceRecords()`: DELETE all

**Best Position**:
- `saveBestPos(track, pos, lapCount, difficulty)`: INSERT or UPDATE
- `loadBestPos(track, lapCount, difficulty)`: Returns `std::pair<int, bool>`
- `resetBestPos()`: DELETE all

**Track Unlock**:
- `saveTrackUnlockStatus(track, lapCount, difficulty)`: INSERT only
- `loadTrackUnlockStatus(track, lapCount, difficulty)`: Returns bool
- `resetTrackUnlockStatuses()`: DELETE all

---

## **9. APPLICATION LIFECYCLE**

### 9.1 Game Construction (game.cpp)
```
Game::Game(argc, argv)
  ├─ parseArgs() - Handle CLI args
  ├─ createRenderer() - Initialize OpenGL renderer
  ├─ Connect signals/slots
  ├─ Start QElapsedTimer
  ├─ Setup update timer (interval: updateDelay)
  └─ addTrackSearchPaths()
```

### 9.2 Initialization Flow
1. **Renderer initialized** → `Game::init()` slot triggered
2. **Audio thread** started (QThread)
3. **AudioWorker** moved to thread, `init()` and `loadSounds()` called
4. **TrackLoader::loadAssets()** - Load graphics
5. **loadTracks()** - Load track data (must load ≥1 track)
6. **initScene()** - Create Scene object, add tracks to menu
7. **start()** - Begin update loop (`m_updateTimer.start()`)

### 9.3 Main Loop
```
updateTimer fires every updateDelay (16.67ms for 60 FPS)
  └─ m_stateMachine->update()
  └─ m_scene->updateFrame()
  └─ m_scene->updateOverlays()
  └─ m_renderer->renderNow()
```

### 9.4 Pause System
**Toggle on**: `EventHandler::pauseToggled` → `togglePause()`

```
if (paused) {
    start() - Resume timer
    Log: "Game continued."
} else {
    stop() - Pause timer
    Log: "Game paused."
}
```

### 9.5 Exit Sequence
**Signal**: `exitGame()` (can come from menu or StateMachine)

```
Game::exitGame()
  ├─ stop() - Pause update timer
  ├─ m_renderer->close() - Close window
  ├─ m_audioThread->quit()
  ├─ m_audioThread->wait()
  └─ m_app.quit() - Exit application
```

### 9.6 Destruction
```
~Game()
  ├─ delete m_stateMachine
  ├─ delete m_scene
  ├─ delete m_trackLoader
  ├─ delete m_eventHandler
  ├─ delete m_inputHandler
  ├─ delete m_audioWorker
  ├─ delete m_world
  └─ delete m_renderer
```

### 9.7 Exception Handling
**Application::notify()** overrides Qt's event dispatch:
- Catches `std::exception`
- Logs: `"Initializing the game failed!"`
- Exit with `EXIT_FAILURE`

---

## **10. COMMAND-LINE ARGUMENTS**

| Argument | Type | Function |
|----------|------|----------|
| `--screen <N>` | int | Force screen index N (multi-display) |
| `--lang <CODE>` | string | Language override (fi, fr, it, cs, nl, tr) |
| `--no-vsync` | flag | Disable vertical sync |
| `--debug` | flag | Set log level to Debug |
| `--trace` | flag | Set log level to Trace |

---

## **11. RENDERER & GRAPHICS**

### 11.1 OpenGL Context
**Default Configuration**:
```cpp
QSurfaceFormat format;
format.setSamples(0);  // No MSAA

// Platform-specific:
#ifdef __MC_GL30__
    format.setVersion(3, 0);
    format.setProfile(QSurfaceFormat::CoreProfile);
#elif defined(__MC_GLES__)
    format.setVersion(1, 0);
#else
    format.setVersion(2, 1);
#endif

// VSync (Qt 5.3+):
if (forceNoVSync) {
    format.setSwapInterval(0);  // Off
} else {
    format.setSwapInterval(Settings::loadVSync());  // 1=on, 0=off
}
```

### 11.2 Font Configuration
**Font Name**: `"generated"` (loaded from asset system)

### 11.3 Cursor Control
- **Default**: `Qt::BlankCursor` (hidden)
- **Menu**: `Qt::ArrowCursor` (visible)
- Controlled via signals: `cursorRevealed()` / `cursorHid()`

### 11.4 Scene Sizing
**Virtual Resolution** (derived from screen aspect ratio):
```
Scene::height = Scene::width * (screenHeight / screenWidth)
```

**Aspect Ratio Preservation**: Scene height adjusted to match display aspect ratio

---

## **12. AUDIO SYSTEM**

### 12.1 Audio Thread Architecture
- Separate `QThread` for audio processing
- `AudioWorker` moved to thread via `moveToThread()`
- Async slot invocation for `init()` and `loadSounds()`

### 12.2 Sound Effects
- Car engine sounds (start/stop)
- Race effects: "bell" (checkered flag), "cheering" (race end)
- Pit stop: "pit" sound with 3D position
- Menu transitions (implied)
- Controllable via `Settings::soundsKey()` (default: true)

---

## **13. NUMERIC CONSTANTS SUMMARY**

| Constant | Value | Use |
|----------|-------|-----|
| `MAX_PLAYERS` | 2 | Player count limit |
| `updateFps` | 60 | Default frame rate |
| `updateDelay` | 1000/60 = 16.67 | Update interval (ms) |
| `timeStep` | 1000/60 = 16.67 | Physics timestep (ms) |
| `lapCount` (default) | 5 | Default race length |
| `OFF_TRACK_MESSAGE_INTERVAL` | 30000 | Off-track message cooldown (ms) |
| `OFF_TRACK_LIMIT` | 60 | Off-track message trigger (ticks) |
| `STUCK_LIMIT` | 300 | Stuck car threshold (ticks = 5 sec) |
| `m_unlockLimit` | 6 | Position required to unlock track |
| `Intro duration` | 3000 | Intro screen duration (ms) |
| `MenuTransitionIn` | 2000 | Fade duration (ms) |
| `MenuTransitionOut` | 2000 | Fade duration (ms) |
| `GameTransitionIn` | 2000 | Fade duration (ms) |
| `GameTransitionOut` (normal) | 2000 | Fade duration (ms) |
| `GameTransitionOut` (finished) | 10000+10000 | Fade + delay (ms) |
| `oddOffset` | TrackTile::width/8 | Grid placement |
| `gridOffset` | TrackTile::width/12 | Grid offset |
| `spacing` | 0.75 × TrackTile::width | Car spacing |
| `checkpoint tolerance (finish)` | TrackTile::height/20 | Tolerance (units) |
| `stuck car radius` | TrackTile::width/4 | Recovery randomization |
| `checkered flag threshold` | 90% of lap | Flag trigger point |
| `TRACK_SET_VERSION` | 1 | Database schema version |

---

## **14. ENUM VALUES**

### Game Modes
- `OnePlayerRace = 0`
- `TwoPlayerRace = 1`
- `TimeTrial = 2`
- `Duel = 3`

### Split Screen Types
- `Horizontal = 0`
- `Vertical = 1`

### FPS Options
- `Fps30 = 0`
- `Fps60 = 1`

### Difficulty Levels
- `Easy = 0`
- `Medium = 1`
- `Hard = 2`

### State Machine States
- `Init = 0`
- `DoIntro = 1`
- `Menu = 2`
- `MenuTransitionIn = 3`
- `MenuTransitionOut = 4`
- `GameTransitionIn = 5`
- `GameTransitionOut = 6`
- `DoStartlights = 7`
- `Play = 8`

### Input Actions (Settings mapping)
- `Up` → `"IA_UP"`
- `Down` → `"IA_DOWN"`
- `Left` → `"IA_LEFT"`
- `Right` → `"IA_RIGHT"`

---

## **15. KEY STRINGS & IDENTIFIERS**

### Settings Group
- `"Config"`

### Settings Keys
- `"hRes"`, `"vRes"`, `"fullScreen"` (resolution)
- `"difficulty"` (difficulty level)
- `"fps"` (frame rate)
- `"lapCount"` (race length)
- `"sounds"` (audio enabled)
- `"screen"` (display index)
- `"vsync"` (vertical sync)

### Database Identifiers
- `"lap_record"` (table name)
- `"race_record"` (table name)
- `"best_position"` (table name)
- `"track_unlock"` (table name)

### Sound Effect Handles
- `"grid"` (start grid object)
- `"bell"` (checkered flag)
- `"cheering"` (race end)
- `"pit"` (pit stop)

### Messages (tr() for translation)
- `"New lap record!"`
- `"New race record!"`
- `"The Time Trial has ended!"`
- `"The winner has finished!"`
- `"Pit stop!"`
- `"You must stay on track!"`
- `"Watch your tires!"`
- `"A new best pos!"`
- `"A new track unlocked!"`
- `"Better luck next time.."`

### Font Name
- `"generated"`

---

## **16. BEHAVIORAL LOGIC SUMMARY**

### Race Start Sequence
1. MenuTransitionOut (fade-out flash, 2s)
2. GameTransitionIn (fade-in, 2s)
3. DoStartlights (light animation)
4. Play (racing active)

### Lap Completion Trigger
- Car passes through checkpoint 0
- Previous checkpoint was route.numNodes()-1 (circuit complete)
- Lap time recorded to `Timing` system
- Lap record notification if human player beats global record

### Race Completion Conditions
- **OnePlayerRace**: Human player finishes all laps
- **TwoPlayerRace**: Both human players finish all laps
- **Duel**: Both human players finish all laps
- **TimeTrial**: Human player finishes all laps

### Position Calculation Algorithm
1. Sort cars by `routeProgression` (descending)
2. Within same progression, sort by order within checkpoint
3. Assign 1-based position

### AI Behavior
- AI cars have reduced acceleration multiplier vs. humans
- AI cars are repositioned if stuck (moves to previous checkpoint + random)
- AI cars cannot set lap/race records

### Track Unlock Mechanism
- Unlock triggered when human finishes position ≤ 6
- Saves unlock status to database per (track, lapCount, difficulty)
- Message displayed during race
- Persists across sessions via Database

---

## **17. SIGNAL/SLOT CONNECTIONS**

### Race Signals
- `finished()` - Race completed
- `messageRequested(QString)` - Display message
- `tiresChanged(Car&)` - Tires changed in pit stop
- `lapRecordAchieved(int msec)` - New lap record
- `raceRecordAchieved(int msec)` - New race record

### Timing Signals
- `lapCompleted(size_t index, int msec)` - Lap finished
- `lapRecordAchieved(int msec)` - Lap record
- `raceRecordAchieved(int msec)` - Race record

### State Machine Signals
- `fadeInRequested(int, int, int)`
- `fadeOutRequested(int, int, int)`
- `fadeOutFlashRequested(int, int, int)`
- `soundsStopped()`
- `startlightAnimationRequested()`
- `renderingEnabled(bool)`
- `exitGameRequested()`

---

**End of Specification Document**
