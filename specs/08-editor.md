# **DUST RACING 2D TRACK EDITOR - COMPREHENSIVE SPECIFICATION**

## **1. EDITOR UI LAYOUT**

### **1.1 Main Window Structure**
The MainWindow (`mainwindow.cpp/hpp`) implements a Qt-based GUI with the following layout hierarchy:

**Layout Hierarchy:**
```
QMainWindow (MainWindow)
├── QSplitter (Vertical)
│   ├── Widget containing:
│   │   ├── QHBoxLayout (viewToolBarLayout)
│   │   │   ├── EditorView (QGraphicsView)
│   │   │   └── QToolBar (vertical orientation)
│   │   └── QHBoxLayout (sliderLayout)
│   │       ├── QLabel ("Scale:")
│   │       ├── QSlider (zoom/scale control)
│   │       └── QCheckBox (random rotation)
│   └── QTextEdit (console output)
├── QMenuBar
│   ├── File Menu
│   ├── Edit Menu
│   ├── Route Menu
│   └── Help Menu
└── QStatusBar
```

### **1.2 Zoom/Scale Parameters**
- **Minimum Zoom:** 0%
- **Maximum Zoom:** 200%
- **Initial Zoom:** 100%
- **Slider Range:** 0-200
- **Slider Tick Interval:** 10
- **Mouse Wheel Sensitivity:** ±10 per scroll (Ctrl+Wheel adjusts scale)
- **Console Height:** 64 pixels (fixed in splitter)

### **1.3 Toolbar**
- **Orientation:** Vertical
- **Position:** Right side of editor view
- **Content:** QActions for tile/object placement
  - Built dynamically from `editorModels.conf` configuration file
  - Two categories: "tile" (track pieces) and "free" (objects)
  - **Built-in Actions:**
    - "select" (icon: SELECT_ICON_PATH)
    - "erase" (icon: ERASE_ICON_PATH) - EraseObject mode
    - "clear" (icon: CLEAR_ICON_PATH) - SetTileType mode
  - **Custom Actions:** Loaded from object models (tiles and free objects)
- Each action stores model role as QVariant data
- Currently clicked action tracked in `m_currentToolBarAction`

### **1.4 Menus**

**File Menu:**
- New... (Ctrl+N) → initializeNewTrack()
- Open... (Ctrl+O) → openTrack()
- Save (Ctrl+S) → saveTrack() [disabled until track created]
- Save as... (Ctrl+Shift+S) → saveAsTrack() [disabled until track created]
- Quit (Ctrl+W) → close()

**Edit Menu:**
- Undo (Ctrl+Z) → undo() [disabled initially]
- Redo (Ctrl+Shift+Z) → redo() [disabled initially]
- Set properties... → setTrackProperties() [disabled until track created]

**Route Menu:**
- Clear route → clearRoute() [disabled until route exists]
- Set route... → beginSetRoute() [disabled until track created]

**Help Menu:**
- About
- About Qt

---

## **2. TRACK CREATION WORKFLOW**

### **2.1 New Track Dialog (NewTrackDialog)**

**Dialog Fields:**
- **Track Name:** QLineEdit (required, length > 0)
- **Columns:** QLineEdit (required, integer > 1)
- **Rows:** QLineEdit (required, integer > 1)
- **User Track:** QCheckBox (default: checked)
- **OK Button:** QPushButton (enabled only when all fields valid)
- **Cancel Button:** QPushButton

**Validation Rules:**
```cpp
m_nameEdit->length() > 0 && 
m_colsEdit->toInt() > 1 && 
m_rowsEdit->toInt() > 1
```

**Data Access:**
- `cols()` → returns unsigned int
- `rows()` → returns unsigned int
- `name()` → returns QString
- `isUserTrack()` → returns bool (checkbox state)

### **2.2 Track Creation Flow**

1. **User Action:** File → New...
2. **Dialog Display:** NewTrackDialog shown
3. **User Input:** Name, Cols, Rows, User Track flag
4. **Validation:** Data validated in real-time
5. **Dialog Accept:** Creates TrackData object
   ```cpp
   TrackData(name, isUserTrack, cols, rows)
   ```
6. **Scene Recreation:** Old QGraphicsScene deleted, new one created
7. **Grid Setup:** TrackTiles created for each grid position
   - Each tile initialized as "clear" type
   - Positioned at: (i * TILE_WIDTH, j * TILE_HEIGHT)
8. **Display:** Tiles added to scene, view updated
9. **Menu State:** Toolbar and menus enabled
10. **Status:** Track filename set to "New file", console message logged

### **2.3 Track Data Structure (TrackData)**
Contains:
- `name()` / `setName()` - Track name
- `fileName()` / `setFileName()` - File path
- `index()` / `setIndex()` - Track index (integer)
- `isUserTrack()` / `setUserTrack()` - User track flag
- `map()` - 2D grid of TrackTiles
- `objects()` - Collection of Objects
- `route()` - Route with target nodes
- Copy constructor for undo/redo

---

## **3. TILE PLACEMENT SYSTEM**

### **3.1 Tile Grid Structure**

**Grid Organization:**
- **Tile Size:** 64×64 pixels (TrackTile::width() = 64, TrackTile::height() = 64)
- **Grid Layout:** Indexed as [column][row] (i, j notation)
- **Scene Coordinates:** Tile at (i, j) positioned at (i*64, j*64)
- **Total Scene Size:** cols × 64 pixels wide, rows × 64 pixels tall

**TrackTile Class (Editor-specific):**
- Inherits from: QGraphicsItem + TrackTileBase
- **Constructor:**
  ```cpp
  TrackTile(QPointF location, QPoint matrixLocation, const QString & type = "clear")
  ```
- **Bounding Rect:** [-w/2, -h/2, w, h] centered at tile position
- **Rendering:** QPainter draws pixmap + overlays (active highlight, computer hints)

### **3.2 Tile Types**

All tile types defined in `editorModels.conf`:

**Track Tiles (category="tile"):**
| Role | Image | Description |
|------|-------|-------------|
| bridge | bridgeEditor.png | Bridge over obstacle |
| corner90 | corner.png | 90° corner |
| corner45Left | corner45Left.png | 45° corner (left) |
| corner45Right | corner45Right.png | 45° corner (right) |
| straight | straight.png | Straight track |
| straight45Male | straight45Male.png | 45° diagonal (male) |
| straight45Female | straight45Female.png | 45° diagonal (female) |
| finish | finishEditor.png | Finish line tile |
| grass | grassEditor.png | Grass border |
| sandGrassStraight | sandGrassStraightEditor.png | Sand-grass transition |
| sandGrassCorner | sandGrassCornerEditor.png | Sand-grass corner |
| sandGrassCorner2 | sandGrassCorner2Editor.png | Sand-grass corner variant 2 |
| sand | sandEditor.png | Sand tile |
| clear | - (internal) | Empty/unplaced tile |

### **3.3 Tile Properties**

**Per-Tile State:**
- **Type:** QString (e.g., "straight", "corner90")
- **Rotation:** Integer (0, 90, 180, 270 degrees)
- **Position:** QPointF (scene coordinates)
- **Pixmap:** QPixmap (rendered image)
- **Computer Hint:** Enum (None, Brake, BrakeHard)
- **Exclude from Minimap:** Boolean flag
- **Active State:** Boolean (visual highlight when selected)
- **Added State:** Boolean (tracks if added to scene)

### **3.4 Tile Placement Modes**

**Mode: SetTileType**

**Single Tile Placement:**
1. User selects tile type from toolbar
2. Cursor changes to tile icon
3. Left-click on target tile → changes tile type/pixmap/rotation
4. Undo point saved automatically

**Keyboard Modifiers:**
- **Ctrl+Click:** Activates flood fill algorithm

**Right-Click Context Menu on Tile:**
- Rotate 90° CW → `tile->rotate90CW()` (animated)
- Rotate 90° CCW → `tile->rotate90CCW()` (animated)
- Clear computer hint
- Set computer hint 'brake hard' (draws red 50% overlay)
- Set computer hint 'brake' (draws dark red 50% overlay)
- Exclude from minimap (checkable)
- Insert row before/after (at active row)
- Delete row (at active row)
- Insert column before/after (at active column)
- Delete column (at active column)

**Drag & Drop Tiles:**
1. Left-click on tile (mode=None)
2. Drag to target tile
3. Release → swaps tile properties (type, pixmap, rotation, hints)
4. Source tile restores to original position

### **3.5 Grid Manipulation**

**Active Position Tracking:**
- Updated on mouse move via `updateCoordinates(QPointF mappedPos)`
- Calculated as: `int column = mappedPos.x() / 64`, `int row = mappedPos.y() / 64`
- Clamped to [0, cols-1] and [0, rows-1]
- Displayed in status bar: "X: {mappedPos.x} Y: {mappedPos.y} I: {col} J: {row}"

**Row/Column Operations:**
```cpp
insertRowBefore()  // Insert before active row, shift down
insertRowAfter()   // Insert after active row, shift down
deleteRow()        // Delete active row, shift up
insertColumnBefore() // Insert before active column, shift right
insertColumnAfter()   // Insert after active column, shift right
deleteColumn()        // Delete active column, shift left
```

**Grid Modification Flow:**
1. User right-clicks tile → context menu
2. Selects row/column operation
3. Undo point saved
4. `TrackData::deleteRow/deleteColumn/insertRow/insertColumn()` called
5. Scene rect recalculated
6. View updated

---

## **4. OBJECT PLACEMENT SYSTEM**

### **4.1 Object Types and Dimensions**

**Brake Hint Objects (category="free"):**
| Role | Image | Width | Height | Purpose |
|------|-------|-------|--------|---------|
| brake | brake.png | 64 | 32 | Brake hint marker |
| left | left.png | 64 | 32 | Left turn hint |
| right | right.png | 64 | 32 | Right turn hint |

**Decoration Objects:**
| Role | Image | Width | Height |
|------|-------|-------|--------|
| crate | wood.png | 24 | 24 |
| rock | rock.png | 16 | 16 |
| tire | tire.png | 15 | 15 |
| plant | plant.png | 32 | 32 |
| tree | tree.png | 64 | 64 |
| wall | steel.jpg | 64 | 16 |
| wallLong | steel.jpg | 256 | 16 |

**Scenic Objects:**
| Role | Image | Width | Height |
|------|-------|-------|--------|
| bushArea | bushArea.png | 128 | 128 |
| grandstand | grandstandEditor.png | 128 | 128 |
| sandAreaCurve | sandAreaCurve.png | 128 | 128 |
| sandAreaBig | sandAreaBig.png | 512 | 64 |
| pit | pit.png | 256 | 54 |
| dustRacing2DBanner | dustRacing2DBanner.png | 256 | 32 |

### **4.2 Object Class Structure**

**Object Properties:**
- **Category:** QString ("tile" or "free")
- **Role:** QString (model name, e.g., "tree", "brake")
- **Location:** QPointF (scene coordinates, center point)
- **Rotation:** Integer (0-360 degrees)
- **Force Stationary:** Boolean (physics flag for game engine)
- **ZValue:** Integer (render order, higher = on top)
- **Size:** QSizeF (width, height)
- **Pixmap:** QPixmap (rendered image)

### **4.3 Object Placement Modes**

**Mode: AddObject**

**Placement:**
1. User selects object from toolbar
2. Mode set to EditorMode::AddObject
3. Cursor changes to custom cursor icon
4. Left-click on canvas → creates new object at clicked position
5. Undo point saved

**Object Creation Pipeline:**
```cpp
// ObjectFactory::createObject(role)
1. Fetch ObjectModel by role from ObjectModelLoader
2. Get dimensions (width, height) or use pixmap dimensions
3. Create Object with category, role, size, pixmap
4. Object added to scene via Mediator::addObject()
5. Object added to TrackData::objects() collection
6. ZValue set to 10 (above tiles)
```

**Drag & Drop Objects:**
1. Left-click on object (mode=None)
2. Drag to new position
3. Release → finalizes position
4. Undo point saved on initial click

**Arrow Key Movement:**
- Selected object moves ±1 pixel per key press
- Keys: Left, Right, Up, Down
- Undo point saved per key press

**Right-Click Context Menu on Object:**
- Rotate... → RotateDialog (prompts for angle, adds to current rotation)
- Force stationary (checkable) → toggles forceStationary flag

### **4.4 Object Persistence**

Objects stored in `TrackData::objects()` collection:
```cpp
// Saved to XML:
<object category="free" role="tree" x="512" y="256" orientation="45" forceStationary="1"/>
```

---

## **5. DRAG AND DROP SYSTEM**

### **5.1 DragAndDropStore Class**

**Purpose:** Tracks active drag-and-drop operation state

**Properties:**
```cpp
TrackTile * m_dragAndDropSourceTile        // Tile being dragged
Object * m_dragAndDropObject               // Object being dragged
TargetNode * m_dragAndDropTargetNode       // Target node being dragged
QPointF m_dragAndDropSourcePos             // Original tile position (for swap)
```

**API:**
- `setDragAndDropSourceTile(TrackTile *)` / `dragAndDropSourceTile()`
- `setDragAndDropObject(Object *)` / `dragAndDropObject()`
- `setDragAndDropTargetNode(TargetNode *)` / `dragAndDropTargetNode()`
- `setDragAndDropSourcePos(QPointF)` / `dragAndDropSourcePos()`
- `clear()` - Resets all pointers to nullptr

### **5.2 Tile Drag & Drop**

**Sequence:**
1. **Mouse Press (Left Button, mode=None):**
   - User clicks tile
   - `m_mediator.dadStore().setDragAndDropSourceTile(tile)`
   - `m_mediator.dadStore().setDragAndDropSourcePos(tile.pos())`
   - `tile.setZValue(zValue + 1)` - Bring to front
   - Cursor → Qt::ClosedHandCursor

2. **Mouse Move:**
   - Tile follows cursor: `sourceTile->setPos(mappedPos)`

3. **Mouse Release:**
   - Fetch destination tile at release position
   - If different tile found: `sourceTile->swap(*destTile)`
   - Restore source tile position: `sourceTile->setPos(sourcePos)`
   - Restore ZValues
   - Clear drag store
   - Cursor restored
   - Undo point already saved (on mouse press)

**Tile::swap() Operation:**
Exchanges between two tiles:
- Tile type
- Pixmap
- Rotation
- Computer hints

### **5.3 Object Drag & Drop**

**Sequence:**
1. **Mouse Press (Left Button, mode=None on Object):**
   - `m_mediator.dadStore().setDragAndDropObject(object)`
   - `object.setZValue(zValue + 1)` - Bring to front
   - Cursor → Qt::ClosedHandCursor
   - Undo point saved

2. **Mouse Move:**
   - Object follows cursor: `object->setLocation(mappedPos)`

3. **Mouse Release:**
   - Set final position: `object->setLocation(releasePos)`
   - `object.setZValue(zValue - 1)` - Restore layer
   - Clear drag store
   - Cursor restored

### **5.4 Target Node Drag & Drop**

**Sequence:**
1. **Mouse Press (Left Button, mode=None on TargetNode):**
   - `m_mediator.dadStore().setDragAndDropTargetNode(tnode)`
   - `tnode.setZValue(zValue + 1)` - Bring to front
   - Cursor → Qt::ClosedHandCursor
   - Undo point saved

2. **Mouse Move:**
   - Node follows cursor: `tnode->setLocation(mappedPos)`

3. **Mouse Release:**
   - Set final position: `tnode->setLocation(releasePos)`
   - `tnode.setZValue(zValue - 1)` - Restore layer
   - Clear drag store
   - Cursor restored

---

## **6. FLOOD FILL ALGORITHM**

### **6.1 Activation**

**Trigger:** Ctrl+Left-Click on tile (SetTileType mode)

**Condition Check:**
```cpp
if (Ctrl modifier pressed && tile.tileType() != action->data().toString())
{
    m_mediator.floodFill(tile, action, tile.tileType());
}
```

### **6.2 Algorithm Implementation**

**Located in:** `FloodFill::floodFill()` namespace function

**Parameters:**
- `TrackTile & tile` - Starting tile
- `QAction * action` - Tile type action (contains new type + icon)
- `QString typeToFill` - Original tile type to replace
- `MapBase & map` - Grid reference

**Algorithm:**
```cpp
const QPoint neighborOffsets[4] = {
    {1, 0},   // right
    {0, -1},  // up
    {-1, 0},  // left
    {0, 1}    // down
};

std::deque<QPoint> stack;
stack.push_back(tile.matrixLocation());

while (stack.size()) {
    QPoint location = stack.back();
    stack.pop_back();
    
    if (tile = getTile(location)) {
        setTileType(*tile, action);  // Change type + pixmap
    }
    
    for (4 directions) {
        int x = location.x() + offset.x();
        int y = location.y() + offset.y();
        
        // Bounds check
        if (x >= 0 && y >= 0 && x < cols && y < rows) {
            if (tile = getTile(x, y)) {
                // Only add if same type as original
                if (tile->tileType() == typeToFill) {
                    stack.push_back({x, y});
                }
            }
        }
    }
}
```

**Key Points:**
- **Flood Fill Uses:** Depth-first search with deque (stack)
- **Connectivity:** 4-directional (no diagonals)
- **Boundary Check:** Prevents out-of-bounds access
- **Type Check:** Only fills contiguous same-type tiles
- **Undo Point:** Saved before flood fill starts
- **Visual Update:** All affected tiles rendered with new pixmap

---

## **7. UNDO/REDO SYSTEM**

### **7.1 UndoStack Class**

**Configuration:**
- **Max History Size:** 100 (default)
- **Storage:** Two std::lists (deque-like)
  - `m_undoStack` - Historical states
  - `m_redoStack` - Redo states

### **7.2 Undo/Redo Workflow**

**Undo Point Saving:**
```cpp
void EditorData::saveUndoPoint() {
    TrackData copy = *currentTrackData;  // Deep copy via copy constructor
    m_undoStack.push_back(copy);
    
    if (m_undoStack.size() > maxHistorySize) {
        m_undoStack.pop_front();  // Remove oldest
    }
    m_mediator.enableUndo(m_undoStack.isUndoable());
}
```

**Undo Action:**
```cpp
void EditorData::undo() {
    if (m_undoStack.isUndoable()) {
        m_dadStore.clear();                // Clear drag state
        m_selectedObject = nullptr;        // Clear selections
        m_selectedTargetNode = nullptr;
        
        saveRedoPoint();                   // Save current for redo
        clearScene();                      // Remove all graphics items
        m_trackData = m_undoStack.undo();  // Get previous state
    }
}
```

**Redo Action:**
```cpp
void EditorData::redo() {
    if (m_undoStack.isRedoable()) {
        // Same as undo but using redo stack
        saveUndoPoint();                   // Save current for undo
        clearScene();
        m_trackData = m_undoStack.redo();
    }
}
```

### **7.3 Operations Triggering Undo Points**

Undo points saved before:
- Tile type change (left-click)
- Tile rotation (right-click)
- Computer hint setting/clearing
- Exclude from minimap toggle
- Tile drag & drop completion
- Row/column insertion or deletion
- Object placement
- Object drag & drop completion
- Object rotation
- Object force stationary toggle
- Target node drag & drop completion
- Target node size change
- Route clearing
- Route node addition

### **7.4 Menu Integration**

**Undo Action (Ctrl+Z):**
- Call `m_mediator.undo()`
- Call `setupTrackAfterUndoOrRedo()`
- Recreate scene with new track state
- Update action enablement based on stack state

**Redo Action (Ctrl+Shift+Z):**
- Call `m_mediator.redo()`
- Call `setupTrackAfterUndoOrRedo()`
- Recreate scene with new track state
- Update action enablement based on stack state

**Action Enablement:**
- Initially disabled (no undo history)
- Enabled after `saveUndoPoint()` called (undo=true)
- Toggle based on `m_undoStack.isUndoable()` / `isRedoable()`

---

## **8. TRACK FILE FORMAT**

### **8.1 File Extension and Format**

- **Extension:** `.trk` (track file)
- **Format:** XML
- **Encoding:** UTF-8
- **Processing Instruction:** `<?xml version='1.0' encoding='UTF-8'?>`

### **8.2 Root Element Structure**

```xml
<?xml version='1.0' encoding='UTF-8'?>
<track version="VERSION"
       name="Track Name"
       cols="16"
       rows="20"
       index="3"
       user="1">
    <!-- Tile definitions -->
    <!-- Object definitions -->
    <!-- Target node definitions -->
</track>
```

**Root Attributes:**
| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| version | String | Yes | Editor version (Config::Editor::EDITOR_VERSION) |
| name | String | Yes | Track name |
| cols | Integer | Yes | Grid columns (width) |
| rows | Integer | Yes | Grid rows (height) |
| index | Integer | Yes | Track index (lap count / race position) |
| user | Boolean | No | "1" if user track, omitted if not |

### **8.3 Tile Elements**

```xml
<tile type="straight"
      i="0"
      j="0"
      orientation="90"
      excludeFromMinimap="0"
      computerHint="1"/>
```

**Tile Attributes:**
| Attribute | Type | Default | Description |
|-----------|------|---------|-------------|
| type | String | "clear" | Tile type (straight, corner90, etc.) |
| i | Integer | 0 | Column index |
| j | Integer | 0 | Row index |
| orientation | Integer | 0 | Rotation (0, 90, 180, 270) |
| excludeFromMinimap | Boolean | "0" | Omitted if false |
| computerHint | Integer | omitted | 0=None, 1=Brake, 2=BrakeHard |

**Tile Definition:**
One `<tile>` element per grid cell (cols × rows elements total)

### **8.4 Object Elements**

```xml
<object category="free"
        role="tree"
        x="256"
        y="384"
        orientation="45"
        forceStationary="0"/>
```

**Object Attributes:**
| Attribute | Type | Default | Description |
|-----------|------|---------|-------------|
| category | String | - | "tile" or "free" |
| role | String | - | Object type (tree, crate, brake, etc.) |
| x | Integer | 0 | X coordinate (pixels) |
| y | Integer | 0 | Y coordinate (pixels) |
| orientation | Integer | 0 | Rotation (0-360 degrees) |
| forceStationary | Integer | omitted | Omitted if 0 (false) |

**Object Definition:**
One `<object>` element per placed object (variable count)

### **8.5 Target Node Elements**

```xml
<node index="0"
      x="512"
      y="256"
      width="128"
      height="64"/>
```

**Node Attributes:**
| Attribute | Type | Default | Description |
|-----------|------|---------|-------------|
| index | Integer | - | Node order in route (0, 1, 2, ...) |
| x | Integer | 0 | X coordinate (pixels) |
| y | Integer | 0 | Y coordinate (pixels) |
| width | Integer | 0 | Node size (width) |
| height | Integer | 0 | Node size (height) |

**Node Definition:**
One `<node>` element per target node in route (variable count, creates loop)

### **8.6 File I/O Operations**

**Save Operation (TrackIO::save):**
1. Create QDomDocument
2. Add XML processing instruction
3. Create root element with track metadata
4. Iterate grid: write all `<tile>` elements
5. Iterate objects: write all `<object>` elements
6. Iterate route: write all `<node>` elements
7. Open file for writing
8. Write doc.toString() to file
9. Close file and return success

**Load Operation (TrackIO::open):**
1. Open file for reading
2. Parse XML into QDomDocument
3. Extract root attributes: cols, rows, name, index, user flag
4. Create new TrackData(name, isUserTrack, cols, rows)
5. Set filename and index
6. Iterate elements:
   - `<tile>` → readTile() - Updates existing tile in grid
   - `<object>` → readObject() - Creates new Object, adds to collection
   - `<node>` → readTargetNode() - Stores in temporary vector
7. Build route from sorted node vector
8. Return TrackDataPtr or nullptr if parse fails

**Error Handling:**
- Returns nullptr if file doesn't open
- Returns nullptr if XML parse fails
- Returns nullptr if cols/rows invalid (≤0)

---

## **9. NEW TRACK DIALOG OPTIONS**

### **9.1 Dialog Layout**

```
┌─────────────────────────────┐
│ Create a new track          │
├─────────────────────────────┤
│ Track name:    [_________]  │
│ User track:    [✓]          │
│ Columns:       [_________]  │
│ Rows:          [_________]  │
├─────────────────────────────┤
│ [Ok]         [Cancel]       │
└─────────────────────────────┘
```

**Grid Layout (QGridLayout):**
- Row 0: Label "Track name:" | QLineEdit (m_nameEdit)
- Row 1: Label "User track:" | QCheckBox (m_userCheck, default=checked)
- Row 2: Label "Columns:" | QLineEdit (m_colsEdit)
- Row 3: Label "Rows:" | QLineEdit (m_rowsEdit)
- Row 4: QPushButton "Ok" | QPushButton "Cancel"

### **9.2 Input Validation**

**Real-time Validation (validateData slot):**
- Connected to all three edit fields' `textChanged` signal
- OK button enabled only when:
  ```cpp
  m_nameEdit->text().length() > 0 &&
  m_colsEdit->text().toInt() > 1 &&
  m_rowsEdit->text().toInt() > 1
  ```
- OK button initially disabled

**Constraints:**
- **Name:** At least 1 character (non-empty string)
- **Columns:** Integer > 1 (minimum 2)
- **Rows:** Integer > 1 (minimum 2)
- **User Track:** Boolean (checkbox state)

### **9.3 Dialog Signals & Slots**

**Connections:**
- `m_nameEdit::textChanged` → `validateData()`
- `m_rowsEdit::textChanged` → `validateData()`
- `m_colsEdit::textChanged` → `validateData()`
- `m_okButton::clicked` → `accept()`
- `m_cancelButton::clicked` → `reject()`

**Return Values (accessors):**
- `cols()` - Returns m_colsEdit text as unsigned int
- `rows()` - Returns m_rowsEdit text as unsigned int
- `name()` - Returns m_nameEdit text as QString
- `isUserTrack()` - Returns m_userCheck checked state as bool

### **9.4 Dialog Execution**

**Usage (in MainWindow::initializeNewTrack):**
```cpp
NewTrackDialog dialog(&m_mainWindow);
if (dialog.exec() == QDialog::Accepted) {
    // User clicked Ok - proceed with track creation
    createNewTrack(
        dialog.name(),
        dialog.isUserTrack(),
        dialog.cols(),
        dialog.rows()
    );
}
// else: User clicked Cancel - do nothing
```

---

## **10. EDITOR MODE SYSTEM (EditorMode)**

**Modes:**
| Mode | Value | Triggered By | Behavior |
|------|-------|--------------|----------|
| None | 0 | Select action | Normal selection/drag mode |
| SetTileType | 1 | Tile selection | Change tile types, flood fill with Ctrl |
| EraseObject | 2 | Erase action | Remove objects on click |
| AddObject | 3 | Object selection | Place objects on click |
| SetRoute | 4 | Begin route | Place target nodes on tile click |

**Mode Transitions:**
- SetTileType → None (Select clicked)
- EraseObject → None (Select clicked)
- AddObject → None (Select clicked)
- SetRoute → None (Route finalized)
- Any mode → None (clearEditMode())

---

## **11. CONSTANTS AND CONFIGURATION**

### **11.1 TrackTile Dimensions**
```cpp
static const size_t width()  = 64  // pixels
static const size_t height() = 64  // pixels
```

### **11.2 MainWindow Zoom Limits**
```cpp
m_minZoom = 0              // Minimum zoom percentage
m_maxZoom = 200            // Maximum zoom percentage
m_initZoom = 100           // Initial zoom (100%)
m_consoleHeight = 64       // Console display height
```

### **11.3 Scale/View Constants**
```cpp
// Mouse wheel sensitivity
const int sensitivity = 10;

// Max scale (for scale slider)
const int maxScale = 200;

// Route line Z-value (above tiles)
const int routeLineZ = 10;

// Object default Z-value
const int objectZ = 10;
```

### **11.4 Computer Hint Visualization**

**Brake Hard (Value=1):**
- Overlay: QColor(255, 0, 0, 128) - Bright red with 50% alpha
- Fills entire tile bounding rect

**Brake (Value=2):**
- Overlay: QColor(128, 0, 0, 128) - Dark red with 50% alpha
- Fills entire tile bounding rect

### **11.5 Undo Stack Configuration**
```cpp
const unsigned int maxHistorySize = 100  // Default undo history limit
```

### **11.6 Configuration File Paths**
```cpp
Config::Editor::MODEL_CONFIG_FILE_NAME  // "editorModels.conf"
Config::General::dataPath               // Base data directory

// Icon paths
Config::Editor::SELECT_ICON_PATH        // Select cursor icon
Config::Editor::ERASE_ICON_PATH         // Erase mode icon
Config::Editor::CLEAR_ICON_PATH         // Clear/empty tile icon
```

### **11.7 Editor Version**
```cpp
Config::Editor::EDITOR_VERSION          // Stored in track file root/@version
Config::Editor::EDITOR_NAME             // "Dust Racing 2D Editor"
```

---

## **12. MEDIATOR PATTERN ARCHITECTURE**

The Mediator pattern centralizes communication between UI components:

```
MainWindow (UI Window)
    ↓
Mediator (Communication Hub)
    ├─ EditorData (Track State + I/O)
    ├─ EditorView (Graphics View)
    ├─ QGraphicsScene (Graphics Items)
    └─ ObjectModelLoader (Asset Management)
```

**Key Mediator Methods:**
- `addCurrentToolBarObjectToScene()` - Place object at clicked position
- `floodFill()` - Execute flood fill algorithm
- `saveUndoPoint()` / `undo()` / `redo()` - Undo/redo operations
- `setMode()` / `mode()` - Manage editor modes
- `openTrack()` - Load track file
- `saveTrackData()` / `saveTrackDataAs()` - Save operations
- `updateCoordinates()` - Update status bar with grid position
- `setScale()` - Apply zoom transform

---

## **13. ROUTE/TARGET NODE SYSTEM**

### **13.1 Route Management**

**Route Creation:**
1. User: Route → Set route...
2. Dialog: "Setting the route defines checkpoints..."
3. Mode: EditorMode::SetRoute activated
4. User: Clicks tiles to place target nodes
5. Each click: Creates TargetNode at clicked position
6. Route forms: Chain of nodes with next/prev pointers
7. Closure: Click on first node again to close loop
8. Completion: endSetRoute() called

**Route Properties:**
- **Type:** Circular linked list of TargetNode objects
- **Nodes:** Connected via setPrev()/setNext()
- **Route Lines:** QGraphicsLineItem connecting nodes (Z=10)
- **Visualization:** Nodes + lines rendered in scene
- **Persistence:** Saved to XML as `<node>` elements

### **13.2 TargetNode Features**

**TargetNode (Editor-specific subclass):**
- **Location:** QPointF (scene coordinate)
- **Size:** QSizeF (width, height for checkpoint area)
- **Index:** Integer (position in route)
- **Graphics:** Circular visual representation
- **Route Line:** Associated QGraphicsLineItem

**Right-Click Context Menu:**
- Set size... → TargetNodeSizeDlg (prompts for width/height)

**Keyboard/Arrow Movement:**
- Arrow keys move selected node ±1 pixel

**Drag & Drop:**
- Left-click + drag repositions node
- Undo point saved on initial click

---

## **14. SCENE AND RENDERING**

### **14.1 Graphics Scene Hierarchy**

**QGraphicsScene Contents (Z-Order, top to bottom):**
1. Route Lines (Z=10)
2. Target Nodes (Z=10)
3. Objects (Z=10)
4. Tiles (Z varies based on drag state)

**Scene Rect:**
- Calculated after track load/creation
- Size: (cols * 64, rows * 64) pixels
- Top-left: (0, 0)

### **14.2 Tile Rendering (paint):**

**For "clear" tiles:**
- Draw pixmap (CLEAR_ICON_PATH)
- Draw black border

**For typed tiles:**
- Draw tile pixmap
- If computerHint=BrakeHard: Fill rect with red overlay (255,0,0,128)
- If computerHint=Brake: Fill rect with dark red overlay (128,0,0,128)

**If tile is active:**
- Fill entire bounding rect with dark overlay (0,0,0,64)

---

## **15. TRACK PROPERTIES DIALOG**

**TrackPropertiesDialog:**
Allows editing after track creation:
- **Track Name:** Editable field
- **Track Index:** Integer field (lap count / race order)
- **User Track:** Checkbox

**Accessed via:**
- Edit → Set properties...

**Applied to:**
- Current TrackData object
- Does not require track reload

---

## **SUMMARY TABLE: All Object Categories**

| Category | Type | Count | Purpose |
|----------|------|-------|---------|
| Tile | Track pieces | 13 | Buildtrack grid |
| Free | Obstacles/Decorations | 15 | Scenery and gameplay |
| **Total** | | **28** | |

**Track Tile Count:** 13 types (bridge, straight, corners, grass, sand, finish, etc.)
**Free Object Count:** 15 types (brake, tree, crate, pit, grandstand, etc.)

---

This comprehensive specification documents the DustRacing2D track editor with complete architecture, workflows, file format, and all constants.
