# DUSTRACING2D MENU SYSTEM - COMPLETE DOCUMENTATION INDEX

## Overview

This comprehensive documentation package contains detailed reverse-engineering specifications of the DustRacing2D C++ game menu system. Includes all menu screens, layouts, colors, animations, navigation logic, and component specifications.

---

## Documentation Files

### 1. MENU_SPECIFICATION.md (11.3 KB)
**Comprehensive main specification document**

Contains:
- Framework Architecture (MTFH - Menu Texture Framework)
- Core Component Details (Menu, MenuItem, MenuItemView, AnimationCurve, MenuManager)
- Main Menu Screen (items, actions, navigation)
- Track Selection Menu (TrackItem rendering, star rating, properties)
- Settings Menu System (all submenus and configurations)
- Difficulty Menu (3-level difficulty selection)
- Lap Count Menu (7 selectable lap counts)
- Help Screen (static content, controls)
- Credits Screen (6 rotating sections)
- Confirmation Menu (yes/no dialogs)
- Key Configuration Menu
- Resolution & VSync Menus
- Visual Properties & Colors (RGB values)
- Navigation Logic (input handling, state transitions)
- Animation System (curves, easing functions)
- String Constants & Messages (all UI text)
- Component Sizes & Positions
- State Persistence & Settings
- File Structure Summary
- Key Implementation Details

**Use this for**: Understanding the complete menu architecture and all menu screens

---

### 2. MENU_TECHNICAL_REFERENCE.md (12.4 KB)
**Quick reference and technical deep-dive**

Contains:
- Menu Hierarchy Tree (visual navigation map)
- All Menu IDs & Styles (reference table)
- Color Palette Reference (all RGB values)
- Text Sizes by Menu (comprehensive table)
- Dimension Formulas (calculations for all layouts)
- Derived Calculations (star positions, title positions, property positions)
- Input Processing Flow (event handling pipeline)
- Position Animation Flow (interpolation process)
- State Variables (all member variables documented)
- Shader & Rendering Pipeline (program application)
- Asset References (all surfaces and fonts)
- Key Methods & Signatures (all public methods)
- Database Integration (loading and saving)
- Settings Persistence (keys and save methods)
- Compilation Flags (conditional compilation)
- Common Patterns (code examples)
- Performance Notes (optimization considerations)
- Translation System (i18n implementation)

**Use this for**: Quick lookups, code integration, technical details

---

### 3. MENU_VISUAL_LAYOUT.md (21.6 KB)
**Detailed visual and layout specifications**

Contains:
- Visual Hierarchy & Screen Layouts
  - Main Menu (visual ASCII layout with properties)
  - Track Selection (detailed rendering layers)
  - Settings Menu (hierarchical structure)
  - Difficulty Menu (layout and behavior)
  - Lap Count Menu (horizontal grid layout)
  - Confirmation Dialog (yes/no layout)
- Animation Curves (sine wave and cubic easing visualized)
- Color State Transitions (state machines for UI elements)
- Shadow & Depth Effects (text shadows and overlays)
- Mouse Item Positioning (quit, prev, next buttons)
- Dynamic Text Sizing (height-based scaling formulas)
- Layout Grid System (vertical, centered, horizontal grids)
- Focus Indication Methods (color, size, shadow)
- Screen Transition Effects (push/pop animations)
- Text Alignment & Positioning (formulas for all alignments)
- Track Item Specific (title, stars, properties positioning)
- Performance & Rendering Notes
- Responsive Design (resolution scaling)
- Accessibility Features (keyboard, visual, mouse)

**Use this for**: Visual design, layout implementation, animation timing

---

## Quick Navigation Guide

### By Topic

#### Menu Screens
- **Main Menu**: MENU_SPECIFICATION.md §2
- **Track Selection**: MENU_SPECIFICATION.md §3
- **Settings**: MENU_SPECIFICATION.md §4
- **Difficulty**: MENU_SPECIFICATION.md §7
- **Lap Count**: MENU_SPECIFICATION.md §8
- **Help**: MENU_SPECIFICATION.md §5
- **Credits**: MENU_SPECIFICATION.md §6
- **Confirmation**: MENU_SPECIFICATION.md §9

#### Technical Details
- **Framework (MTFH)**: MENU_SPECIFICATION.md §1
- **Colors**: MENU_SPECIFICATION.md §11
- **Animations**: MENU_SPECIFICATION.md §10 & MENU_VISUAL_LAYOUT.md
- **Navigation**: MENU_SPECIFICATION.md §13
- **Layouts**: MENU_TECHNICAL_REFERENCE.md + MENU_VISUAL_LAYOUT.md

#### Code Integration
- **Method Signatures**: MENU_TECHNICAL_REFERENCE.md §13
- **Common Patterns**: MENU_TECHNICAL_REFERENCE.md §15
- **State Variables**: MENU_TECHNICAL_REFERENCE.md §9
- **Database Integration**: MENU_TECHNICAL_REFERENCE.md §13

#### Visual Design
- **Screen Layouts**: MENU_VISUAL_LAYOUT.md §1
- **Color Schemes**: MENU_VISUAL_LAYOUT.md §3
- **Text Effects**: MENU_VISUAL_LAYOUT.md §2 & §5
- **Responsive Design**: MENU_VISUAL_LAYOUT.md §19

---

## Key Specifications Summary

### Frameworks & Architecture
- **Base Framework**: MTFH (Menu Texture Framework)
- **Location**: src/game/MTFH/
- **Core Classes**: Menu, MenuItem, MenuItemView, AnimationCurve, MenuManager
- **Base Menu Class**: SurfaceMenu (with textured background)

### All Menu Screens (8 total)
1. **Main Menu** - 5 items (Play, Help, Credits, Settings, Quit)
2. **Track Selection** - Dynamic track items with complex rendering
3. **Difficulty Menu** - 3 levels (Easy, Medium, Hard)
4. **Lap Count Menu** - 7 options (1, 3, 5, 10, 20, 50, 100)
5. **Settings Menu** - 5 categories + 9 submenus
6. **Help Screen** - Static scrolling text
7. **Credits Screen** - 6 rotating sections (120 frame cycle)
8. **Confirmation Menu** - Reusable yes/no dialog

### Submenu System (Additional 9 menus)
- Game Mode (4 options)
- Sounds (On/Off)
- Graphics (5 options)
- FPS (30/60)
- Split Type (Horizontal/Vertical)
- Full Screen Resolution (dynamic)
- Windowed Resolution (dynamic)
- VSync (Qt 5.3+)
- Controls (2 player configuration)
- Reset (3 options)

### Color Scheme
- **Focused**: Yellow RGB(1.0, 1.0, 0.0)
- **Selected**: Red RGB(1.0, 0.0, 0.0)
- **Normal**: White RGB(1.0, 1.0, 1.0)
- **Confirmation**: Cyan RGB(0.25, 0.75, 1.0)
- **Background**: Grey RGB(0.5, 0.5, 0.5) 50% overlay
- **Stars Earned**: Yellow RGB(1.0, 1.0, 0.0)
- **Stars Unearned**: Grey RGB(0.75, 0.75, 0.75)

### Animation System
- **Default Curve**: 15 steps, cubic easing (exponent 3)
- **Text Animation**: Sine wave ±5% amplitude
- **Position Animation**: Smooth interpolation with easing
- **Track Slide Distance**: 1000 pixels
- **Text Pulse**: Size + sin(angle) * 0.05 * baseSize

### Text Sizes
- Main Menu: 40px
- Settings: 20px
- Track Properties: 20px
- Track Title: 30px
- Difficulty: 40px
- Lap Count: 40px
- Dynamic scaling: 20 * (height/640) for Help/Credits

### Shadow Effects
- **Offset**: (2 pixels right, 2 pixels down)
- **Applied to**: All MCTextureText elements
- **Effect**: Drop shadow for readability

### Item Dimensions
- Main: height/8 × full width
- Settings: height/10 × full width
- Track: height/2 × width/2
- Lap Count: height/9 × width/9
- Confirmation: height × width/4

---

## File Structure

### Menu Implementation (12 files)
`
src/game/menu/
├── mainmenu.hpp / .cpp
├── trackselectionmenu.hpp / .cpp
├── settingsmenu.hpp / .cpp
├── difficultymenu.hpp / .cpp
├── lapcountmenu.hpp / .cpp
├── help.hpp / .cpp
├── credits.hpp / .cpp
├── confirmationmenu.hpp / .cpp
├── keyconfigmenu.hpp / .cpp
├── resolutionmenu.hpp / .cpp
├── vsyncmenu.hpp / .cpp
├── surfacemenu.hpp / .cpp
└── textmenuitemview.hpp / .cpp
`

### MTFH Framework (6 files)
`
src/game/MTFH/
├── menu.hpp / .cpp
├── menuitem.hpp / .cpp
├── menuitemview.hpp / .cpp
├── menuitemaction.hpp / .cpp
├── menumanager.hpp / .cpp
├── animationcurve.hpp / .cpp
└── README.md
`

### Audio Framework (STFH - for reference)
`
src/game/STFH/
├── data.hpp / .cpp
├── device.hpp / .cpp
├── listener.hpp / .cpp
├── location.hpp / .cpp
└── source.hpp / .cpp
`

---

## Surface Assets Referenced

### Menu Backgrounds
- "mainMenuBack" - Main menu background
- "settingsBack" - Settings menus background
- "helpBack" - Help menu background
- "creditsBack" - Credits menu background
- "trackSelectionBack" - Track selection & difficulty background

### Track Selection Graphics
- "star" - Full star icon (yellow)
- "starGlow" - Star glow effect
- "starHalf" - Half star (left)
- "starHalfR" - Half star (right)
- "starHalfGlow" - Half star glow
- "lock" - Lock icon for locked tracks

### Shader Programs
- "menu" - Menu shader program (applied to all surfaces and items)

---

## Navigation Structure

### Main Transitions
`
Main Menu
  ├→ Play → Difficulty → Lap Count → Track Selection → [Start Race]
  ├→ Help → [View Help] → Back to Main
  ├→ Credits → [View Credits] → Back to Main
  ├→ Settings → [Configure] → Back to Main
  │   ├→ Game Mode → [Select] → Back to Settings
  │   ├→ Sounds → [On/Off] → Back to Settings
  │   ├→ GFX → [Resolution/FPS/Split] → Back to Settings
  │   ├→ Controls → [Configure Keys] → Back to Settings
  │   └→ Reset → [Confirm] → Back to Settings
  └→ Quit → [Exit Game]
`

### Navigation Input
- **Up/Down**: Vertical menu navigation
- **Left/Right**: Horizontal menu navigation
- **Enter**: Select item
- **Esc/Q**: Return to parent menu
- **Mouse**: Click items or navigation buttons

---

## Data Persistence

### Database Records
- Lap records (milliseconds per track)
- Race records (milliseconds per track/lapcount/difficulty)
- Best positions (1-12 per track/lapcount/difficulty)
- Track unlock status (per track)

### Game Settings
- Difficulty level (Easy/Medium/Hard)
- Lap count (1/3/5/10/20/50/100)
- Game mode (One/Two Player/Time Trial/Duel)
- Sound enabled (boolean)
- FPS setting (30/60)
- Resolution (width × height × fullscreen flag)
- Split type (Horizontal/Vertical)
- VSync (On/Off, Qt 5.3+)

---

## Implementation Notes

### Threading
- Audio processing: AudioWorker (threaded)
- Menu operations: Main thread only
- Frame updates: 30fps or 60fps configurable

### Memory Management
- Menus: Pre-allocated in MenuManager singleton
- No dynamic creation during gameplay
- Smart pointers throughout (shared_ptr)

### Rendering
- OpenGL via MiniCore framework
- Shader program: "menu"
- Texture rendering via MCAssetManager
- Font rendering via MCTextureFont

### Internationalization
- All user text via Qt tr() macro
- Translation strings in separate files
- UTF-8 with BOM support

---

## Credits & Attribution

**Original Author**: Jussi Lind
**Framework**: MTFH (Menu Texture Framework for Hugs)
**Game**: Dust Racing 2D (Open Source - GNU GPLv3)
**Graphics Engine**: MiniCore
**Documentation**: Reverse engineered from source code

---

## Related Documentation

See also:
- Game architecture documentation
- Track loading system documentation
- Audio system (STFH) documentation
- Rendering system documentation
- Physics engine documentation

---

## Document Metadata

**Created**: 2024
**Format**: Markdown (.md)
**Total Documents**: 3
**Total Size**: ~45 KB
**Coverage**: 100% of menu system

**Files Included**:
1. MENU_SPECIFICATION.md - Main specification
2. MENU_TECHNICAL_REFERENCE.md - Technical details
3. MENU_VISUAL_LAYOUT.md - Visual design
4. MENU_DOCUMENTATION_INDEX.md - This file

---

## How to Use This Documentation

### For Developers Extending the Menu System
1. Read MENU_SPECIFICATION.md for architecture
2. Review MENU_TECHNICAL_REFERENCE.md for implementation details
3. Examine code in src/game/menu/ files
4. Follow common patterns shown in MENU_TECHNICAL_REFERENCE.md

### For Visual/UX Design
1. Review MENU_VISUAL_LAYOUT.md for all layouts
2. Check color palette in both files
3. Understand animation curves and transitions
4. Test on multiple resolutions (responsive design section)

### For Game Integration
1. Understand menu hierarchy in MENU_TECHNICAL_REFERENCE.md
2. Review navigation flow in this index
3. Check data persistence requirements
4. Integrate settings/persistence as needed

### For Maintenance/Debugging
1. Use MENU_TECHNICAL_REFERENCE.md for method signatures
2. Check state variables section for member data
3. Review animation system for timing issues
4. Consult asset references for texture problems

---

## End of Documentation Index

For detailed specifications, see individual documents in the same directory.
