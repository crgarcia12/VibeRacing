# DUSTRACING2D MENU SYSTEM - QUICK START GUIDE

## Start Here: 30-Second Overview

**What is this?** Complete reverse-engineering documentation of the DustRacing2D game menu system - every screen, color, animation, and layout detail.

**Where are the files?** 
C:\gitrepos\github\crgarcia12\DustRacing2D\

**Files generated** (4 documents, ~59 KB):
1. MENU_DOCUMENTATION_INDEX.md - Overview & navigation
2. MENU_SPECIFICATION.md - Complete specification  
3. MENU_TECHNICAL_REFERENCE.md - Quick reference tables
4. MENU_VISUAL_LAYOUT.md - Visual layouts & design

---

## What You'll Find

### Main Menu Screens (8)
- Main (5 items: Play, Help, Credits, Settings, Quit)
- Difficulty (3 levels: Easy, Medium, Hard)
- Lap Count (7 options: 1, 3, 5, 10, 20, 50, 100)
- Track Selection (custom rendering with star ratings)
- Help (static scrolling text)
- Credits (6 rotating sections, 120-frame cycle)
- Settings (5 categories with 9 submenus)
- Confirmation (yes/no dialog)

### All Colors (RGB Values)
- Focused: YELLOW (1.0, 1.0, 0.0)
- Selected: RED (1.0, 0.0, 0.0)
- Normal: WHITE (1.0, 1.0, 1.0)
- Confirmation: CYAN (0.25, 0.75, 1.0)
- Background: GREY (0.5, 0.5, 0.5)
- Stars: YELLOW earned / GREY unearned

### All Text Sizes
- Main Menu: 40px
- Difficulty: 40px
- Lap Count: 40px
- Settings: 20px
- Track Title: 30px
- Track Properties: 20px

### All Animations
- Text pulse: sine wave ±5%
- Focus scale: 1.25x (25% larger)
- Position slide: 1000px in 15 frames (cubic easing)
- Menu transitions: smooth slide-in/out

### All Layouts
- Main Menu: Vertical stack, centered
- Track Selection: Centered item with complex rendering
- Settings: Vertical stack, full width items
- Lap Count: Horizontal grid (7 items)
- Confirmation: Horizontal (Ok | Cancel)

---

## Quick Reference: Key Values

### Dimensions
`
Main Menu Item Height:     screen_height / 8
Settings Item Height:      screen_height / 10
Track Item:                screen_width/2 × screen_height/2
Lap Count Item:            screen_width/9 × screen_height/9
`

### Animation Defaults
`
Steps:                     15
Easing:                    cubic (exp=3)
Text Animation:            angle += 0.010 rad/frame
Text Amplitude:            5% (0.05)
`

### Colors (All RGB 0.0-1.0)
`
Yellow (Focused):          (1.0, 1.0, 0.0)
Red (Selected):            (1.0, 0.0, 0.0)
White (Normal):            (1.0, 1.0, 1.0)
Cyan (Text):               (0.25, 0.75, 1.0)
Grey (Background):         (0.5, 0.5, 0.5)
`

### Shadow Offset
`
All Text:                  (2px right, 2px down)
`

---

## Framework (MTFH)

The menu system uses MTFH (Menu Texture Framework) with these core classes:

**Menu** - Controller
- Manages items, navigation, rendering
- Styles: VerticalList, HorizontalList, ShowOne, ShowMany

**MenuItem** - Individual item
- Has text, position, view, action
- State: focused, selected, current

**MenuItemView** - Renderer
- Abstract base for custom rendering
- TextMenuItemView: text with animation

**AnimationCurve** - Easing
- Exponential interpolation table
- Default: 15 steps, cubic

**MenuManager** - Stack controller
- Singleton managing menu stack
- Push/pop/enter operations

---

## File Structure

**Menu Implementation** (12 files)
`
src/game/menu/
  mainmenu.hpp/cpp
  trackselectionmenu.hpp/cpp
  settingsmenu.hpp/cpp
  difficultymenu.hpp/cpp
  lapcountmenu.hpp/cpp
  help.hpp/cpp
  credits.hpp/cpp
  confirmationmenu.hpp/cpp
  keyconfigmenu.hpp/cpp
  resolutionmenu.hpp/cpp
  vsyncmenu.hpp/cpp
  surfacemenu.hpp/cpp
  textmenuitemview.hpp/cpp
`

**Framework** (6 files)
`
src/game/MTFH/
  menu.hpp/cpp
  menuitem.hpp/cpp
  menuitemview.hpp/cpp
  menuitemaction.hpp/cpp
  menumanager.hpp/cpp
  animationcurve.hpp/cpp
`

---

## Navigation Flow

`
Main Menu
├─ Play → Difficulty → Lap Count → Track Selection → [RACE]
├─ Help → [Text] → Back
├─ Credits → [Rotating] → Back
├─ Settings → [Submenus] → Back
│  ├─ Game Mode → [4 options] → Back
│  ├─ Sounds → [On/Off] → Back
│  ├─ GFX → [5 options] → Back
│  ├─ Controls → [Key binding] → Back
│  └─ Reset → [Confirm] → Back
└─ Quit → [EXIT]
`

---

## Track Selection Rendering

Complex 5-layer rendering system:

1. **Tile Preview** - Map grid (color or desaturated)
2. **Title** - Track name, 30px, centered below
3. **Stars** - 10 stars, yellow/grey based on best position
4. **Lock Icon** - Center if locked
5. **Properties** - Laps, length, records, unlock message

---

## Keyboard Controls

`
Up/Down:      Navigate vertically
Left/Right:   Navigate horizontally
Enter:        Select/confirm
Esc/Q:        Back/exit
`

---

## For Extending the System

### To Add a New Menu Item
`cpp
auto item = std::make_shared<MenuItem>(width, height, L"TEXT");
item->setView(std::make_shared<TextMenuItemView>(20, *item));
item->setAction([](){ /* code */ });
menu->addItem(item);
`

### To Create a Submenu
`cpp
item->setMenuOpenAction("submenuId");
`

### To Create an Animation
`cpp
item->setPos(startX, startY, targetX, targetY);
item->resetAnimationCurve(15, 3);  // 15 steps, cubic
`

---

## Asset Requirements

### Surfaces (11 total)
- mainMenuBack
- settingsBack
- helpBack
- creditsBack
- trackSelectionBack
- star
- starGlow
- starHalf
- starHalfR
- starHalfGlow
- lock

### Shader
- "menu" (applied to all surfaces)

### Font
- Retrieved via Game::instance().fontName()

---

## Database Integration

**Load**:
`cpp
Database::loadLapRecord(track)
Database::loadRaceRecord(track, lapCount, difficulty)
Database::loadBestPos(track, lapCount, difficulty)
`

**Reset**:
`cpp
Database::resetLapRecords()
Database::resetRaceRecords()
Database::resetBestPos()
Database::resetTrackUnlockStatuses()
`

---

## Settings Persistence

**Save/Load**:
`cpp
Settings::saveDifficulty(difficulty)
Settings::saveValue(lapCountKey, count)
Settings::saveValue(soundsKey, enabled)
Settings::saveValue(fpsKey, 30 or 60)
Settings::saveResolution(w, h, fullScreen)
`

---

## Translation System

All user strings use Qt tr() macro:
`cpp
tr("Play")
tr("Settings")
tr("Choose lap count")
// etc.
`

Translations stored separately, referenced by context.

---

## Performance Notes

- Menus pre-allocated (no runtime creation)
- AnimationCurve uses pre-calculated table
- Text rendering cached via MCTextureFont
- All operations on main thread
- Frame-based updates (30/60 fps)

---

## Where to Start

**If you need...**

**Quick overview**: Read this file + MENU_DOCUMENTATION_INDEX.md

**All details**: Read MENU_SPECIFICATION.md (11.3 KB)

**Code integration**: Use MENU_TECHNICAL_REFERENCE.md (12.4 KB)

**Visual design**: Check MENU_VISUAL_LAYOUT.md (21.6 KB)

**Specific menu**: Search in MENU_SPECIFICATION.md

**Color values**: See MENU_TECHNICAL_REFERENCE.md §11

**Animation timing**: Check MENU_VISUAL_LAYOUT.md animations section

**Layout formulas**: Look in MENU_TECHNICAL_REFERENCE.md §4-5

**Method signatures**: Find in MENU_TECHNICAL_REFERENCE.md §13

---

## Key Takeaways

✓ 8 main menu screens with 9 submenus
✓ Framework: MTFH (Menu Texture Framework)
✓ Color scheme: Yellow (focus), Red (select), White (normal), Cyan (confirm)
✓ Animation: Sine wave text pulse + cubic easing for positions
✓ Responsive: Height-based scaling for some elements
✓ Database: Integrated for records, positions, unlocking
✓ Settings: Comprehensive persistence system
✓ Framework: Reusable, extensible, well-documented

---

**Start reading**: MENU_DOCUMENTATION_INDEX.md

**Questions?** Check the appropriate specification document above.
