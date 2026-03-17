# DUSTRACING2D MENU SYSTEM - COMPREHENSIVE SPECIFICATION

## 1. FRAMEWORK ARCHITECTURE (MTFH - Menu Texture Framework)

### Core Classes

**Menu (menu.hpp/cpp)**
- Main menu controller
- Properties: id, width, height, position (x,y), currentIndex, selectedIndex
- Styles: VerticalList, HorizontalList, ShowOne, ShowMany
- Animation: position-based with AnimationCurve (15 steps, exp=3)
- Navigation: up/down/left/right, selectCurrentItem(), exit()

**MenuItem (menuitem.hpp/cpp)**
- Base menu item
- Properties: width, height, text (wstring), position (x,y), targetX, targetY
- State: focused (bool), selected (bool), selectable (bool)
- View: MenuItemViewPtr (custom rendering)
- Action: MenuItemActionPtr or ActionFunction (lambda)
- Margins: left, right, top, bottom

**MenuItemView (menuitemview.hpp)**
- Abstract base for rendering
- Pure virtual: render(float x, float y)
- Optional: stepTime(int msecs) for animations

**TextMenuItemView (textmenuitemview.hpp/cpp)**
- Concrete text renderer
- Sine wave animation: angle += 0.010f per frame
- Amplitude: 5% of text size
- Colors:
  - Focused: YELLOW (1.0, 1.0, 0.0, 1.0)
  - Selected: RED (1.0, 0.0, 0.0, 1.0)
  - Normal: WHITE (1.0, 1.0, 1.0, 1.0)
- Focused scale: 1.25x
- Shadow offset: (2, -2) pixels

**AnimationCurve (animationcurve.hpp/cpp)**
- Exponential easing
- Constructor: AnimationCurve(steps, exponent)
- Methods: step(), value() [0.0-1.0], reset()
- Default: 15 steps, exp=3 (cubic)

**SurfaceMenu (menu/surfacemenu.hpp/cpp)**
- Base class for textured backgrounds
- Background surface: retrieved by surfaceId
- Color overlay: (0.5, 0.5, 0.5) - dimmed
- Optional mouse items: Quit (X), Prev (<), Next (>)
- Slide animations: push/pop enter/exit

**MenuManager (menumanager.hpp)**
- Singleton menu stack controller
- Methods: pushMenu(), popMenu(), popToMenu(), enterMenu()
- Render pipeline for active menu

---

## 2. MAIN MENU

**File**: menu/mainmenu.hpp/cpp
**ID**: "main"
**Background**: "mainMenuBack"
**Style**: VerticalList
**Item height**: height / 8
**Text size**: 40

**Menu Items** (added in reverse, displayed bottom-to-top):
1. Play → "difficulty" submenu
2. Help → "help" submenu
3. Credits → "credits" submenu
4. Quit → exitGameRequested() signal
5. Settings → "settings" submenu

**Sub-menus created**: Help, Credits, LapCountMenu, SettingsMenu, TrackSelectionMenu, DifficultyMenu

---

## 3. TRACK SELECTION MENU

**File**: menu/trackselectionmenu.hpp/cpp
**ID**: "trackSelection"
**Background**: "trackSelectionBack"
**Style**: ShowMany (display multiple items)
**Animation**:
  - Slide distance: 1000 pixels
  - Steps: 15, Exponent: 3
  - Wrap around: false (no circular nav)

**TrackItem Class** (custom MenuItem rendering):

#### Rendering Layers (from back to front):

**1. Track Tile Preview**
- Map grid visualization
- Tile size: maintain square aspect
- Locked track color: RGB(0.5, 0.5, 0.5)
- Unlocked track color: RGB(1.0, 1.0, 1.0)
- Centered in item bounds

**2. Track Title**
- Text: track name uppercase
- Glyph: 30x30 pixels
- Position: center X, Y = height/2 + text.height()
- Shadow: offset (2, -2)
- Centered horizontally

**3. Star Rating** (unlocked tracks only)
- 10 stars total
- Spacing: star_width apart
- Start X: center - 5*star_width + star_width/2
- Best position 1-10: YELLOW (1.0, 1.0, 0.0) + glow
- Position 11: HALF STAR (yellow left, grey right)
- Position 12+: GREY (0.75, 0.75, 0.75)
- Position not set: all grey

**4. Lock Icon** (locked tracks only)
- Position: center item
- Displayed when track.isLocked()

**5. Track Properties Text**
- Glyph: 20x20 pixels
- Shadow: offset (2, -2)
- Properties (if unlocked):
  - "       Laps: " + lap_count
  - "     Length: " + distance_meters
  - " Lap Record: " + time_string
  - "Race Record: " + time_string
- Unlock message (if locked):
  - With AI: "Finish previous track in TOP-6 to unlock!"
  - Multiplayer: "Unlock it in one/two player race!"

**Navigation**:
- Left/Right: previous/next track
- Animation: slide out current (±1000px), slide in new
- Both tracks visible during transition

---

## 4. SETTINGS MENU

**File**: menu/settingsmenu.hpp/cpp
**ID**: "settings"
**Background**: "settingsBack"
**Style**: VerticalList
**Item height**: height / 10
**Text size**: 20

**Main Menu Items** (added in reverse):
1. Reset > → resetMenu
2. Controls > → keyConfigMenu
3. Sounds > → sfxMenu
4. GFX > → gfxMenu
5. Game mode > → gameModeMenu

### Submenu: Game Mode (gameModeMenu)
- Items: One Player Race, Two Player Race, Time Trial, Duel

### Submenu: Sounds (sfxMenu)
- Items: On, Off
- Toggled: AudioWorker.setEnabled()

### Submenu: Graphics (gfxMenu)
- Items:
  - VSync > (Qt 5.3+)
  - Split type >
  - FPS >
  - Windowed resolution >
  - Full screen resolution >

**Sub-submenu: FPS (fpsMenu)**
- Items: 30 FPS, 60 FPS
- Saves: Settings::fpsKey()

**Sub-submenu: Split Type (splitTypeMenu)**
- Items: Horizontal, Vertical

**Sub-submenu: Resolutions (fullScreenResolutionMenu, windowedResolutionMenu)**
- Dynamic based on available resolutions
- Confirmation menu for changes

### Submenu: Reset (resetMenu)
- Items (in order):
  1. Reset record times → confirms, resets lap/race records
  2. Reset best positions → confirms, resets position records
  3. Reset unlocked tracks → confirms, relocks all tracks

- Uses ConfirmationMenu with messages:
  - "Reset record times?"
  - "Reset best positions?"
  - "Reset unlocked tracks?"

---

## 5. DIFFICULTY MENU

**File**: menu/difficultymenu.hpp/cpp
**ID**: "difficulty"
**Background**: "trackSelectionBack"
**Style**: VerticalList
**Item height**: height / 8
**Text size**: 40

**Items** (added in reverse):
1. Hard → Easy, save, open lapCount
2. Medium → Medium, save, open lapCount
3. Easy → Hard, save, open lapCount

Initial selection: auto-selects current difficulty

---

## 6. LAP COUNT MENU

**File**: menu/lapcountmenu.hpp/cpp
**ID**: "lapCount"
**Background**: "trackSelectionBack"
**Style**: HorizontalList
**Item dimensions**: width/(numItems+2) × height/(numItems+2)
**Text size**: 40

**Available counts**: [1, 3, 5, 10, 20, 50, 100]

**Each item**:
- Text: lap count string
- Action: set count, save, open trackSelection

**Custom render**:
- Title: "Choose lap count"
- Glyph: 30x30
- Position: X = width/2, Y = height/2 + text.height()*2

---

## 7. HELP MENU

**File**: menu/help.hpp/cpp
**ID**: "help"
**Background**: "helpBack"
**Style**: VerticalList

**Content** (centered, scrolling text):
`
GAME GOAL
You are racing against eleven computer players.
Your best position will be the next start position.
Finish in TOP-6 to unlock a new race track!

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

[WEBSITE URL]
`

- Glyph size: 20 × (height / 640)
- Centered: X - width/2, Y + height/2
- Shadow: (2, -2)

---

## 8. CREDITS MENU

**File**: menu/credits.hpp/cpp
**ID**: "credits"
**Background**: "creditsBack"
**Animation**: Rotate through sections every 120 frames

**Sections**:
1. Programming - Jussi Lind
2. Graphics - Jussi Lind, Ville Mäkiranta
3. Level Design - Jussi Lind, Wuzzy
4. Translations - Pavel Fric (cs), Wuzzy (de), Paolo Straffi (it), Jussi Lind (fi), Rémi Verschelde (fr)
5. Patches - 8 contributors listed
6. Special Thanks - Tommi Martela, Alex Rietveld, Matthias Mailänder

- Margin width: variable [8, 13, 5, 14, 14, 2] spaces
- Glyph size: 20 × (height / 640)
- Centered: X - width/2, Y + height/2
- 120 frame cycle between sections

---

## 9. CONFIRMATION MENU

**File**: menu/confirmationmenu.hpp/cpp
**Style**: HorizontalList
**Background**: "settingsBack"
**No quit button**: quitItem=false

**Items**:
- Ok (width/4) → custom action
- Cancel (width/4) → close menu

**Default index**: 1 (Cancel focused)

**Text overlay** (cyan):
- Color: RGB(0.25, 0.75, 1.0, 1.0)
- Glyph: 20x20
- Position: X + width/2 + 20, Y + height/2 + 60
- Shadow: (2, -2)

---

## 10. ANIMATION & TRANSITIONS

### Position Animation (Slide-in/out)

**Enter main menu**:
- Start: (0, 0)
- End: (0, 0)

**Push submenu**:
- Current exits: (0, 0) → (-width, 0) [slide left]
- New enters: (width, 0) → (0, 0) [slide right]

**Pop to parent**:
- Current exits: (0, 0) → (width, 0) [slide right]
- Parent enters: (-width, 0) → (0, 0) [slide left]

**Duration**: 15 frames
**Easing**: Cubic (exp=3)

### Text Item Animation

**Per-frame**:
`
angle += 0.010 radians
size = baseSize + sin(angle) * baseSize * 0.05
if (focused) size *= 1.25
`

---

## 11. COLOR VALUES (RGBA)

| Element | R | G | B | A |
|---------|---|---|---|---|
| Focused text | 1.0 | 1.0 | 0.0 | 1.0 (YELLOW) |
| Selected text | 1.0 | 0.0 | 0.0 | 1.0 (RED) |
| Normal text | 1.0 | 1.0 | 1.0 | 1.0 (WHITE) |
| Confirmation text | 0.25 | 0.75 | 1.0 | 1.0 (CYAN) |
| Menu background | 0.5 | 0.5 | 0.5 | 1.0 (GREY 50%) |
| Stars earned | 1.0 | 1.0 | 0.0 | 1.0 (YELLOW) |
| Stars unearned | 0.75 | 0.75 | 0.75 | 1.0 (GREY 75%) |
| Track locked | 0.5 | 0.5 | 0.5 | 1.0 (GREY 50%) |
| Track unlocked | 1.0 | 1.0 | 1.0 | 1.0 (WHITE) |

---

## 12. TEXT SIZES & POSITIONING

| Menu | Item Size (px) | Container Height | Position Strategy |
|------|---|---|---|
| Main | 40 | height/8 | Centered, vertically stacked |
| Settings | 20 | height/10 | Left-aligned text, ">>" indicator |
| Track | - | height/2 | Tiles centered, properties below |
| Difficulty | 40 | height/8 | Centered, vertically stacked |
| Lap Count | 40 | height/9 | Centered, horizontally stacked |
| Help | 20*(h/640) | - | Centered, multi-line |
| Credits | 20*(h/640) | - | Centered, rotating sections |

---

## 13. SHADER & RENDERING

**Shader program**: "menu" (set on all surfaces/items)

**Background rendering**:
- Size: full width × (width * height / background.width)
- Position: center menu position
- Color: RGB(0.5, 0.5, 0.5) dimmed overlay

**Text rendering**:
- Font: MCAssetManager::textureFontManager()
- Shadow: offset (2, -2)
- Glyph size: configured per menu item

---

## 14. KEY BINDINGS & CONTROLS

**Player 1**:
- Left: Arrow Left
- Right: Arrow Right
- Accelerate: Arrow Up / Right Shift
- Brake: Arrow Down / Right Ctrl

**Player 2**:
- Left: A
- Right: D
- Accelerate: W / Left Shift
- Brake: S / Left Ctrl

**UI**:
- Up/Down: Menu navigation
- Left/Right: Horizontal menu nav or item adjustment
- Enter: Select item / confirm
- Esc/Q: Quit or return to parent menu

---

## 15. DATABASE & PERSISTENCE

**Lap Record**: Database::loadLapRecord(track)
**Race Record**: Database::loadRaceRecord(track, lapCount, difficulty)
**Best Position**: Database::loadBestPos(track, lapCount, difficulty)
**Track Locked**: trackData.isLocked()

**Settings saved**:
- Difficulty: Settings::saveDifficulty()
- Lap count: Settings::saveValue(lapCountKey)
- Sounds: Settings::saveValue(soundsKey)
- FPS: Settings::saveValue(fpsKey)
- Resolution: Settings::saveResolution()
- Split type: Game::setSplitType()
- Game mode: Game::setMode()

---

## 16. FILE REFERENCES

**Menu files**: src/game/menu/
- mainmenu.hpp/cpp
- trackselectionmenu.hpp/cpp
- settingsmenu.hpp/cpp
- difficultymenu.hpp/cpp
- lapcountmenu.hpp/cpp
- help.hpp/cpp
- credits.hpp/cpp
- confirmationmenu.hpp/cpp
- keyconfigmenu.hpp/cpp
- resolutionmenu.hpp/cpp
- vsyncmenu.hpp/cpp
- surfacemenu.hpp/cpp
- textmenuitemview.hpp/cpp

**Framework files**: src/game/MTFH/
- menu.hpp/cpp
- menuitem.hpp/cpp
- menuitemview.hpp/cpp
- menuitemaction.hpp/cpp
- menumanager.hpp/cpp
- animationcurve.hpp/cpp

