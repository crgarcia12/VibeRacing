# DUSTRACING2D MENU SYSTEM - TECHNICAL REFERENCE

## QUICK REFERENCE TABLES

### Menu Hierarchy
`
Main ("main")
├── Play → Difficulty ("difficulty")
│   └── Lap Count ("lapCount")
│       └── Track Selection ("trackSelection")
│           └── [Start Race]
├── Help ("help")
├── Credits ("credits")
├── Settings ("settings")
│   ├── Game Mode ("gameModeMenu")
│   │   ├── One Player Race
│   │   ├── Two Player Race
│   │   ├── Time Trial
│   │   └── Duel
│   ├── Sounds ("sfxMenu")
│   │   ├── On
│   │   └── Off
│   ├── GFX ("gfxMenu")
│   │   ├── Full Screen Resolution ("fullScreenResolutionMenu")
│   │   ├── Windowed Resolution ("windowedResolutionMenu")
│   │   ├── FPS ("fpsMenu")
│   │   │   ├── 30 FPS
│   │   │   └── 60 FPS
│   │   ├── Split Type ("splitTypeMenu")
│   │   │   ├── Horizontal
│   │   │   └── Vertical
│   │   └── VSync ("vsyncMenu") [Qt 5.3+]
│   ├── Controls ("keyConfigMenu")
│   │   ├── Player 1 Config
│   │   └── Player 2 Config
│   ├── Reset ("resetMenu")
│   │   ├── Reset Record Times → Confirmation
│   │   ├── Reset Best Positions → Confirmation
│   │   └── Reset Unlocked Tracks → Confirmation
│   └── [Confirmation Menu "confirmationMenu"]
└── Quit

`

### All Menu IDs & Styles

| ID | Class | Style | Background | Items |
|----|-------|-------|------------|-------|
| main | MainMenu | VerticalList | mainMenuBack | 5 |
| difficulty | DifficultyMenu | VerticalList | trackSelectionBack | 3 |
| lapCount | LapCountMenu | HorizontalList | trackSelectionBack | 7 |
| trackSelection | TrackSelectionMenu | ShowMany | trackSelectionBack | N |
| settings | SettingsMenu | VerticalList | settingsBack | 5 |
| gameModeMenu | SurfaceMenu | VerticalList | settingsBack | 4 |
| sfxMenu | SurfaceMenu | VerticalList | settingsBack | 2 |
| gfxMenu | SurfaceMenu | VerticalList | settingsBack | 5 |
| fpsMenu | SurfaceMenu | VerticalList | settingsBack | 2 |
| splitTypeMenu | SurfaceMenu | VerticalList | settingsBack | 2 |
| fullScreenResolutionMenu | ResolutionMenu | VerticalList | settingsBack | N |
| windowedResolutionMenu | ResolutionMenu | VerticalList | settingsBack | N |
| vsyncMenu | VSyncMenu | VerticalList | settingsBack | 2 |
| keyConfigMenu | KeyConfigMenu | VerticalList | settingsBack | N |
| resetMenu | SurfaceMenu | VerticalList | settingsBack | 3 |
| confirmationMenu | ConfirmationMenu | HorizontalList | settingsBack | 2 |
| help | Help | VerticalList | helpBack | 0 |
| credits | Credits | VerticalList | creditsBack | 0 |

---

### Color Palette Reference

**Text States**:
`cpp
MCGLColor focused    = {1.0f, 1.0f, 0.0f, 1.0f}; // YELLOW
MCGLColor selected   = {1.0f, 0.0f, 0.0f, 1.0f}; // RED
MCGLColor normal     = {1.0f, 1.0f, 1.0f, 1.0f}; // WHITE
`

**UI Elements**:
`cpp
MCGLColor confirmText = {0.25f, 0.75f, 1.0f, 1.0f}; // CYAN
MCGLColor background  = {0.5f, 0.5f, 0.5f, 1.0f};  // GREY 50%
`

**Track Selection**:
`cpp
MCGLColor lockedTrack    = {0.5f, 0.5f, 0.5f};     // GREY 50%
MCGLColor unlockedTrack  = {1.0f, 1.0f, 1.0f};     // WHITE
MCGLColor starEarned     = {1.0f, 1.0f, 0.0f};     // YELLOW
MCGLColor starUnearned   = {0.75f, 0.75f, 0.75f}; // GREY 75%
`

---

### Text Sizes by Menu

| Menu | Primary (px) | Secondary (px) | Tertiary (px) |
|------|---|---|---|
| Main | 40 | - | - |
| Difficulty | 40 | - | - |
| Lap Count | 40 | - | - |
| Track Select | 30 | 20 | 20 |
| Settings | 20 | - | - |
| GFX/SFX/Mode | 20 | - | - |
| FPS/Split | 20 | - | - |
| Confirmation | 20 | 20 | - |
| Help | 20*(h/640) | - | - |
| Credits | 20*(h/640) | - | - |

---

### Dimension Formulas

`cpp
// Main menu items
itemHeight = screenHeight / 8
itemWidth = screenWidth

// Settings menu items
itemHeight = screenHeight / 10
itemWidth = screenWidth

// Track selection items
itemWidth = screenWidth / 2
itemHeight = screenHeight / 2

// Lap count items
numCounts = 7
itemWidth = screenWidth / (numCounts + 2)  // ~screenWidth / 9
itemHeight = screenHeight / (numCounts + 2) // ~screenHeight / 9

// Confirmation dialog items
itemWidth = screenWidth / 4
itemHeight = screenHeight

// Mouse items (quit, prev, next)
itemWidth = 40
itemHeight = 40
`

---

### Animation Parameters

`cpp
// Default animation curve
steps = 15
exponent = 3 (cubic easing)

// Track selection animations
slideDistance = 1000 pixels
animationSteps = 15
animationExp = 3

// Text item animation
angleIncrement = 0.010 radians/frame
amplitudePercent = 5% (0.05)
focusedScale = 1.25x

// Credits carousel
cycleDuration = 120 frames
`

---

### Derived Calculations

**Track Item Star Position**:
`cpp
numStars = 10
starW = star->width()
startX = centerX - 5*starW + starW/2
for (i = 0 to numStars-1) {
  starX[i] = startX + i*starW
}
`

**Track Item Title Position**:
`cpp
titleX = centerX - titleWidth/2
titleY = centerY + itemHeight/2 + titleHeight
`

**Track Item Properties Position**:
`cpp
baseY = centerY - itemHeight/2
propX = centerX
propY[0] = baseY - 2*lineHeight
propY[1] = baseY - 3*lineHeight
// etc, incrementing down
`

---

### Input Processing Flow

`
Input Event (Key/Mouse)
    ↓
EventHandler catches input
    ↓
MenuManager::up/down/left/right/selectCurrentItem()
    ↓
Menu::up/down/left/right/selectCurrentItem()
    ↓
MenuItem::setCurrent() OR MenuItem action triggered
    ↓
MenuItemView renders with new state
    ↓
Screen update
`

---

### Position Animation Flow

`
MenuItem::setPos(x, y, targetX, targetY)
    ↓
AnimationCurve reset and initialized
    ↓
Each frame: Menu::positionAnimation(msecs)
    ↓
AnimationCurve::step() advances
    ↓
Interpolated position = start + (end-start)*curve.value()
    ↓
MenuItem rendered at interpolated position
    ↓
When curve.value() == 1.0, animation complete
`

---

### State Variables

**Menu Class**:
`cpp
std::string m_id                      // "main", "settings", etc.
int m_width, m_height
float m_x, m_y                        // Current position
float m_targetX, m_targetY            // Animation target
int m_currentIndex                    // Focused item index
int m_selectedIndex                   // Last selected item
Style m_style                         // VerticalList, HorizontalList, etc.
bool m_isDone                         // Quit menu stack flag
bool m_wrapAround                     // Circular navigation
AnimationCurve m_animationCurve       // Position animation
std::vector<MenuItemPtr> m_items
std::vector<MouseItem> m_mouseItems   // Quit, Prev, Next buttons
`

**MenuItem Class**:
`cpp
std::wstring m_text
float m_width, m_height
float m_x, m_y                        // Current position
float m_targetX, m_targetY            // Animation target
MenuItemViewPtr m_view                // Custom renderer
MenuItemActionPtr m_action            // Action handler
bool m_focused                        // Current (highlighted)
bool m_selected                       // Activated
bool m_selectable
AnimationCurve m_animationCurve
float m_lMargin, m_rMargin, m_tMargin, m_bMargin
int m_index                           // Position in menu
`

**TextMenuItemView Class**:
`cpp
float m_textSize                      // Base glyph size
float m_angle                         // Sine wave animation angle
`

---

## SHADER & RENDERING PIPELINE

### Shader Program
- Name: "menu"
- Applied to: All surfaces, menu items
- Purpose: Specialized menu rendering

### Rendering Order
`
1. Background surface (dimmed)
   - Size: fullWidth × (width*height / bg.width)
   - Position: center
   - Color: RGB(0.5, 0.5, 0.5)

2. Menu items (vertical or horizontal)
   - For each item in m_items:
     - If item focused/selected: animate size+color
     - Call item.render()
     - Call view.render(x, y)

3. Mouse items (overlaid)
   - Quit button (top-right typically)
   - Prev button (left typically)
   - Next button (right typically)
`

---

## ASSET REFERENCES

### Surfaces
`
"mainMenuBack"           - Main menu background
"settingsBack"           - Settings menus background
"helpBack"              - Help menu background
"creditsBack"           - Credits menu background
"trackSelectionBack"    - Track selection & difficulty background
"star"                  - Full star icon (yellow)
"starGlow"              - Star glow effect
"starHalf"              - Half star (left)
"starHalfR"             - Half star (right)
"starHalfGlow"          - Half star glow
"lock"                  - Lock icon for locked tracks
`

### Fonts
`
Game::instance().fontName()  // Retrieved from Game class
`

---

## KEY METHODS & SIGNATURES

### Menu Navigation
`cpp
void Menu::up()
void Menu::down()
void Menu::left()
void Menu::right()
void Menu::selectCurrentItem()
void Menu::exit()
`

### Menu Management
`cpp
void MenuManager::pushMenu(std::string menuId)
void MenuManager::popMenu()
void MenuManager::popToMenu(std::string menuId)
void MenuManager::enterMenu(std::string menuId)
MenuPtr MenuManager::activeMenu() const
void MenuManager::render()
void MenuManager::stepTime(std::chrono::milliseconds timeStep)
`

### MenuItem Actions
`cpp
void MenuItem::setAction(MenuItemActionPtr action)
void MenuItem::setAction(ActionFunction func)
void MenuItem::setMenuOpenAction(std::string menuId)
void MenuItem::setFocused(bool focused)
void MenuItem::setSelected(bool flag)
void MenuItem::setCurrent()
`

### Animation
`cpp
void MenuItem::setPos(float x, float y, float targetX, float targetY)
void MenuItem::positionAnimation(int msecs)
float AnimationCurve::value() const
void AnimationCurve::step()
void AnimationCurve::reset()
`

---

## DATABASE INTEGRATION

### Load Methods
`cpp
std::pair<int, bool> Database::loadLapRecord(Track &track)
std::pair<int, bool> Database::loadRaceRecord(Track &track, int lapCount, Difficulty diff)
std::pair<int, bool> Database::loadBestPos(Track &track, int lapCount, Difficulty diff)
`

### Reset Methods
`cpp
void Database::resetLapRecords()
void Database::resetRaceRecords()
void Database::resetBestPos()
void Database::resetTrackUnlockStatuses()
`

### Track Locking
`cpp
bool TrackData::isLocked() const
void TrackData::setIsLocked(bool locked)
`

---

## SETTINGS PERSISTENCE

### Settings Keys
`cpp
QString Settings::lapCountKey()          // Lap count
QString Settings::soundsKey()            // Audio enabled
QString Settings::fpsKey()               // 30 or 60 fps
`

### Save Methods
`cpp
void Settings::saveValue(QString key, QVariant value)
QVariant Settings::loadValue(QString key)
void Settings::saveDifficulty(Difficulty diff)
void Settings::saveResolution(int w, int h, bool fullScreen)
`

### Game State
`cpp
void Game::setFps(Game::Fps fps)
void Game::setMode(Game::Mode mode)
void Game::setLapCount(int count)
void Game::setSplitType(Game::SplitType type)
void Game::difficultyProfile().setDifficulty(Difficulty diff)
`

---

## COMPILATION FLAGS

`cpp
#ifdef VSYNC_MENU
    // VSync menu included (Qt 5.3+)
#endif

// Note: Must be saved as UTF-8 with BOM (MSVC)
// for wide character string literals to work
`

---

## COMMON PATTERNS

### Creating a Menu Item
`cpp
auto item = std::make_shared<MenuItem>(width, height, L"TEXT");
item->setView(std::make_shared<TextMenuItemView>(20, *item));
item->setAction([](){ /* lambda code */ });
menu->addItem(item);
`

### Creating a Submenu Item
`cpp
auto item = std::make_shared<MenuItem>(width, height, L"SUBMENU >");
item->setView(std::make_shared<TextMenuItemView>(20, *item));
item->setMenuOpenAction("submenuId");
menu->addItem(item);
`

### Position Animation
`cpp
item->setPos(startX, startY, targetX, targetY);
item->resetAnimationCurve(15, 3);  // 15 steps, cubic easing
`

### Confirmation Dialog
`cpp
MenuManager::instance().pushMenu(confirmationMenu->id());
confirmationMenu->setText(L"Confirm action?");
confirmationMenu->setAcceptAction([](){ /* do something */ });
confirmationMenu->setCurrentIndex(1);  // Focus Cancel
`

---

## PERFORMANCE NOTES

- Menu animations run at frame rate (60fps or 30fps configurable)
- Position interpolation uses pre-calculated exponential curve
- Text rendering cached via MCTextureFont
- Star rendering uses glow overlay technique
- All menus stored in MenuManager singleton map (no runtime allocation)

---

## TRANSLATION SYSTEM

All user-facing strings use Qt tr() macro:
`cpp
tr("PLAY")
tr("Settings")
tr("Choose lap count")
tr("Reset record times?")
// etc.
`

Translations stored separately, referenced by context.

---

## END OF TECHNICAL REFERENCE
