# **DUST RACING 2D MENU SYSTEM & FRAMEWORKS - REVERSE ENGINEERING SPECIFICATION**

## **OVERVIEW**
This document details the menu architecture, screen layouts, UI components, and framework implementation for VibeRacing's menu system, including the **MTFH (Menu Template Framework Header)** and **STFH** (Sound/Audio) frameworks.

---

## **PART 1: MENU SCREENS & CONTENT**

### **1. MAIN MENU** (`mainmenu.cpp/.hpp`)
**Menu ID:** `"main"`  
**Style:** VerticalList  
**Background:** `"mainMenuBack"` surface  
**Items** (rendered bottom-to-top):
- **Play** → Opens `"difficulty"` menu
- **Help** → Opens `"help"` menu  
- **Credits** → Opens `"credits"` menu
- **Settings** → Opens `"settings"` menu
- **Quit** → Emits `exitGameRequested()` signal

**Item Height:** `height() / 8`  
**Text Size:** 40px  
**Colors:** White text (1.0, 1.0, 1.0) with 2px shadow offset (2, -2)

---

### **2. DIFFICULTY MENU** (`difficultymenu.cpp/.hpp`)
**Menu ID:** `"difficulty"`  
**Style:** VerticalList  
**Background:** `"trackSelectionBack"`  
**Items** (rendered as):
- **Hard**
- **Medium**  
- **Easy**

**Item Height:** `height() / 8`  
**Text Size:** 40px  
**Actions:** Save difficulty to Settings, push to `"lapCount"` menu

---

### **3. LAP COUNT MENU** (`lapcountmenu.cpp/.hpp`)
**Menu ID:** `"lapCount"`  
**Style:** HorizontalList  
**Background:** `"trackSelectionBack"`  
**Available Lap Counts:** `[1, 3, 5, 10, 20, 50, 100]`  
**Item Height:** `height() / (numLapCounts + 2)` (7 items = `height() / 9`)  
**Item Width:** `width() / (numLapCounts + 2)` (7 items = `width() / 9`)  
**Text Size:** 40px  
**Title Text:** "CHOOSE LAP COUNT" (rendered at y = `height() / 2 + text.height(font) * 2`)  
**Title Color:** Default (white)  
**Title Size:** 30px  
**Actions:** Save lap count, push to `"trackSelection"` menu

---

### **4. TRACK SELECTION MENU** (`trackselectionmenu.cpp/.hpp`)
**Menu ID:** `"trackSelection"`  
**Style:** ShowMany (displays multiple items)  
**Background:** `"trackSelectionBack"`  
**Mouse Items:** Prev `<`, Next `>`, Quit `X`  
**Item Display:** `width() / 2 × height() / 2` preview tiles  
**Wrap Around:** Disabled (no circular navigation)

**Per Track Item Rendering:**
- **Track Tiles Preview:** Grid visualization with map preview surfaces, scaled to fit
  - Locked tracks: Color (0.5, 0.5, 0.5)
  - Unlocked tracks: Color (1.0, 1.0, 1.0)
- **Track Name:** 30px text, centered above preview
- **Stars:** 10-star rating system
  - Yellow (1.0, 1.0, 0.0) stars for achievements
  - Grey (0.75, 0.75, 0.75) empty stars
  - Half-star support for rank 11
  - Located at y = `-height() / 2 + starH / 2`
- **Lock Icon:** Rendered for locked tracks
- **Track Properties** (20px text, left-aligned):
  - "Laps: X"
  - "Length: X meters"
  - "Lap Record: HH:MM:SS.mmm"
  - "Race Record: HH:MM:SS.mmm"
  - "Finish previous track in TOP-6 to unlock!" (locked tracks)
  - "Unlock it in one/two player race!" (single player without AI)

**Animation:** Slide-in/slide-out with exponential easing (15 steps, exp=3)
- Previous/next item entry: `x = ±1000` to `width() / 2`

---

### **5. SETTINGS MENU** (`settingsmenu.cpp/.hpp`)
**Menu ID:** `"settings"`  
**Style:** VerticalList  
**Background:** `"settingsBack"`  
**Item Height:** `height() / 10`  
**Text Size:** 20px

**Menu Items:**
- **Game mode** → Opens `"gameModeMenu"` submenu
- **Sounds** → Opens `"sfxMenu"` submenu
- **GFX** → Opens `"gfxMenu"` submenu
- **Controls** → Opens `"keyConfigMenu"` submenu
- **Reset** → Opens `"resetMenu"` submenu

---

### **6. GAME MODE MENU** (`"gameModeMenu"`)
**Style:** VerticalList  
**Items** (bottom-to-top):
- One player race
- Two player race  
- Time Trial
- Duel

**Text Size:** 20px

---

### **7. FPS MENU** (`"fpsMenu"`)
**Style:** VerticalList  
**Items:**
- 30 fps
- 60 fps

**Text Size:** 20px  
**Item Height:** `height() / 6` (2 items + 4 spacing = 6)

---

### **8. GFX MENU** (`"gfxMenu"`)
**Style:** VerticalList  
**Items** (bottom-to-top):
- Full screen resolution → Opens `"fullScreenResolutionMenu"`
- Windowed resolution → Opens `"windowedResolutionMenu"`
- FPS → Opens `"fpsMenu"`
- Split type → Opens `"splitTypeMenu"`
- VSync → Opens `"vsyncMenu"` (Qt 5.3+)

**Text Size:** 20px

---

### **9. RESOLUTION MENU** (`resolutionmenu.cpp/.hpp`)
**Variants:** `"fullScreenResolutionMenu"`, `"windowedResolutionMenu"`  
**Style:** VerticalList  
**Background:** `"settingsBack"`  
**Resolutions:** 8 options, dynamically scaled down from screen dimensions
- Resolution text format: `"1920x1080"`  
- **Text Size:** 20px

**On Selection:** Shows ConfirmationMenu with text "Restart to change the resolution."

---

### **10. SPLIT TYPE MENU** (`"splitTypeMenu"`)
**Style:** VerticalList  
**Items:**
- Vertical
- Horizontal

**Text Size:** 20px  
**Item Height:** `height() / 6`

---

### **11. VSYNC MENU** (`vsyncmenu.cpp/.hpp`)
**Menu ID:** `"vsyncMenu"`  
**Style:** VerticalList  
**Background:** `"settingsBack"`  
**Item Height:** `height() / 10`  
**Items:**
- Off (vsync = 0)
- On (vsync = 1)

**Text Size:** 20px  
**On Selection:** Shows ConfirmationMenu with text "Restart to change VSync setting."

---

### **12. SFX MENU** (`"sfxMenu"`)
**Style:** VerticalList  
**Items** (bottom-to-top):
- Off (disabled audio)
- On (enabled audio)

**Text Size:** 20px

---

### **13. KEY CONFIG MENU** (`keyconfigmenu.cpp/.hpp`)
**Menu ID:** `"keyConfigMenu"` (internally created as `"pressKeyMenu"`)  
**Style:** VerticalList  
**Background:** `"settingsBack"`  
**Item Height:** `height() / 10`  
**Text Size:** 20px

**Player Two Controls** (rendered first):
- Player Two Brake
- Player Two Accelerate
- Player Two Turn Right
- Player Two Turn Left

**Player One Controls**:
- Player One Brake
- Player One Accelerate
- Player One Turn Right
- Player One Turn Left

**Press Key Menu** (spawned on action):
- **Display Text:** "PRESS A KEY.."
- **Text Color:** (0.25, 0.75, 1.0) cyan
- **Text Size:** 20px  
- **Centered at:** `width() / 2, height() / 2`

---

### **14. RESET MENU** (`"resetMenu"`)
**Style:** VerticalList  
**Items:**
- Reset record times
- Reset best positions
- Reset unlocked tracks

**Text Size:** 20px  
**Actions:** Opens ConfirmationMenu for each

---

### **15. CONFIRMATION MENU** (`confirmationmenu.cpp/.hpp`)
**Menu ID:** `"confirmationMenu"`  
**Style:** HorizontalList (2-button layout)  
**Background:** `"settingsBack"`  
**Width:** Full menu width  
**Height:** Full menu height  
**Items:**
- **Ok** (width: `width() / 4`, height: `height()`)
- **Cancel** (width: `width() / 4`, height: `height()`)

**Text Color:** (0.25, 0.75, 1.0) cyan  
**Text Size:** 20px  
**Display Text:** Custom per confirmation (rendered at y = `height() / 2 + 60`)

---

### **16. HELP SCREEN** (`help.cpp/.hpp`)
**Menu ID:** `"help"`  
**Style:** VerticalList  
**Background:** `"helpBack"`  
**Display:** Static rendered text (no menu items)

**Content:**
```
GAME GOAL

You are racing against eleven
computer players.

Your best position will be
the next start position.

Finish in TOP-6 to unlock
a new race track!

CONTROLS FOR PLAYER 1

Turn left  : Left
Turn right : Right
Accelerate : Up / RIGHT SHIFT
Brake      : Down / RIGHT CTRL

CONTROLS FOR PLAYER 2

Turn left  : A
Turn right : D
Accelerate : W / LEFT SHIFT
Brake      : S / LEFT CTRL

Quit       : ESC/Q
Pause      : P

[WEB_SITE_URL]
```

**Text Size:** 20px × (20 * height() / 640)  
**Text Color:** Default white

---

### **17. CREDITS SCREEN** (`credits.cpp/.hpp`)
**Menu ID:** `"credits"`  
**Style:** VerticalList  
**Background:** `"creditsBack"`  
**Display:** Rotating credits (cycle every 120 frames)

**Credit Sections** (rotated):
1. PROGRAMMING: Jussi Lind
2. GRAPHICS: Jussi Lind, Ville Mäkiranta (original)
3. LEVEL DESIGN: Jussi Lind, Wuzzy
4. TRANSLATIONS: Pavel Fric (cs), Wuzzy (de), Paolo Straffi (it), Jussi Lind (fi), Rémi Verschelde (fr)
5. PATCHES: [Multiple contributors]
6. SPECIAL THANKS: Tommi Martela, Alex Rietveld, Matthias Mailänder

**Text Size:** 20px × (20 * height() / 640)  
**Rotation Interval:** 120 frames per section  
**Text Alignment:** Centered

---

## **PART 2: MTFH FRAMEWORK (MENU TEMPLATE FRAMEWORK HEADER)**

### **Architecture Overview**
MTFH is a hierarchical menu framework using a **stack-based menu management system** with **animation curves** for smooth transitions.

---

### **Core Components**

#### **A. Menu Class** (`menu.hpp/.cpp`)

**Namespace:** `MTFH`

**Menu Styles (enum):**
```cpp
enum class Style {
    VerticalList,    // Items stacked vertically, centered
    HorizontalList,  // Items in row, centered horizontally
    ShowOne,         // Single item displayed at a time
    ShowMany         // Multiple items shown (used by TrackSelectionMenu)
};
```

**Mouse Item Types:**
```cpp
enum class MouseItemType {
    Quit,   // X button (top-right)
    Prev,   // < button (left)
    Next    // > button (right)
};
```

**Key Methods:**
- `addItem(MenuItemPtr)` - Add menu item
- `reverseItems()` - Reverse item order
- `addMouseItem(MouseItemType, MenuItemPtr)` - Add mouse control button
- `render()` - Render menu items
- `up(), down(), left(), right()` - Navigate items (wrap-around by default)
- `selectCurrentItem()` - Select/activate current item
- `setPos(x, y)` - Set menu position
- `setPos(x, y, targetX, targetY)` - Set animation target
- `setItemsToShow(vector<int>)` - For ShowMany style
- `enter(), pushEnter(), pushExit(), popEnter(), popExit()` - Lifecycle methods
- `exit()` - Pop from menu stack
- `stepTime(msecs)` - Update animations

**Position Animation:**
```cpp
m_x = m_x + (m_targetX - m_x) * m_animationCurve.value();
m_y = m_y + (m_targetY - m_y) * m_animationCurve.value();
```

**Private Members:**
- `std::vector<MenuItemPtr> m_items` - Menu items
- `std::vector<MouseItem> m_mouseItems` - Mouse control buttons
- `std::string m_id` - Unique menu identifier
- `int m_width, m_height` - Menu dimensions
- `float m_x, m_y, m_targetX, m_targetY` - Position & animation target
- `int m_currentIndex` - Currently focused item
- `int m_selectedIndex` - Currently selected item
- `Style m_style` - Layout style
- `bool m_isDone` - Exit flag
- `bool m_wrapAround` - Enable wrap-around navigation
- `AnimationCurve m_animationCurve` - Position animation curve

---

#### **B. MenuItem Class** (`menuitem.hpp/.cpp`)

**Constructor:**
```cpp
MenuItem(float width, float height, std::wstring text = L"", bool selectable = false);
```

**Key Methods:**
- `setMenu(Menu*)` - Set parent menu
- `setIndex(int)` - Set item index in menu
- `setPos(x, y)` / `setPos(x, y, targetX, targetY)` - Position & animation
- `setView(MenuItemViewPtr)` - Set custom renderer
- `setAction(MenuItemActionPtr)` / `setAction(ActionFunction)` - Set behavior
- `setMenuOpenAction(menuId)` - Set submenu to open on selection
- `setFocused(bool)` - Set focus state (keyboard highlight)
- `setSelected(bool)` / `setCurrent()` - Set selection state
- `render()` - Render the item via its view
- `stepTime(msecs)` - Update animations
- `positionAnimation(msecs)` - Update position animation
- `setContentsMargins(left, right, top, bottom)` - Set padding

**Animation:**
- Default curve: 30 steps, exponent 3
- Position can animate from current to target

---

#### **C. MenuItemView Class** (`menuitemview.hpp`)

**Pure Virtual Base Class:**
```cpp
class MenuItemView {
    virtual void render(float x, float y) = 0;
    virtual void stepTime(int msecs) {}
};
```

**Implementation:** TextMenuItemView (renders text with animations)

---

#### **D. TextMenuItemView Class** (`textmenuitemview.cpp/.hpp`)

**Colors:**
- **Focused:** Yellow (1.0, 1.0, 0.0)
- **Selected:** Red (1.0, 0.0, 0.0)
- **Normal:** White (1.0, 1.0, 1.0)

**Text Animation:**
```cpp
float amp = 0.05f;
float animatedSize = m_textSize + sin(m_angle) * m_textSize * amp;
if (owner().focused()) {
    animatedSize *= 1.25f;  // 25% scale-up when focused
}
```

**Animation Loop:** `m_angle += 0.010f` per frame (breathing effect)

**Shadow:** 2px offset (x=2, y=-2)

---

#### **E. MenuItemAction Class** (`menuitemaction.hpp/.cpp`)

**Base class for item actions:**
```cpp
class MenuItemAction {
    virtual void fire() { /* override */ }
};
```

**Implementations:**
- `SaveResolutionAction` - Save resolution settings
- `SaveVSyncAction` - Save VSync settings
- `ResetAction` - Reset game data (times, positions, tracks)

---

#### **F. MenuManager Class** (`menumanager.hpp/.cpp`)

**Singleton Pattern:** Static instance `MenuManager::instance()`

**Menu Stack Management:**
```cpp
std::vector<MenuPtr> m_menuStack;        // Stack of active menus
std::map<std::string, MenuPtr> m_idToMenuMap;  // ID -> Menu mapping
MenuPtr m_prevMenu;                       // Previous menu (for animations)
```

**Key Methods:**
- `addMenu(MenuPtr)` - Register menu by ID
- `getMenuById(menuId)` - Retrieve menu
- `enterMenu(menuId)` - Clear stack, enter menu
- `pushMenu(menuId)` - Push onto stack
- `popMenu()` - Pop from stack
- `popToMenu(menuId)` - Pop until specific menu
- `activeMenu()` - Get current top menu
- `render()` - Render active menu
- `up(), down(), left(), right()` - Delegate to active menu
- `selectCurrentItem()` - Delegate to active menu
- `stepTime(milliseconds)` - Update all menus in stack
- `mousePress/Release()` - Handle mouse input
- `isDone()` - Check if exit requested

**Menu Lifecycle Callbacks:**
- `enter()` - Called when menu becomes active
- `pushEnter()` - Called when pushed onto stack (with slide animation)
- `pushExit()` - Called when being pushed away
- `popEnter()` - Called when popped back (with slide animation)
- `popExit()` - Called when popped away

---

#### **G. AnimationCurve Class** (`animationcurve.hpp/.cpp`)

**Purpose:** Generate smooth easing curves for animations

**Constructor:**
```cpp
AnimationCurve(size_t steps, int exponent = 2);
```

**Easing Formula:**
```cpp
value = (index / (steps - 1)) ^ exponent
```

**Examples:**
- `AnimationCurve(15, 3)` - 15 steps, cubic easing (menu transitions)
- `AnimationCurve(30, 3)` - 30 steps, cubic easing (item animations)

**Methods:**
- `step()` - Advance to next value
- `reset()` - Start from beginning
- `value()` - Get current easing value (0.0 to 1.0)

**Exponent Values Common in Menus:**
- Exponent 2 = Quadratic (gentle acceleration)
- Exponent 3 = Cubic (more dramatic acceleration)

---

### **SurfaceMenu Class** (`surfacemenu.cpp/.hpp`)

**Inheritance:** `MTFH::Menu`

**Constructor:**
```cpp
SurfaceMenu(
    std::string surfaceId,    // Background surface asset ID
    std::string id,           // Menu ID
    int width,
    int height,
    Menu::Style style = Menu::Style::VerticalList,
    bool quitItem = true,     // Add X button
    bool prevItem = false,    // Add < button
    bool nextItem = false     // Add > button
);
```

**Mouse Control Items:**
- Quit button: 40×40 px, displays "X", positioned at top-right
- Prev button: 40×40 px, displays "<", positioned at left-center
- Next button: 40×40 px, displays ">", positioned at right-center

**Animations:**
```cpp
// Normal enter: from (0, 0) to (0, 0)
void enter() {
    setPos(0, 0, 0, 0);
}

// Push onto stack: slide from right
void pushEnter() {
    setPos(width(), 0, 0, 0);
}

// Push exit: slide left
void pushExit() {
    setPos(0, 0, -width(), 0);
}

// Pop from stack: slide from left
void popEnter() {
    setPos(-width(), 0, 0, 0);
}

// Pop exit: slide right
void popExit() {
    setPos(0, 0, width(), 0);
}
```

**Background Rendering:**
```cpp
m_back->setShaderProgram(Renderer::instance().program("menu"));
m_back->setColor({0.5, 0.5, 0.5, 1.0});  // 50% grey tint
m_back->setSize(width(), width() * m_back->height() / m_back->width());  // Aspect-correct
m_back->render(nullptr, {x() + width() / 2, y() + height() / 2, 0}, 0);
```

**Background Size Calculation:** Maintains aspect ratio, scales to width

---

## **PART 3: LAYOUT POSITIONING**

### **Vertical List Layout**
```
Total height of all items calculated
Start Y = centerY - (totalHeight / 2) + (itemSpacing / 2)
Each item: x = centerX, y = startY + offset
```

### **Horizontal List Layout**
```
Total width of all items calculated
Start X = centerX - (totalWidth / 2) + (itemSpacing / 2)
Each item: x = startX + offset, y = centerY
```

### **ShowOne Layout**
```
Single item displayed
Position: centerX, centerY
```

### **ShowMany Layout**
```
Multiple items shown via setItemsToShow([indices])
Each item uses its own setPos() call
```

---

## **PART 4: STRING LITERALS & TRANSLATIONS**

**All translatable strings use:** `QObject::tr("String").toUpper().toStdWString()`

**Menu Item Text:**
- "PLAY" / "HELP" / "CREDITS" / "SETTINGS" / "QUIT"
- "EASY" / "MEDIUM" / "HARD"
- "CHOOSE LAP COUNT"
- "ONE PLAYER RACE" / "TWO PLAYER RACE" / "TIME TRIAL" / "DUEL"
- "GAME MODE >" / "SOUNDS >" / "GFX >" / "CONTROLS >" / "RESET >"
- "FULL SCREEN RESOLUTION >" / "WINDOWED RESOLUTION >"
- "FPS >" / "SPLIT TYPE >" / "VSYNC >"
- "30 FPS" / "60 FPS"
- "VERTICAL" / "HORIZONTAL"
- "ON" / "OFF"
- "PLAYER ONE ACCELERATE" / "PLAYER ONE BRAKE" / "PLAYER ONE TURN LEFT" / "PLAYER ONE TURN RIGHT"
- "PLAYER TWO ACCELERATE" / "PLAYER TWO BRAKE" / "PLAYER TWO TURN LEFT" / "PLAYER TWO TURN RIGHT"
- "RESET RECORD TIMES" / "RESET BEST POSITIONS" / "RESET UNLOCKED TRACKS"
- "OK" / "CANCEL"
- "PRESS A KEY.."

**Confirmation Texts:**
- "Restart to change the resolution."
- "Restart to change VSync setting."
- "Reset record times?"
- "Reset best positions?"
- "Reset unlocked tracks?"

**Track Properties:**
- "Laps: X"
- "Length: X"
- "Lap Record: HH:MM:SS.mmm"
- "Race Record: HH:MM:SS.mmm"
- "Finish previous track in TOP-6 to unlock!"
- "Unlock it in one/two player race!"

---

## **PART 5: COLOR SPECIFICATIONS (RGB)**

| Component | State | Color (R, G, B, A) | Hex |
|-----------|-------|-------------------|-----|
| Text (MenuItem) | Focused | (1.0, 1.0, 0.0, 1.0) | #FFFF00 |
| Text (MenuItem) | Selected | (1.0, 0.0, 0.0, 1.0) | #FF0000 |
| Text (MenuItem) | Normal | (1.0, 1.0, 1.0, 1.0) | #FFFFFF |
| Confirmation Text | Default | (0.25, 0.75, 1.0, 1.0) | #4DBFFF |
| Help/Credits Text | Default | (1.0, 1.0, 1.0, 1.0) | #FFFFFF |
| SurfaceMenu Background | Default | (0.5, 0.5, 0.5, 1.0) | #808080 |
| Track Stars | Active | (1.0, 1.0, 0.0) | #FFFF00 |
| Track Stars | Inactive | (0.75, 0.75, 0.75) | #BFBFBF |
| Track Preview (Unlocked) | Default | (1.0, 1.0, 1.0) | #FFFFFF |
| Track Preview (Locked) | Default | (0.5, 0.5, 0.5) | #808080 |

---

## **PART 6: TIMING VALUES**

| Event | Timing |
|-------|--------|
| Text animation frequency | m_angle += 0.010f per frame |
| Text amplitude | 5% of base size (`0.05f`) |
| Text focused scale-up | 1.25× |
| Credits rotation interval | 120 frames (2 seconds @ 60 FPS) |
| Menu slide animation | 15 steps, cubic exponent (3) |
| Menu item animation | 30 steps, cubic exponent (3) |
| Track selection slide distance | 1000 pixels |
| Position animation formula | `new = old + (target - old) * curve_value` |

---

## **PART 7: STFH FRAMEWORK (SOUND/AUDIO)**

**Directory:** `src/game/STFH/`

**Note:** STFH is a sound/audio framework with minimal menu integration. It provides:
- **Data.cpp/.hpp** - Audio data management
- **Device.cpp/.hpp** - Audio device handling  
- **Listener.cpp/.hpp** - Audio listener positioning
- **Location.cpp/.hpp** - 3D sound positioning
- **Source.cpp/.hpp** - Audio source management

**Menu Integration:** Settings menu provides ON/OFF toggle for sounds via `Game::instance().audioWorker().setEnabled(bool)`

**Persistence:** Sounds preference saved via `Settings::instance().saveValue(Settings::soundsKey(), bool)`

---

## **SUMMARY**

### **Key Architectural Patterns**

1. **Stack-based Menu Navigation**: MenuManager maintains a stack; pushing/popping enables sub-menus
2. **Smooth Animations**: All menu transitions use AnimationCurve with easing functions
3. **Item View Separation**: MenuItem delegates rendering to MenuItemView implementations
4. **Action-based Item Behavior**: MenuItemAction decouples item selection from behavior
5. **Surface Backgrounds**: All game menus use textured backgrounds for visual consistency
6. **Mouse Support**: Optional prev/next/quit buttons for mouse-driven navigation
7. **Wide-char Strings**: All menu text uses `std::wstring` (L"...") for Unicode support

### **File Organization**
- **Menu Screens:** `src/game/menu/*.cpp/.hpp` (17 files)
- **MTFH Framework:** `src/game/MTFH/*.cpp/.hpp` (7 files)
- **STFH Framework:** `src/game/STFH/*.cpp/.hpp` (5 files)
