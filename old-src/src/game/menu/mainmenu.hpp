// This file is part of VibeRacing.
// Copyright (C) 2012 Jussi Lind <jussi.lind@iki.fi>
//
// VibeRacing is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// VibeRacing is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with VibeRacing. If not, see <http://www.gnu.org/licenses/>.

#ifndef MAINMENU_HPP
#define MAINMENU_HPP

#include <QObject>

#include "surfacemenu.hpp"

namespace MTFH {
class MenuManager;
}
class Scene;

//! The main menu of the game.
class MainMenu : public QObject, public SurfaceMenu
{
    Q_OBJECT

public:
    static std::string MenuId;

    //! Constructor.
    MainMenu(MTFH::MenuManager & menuManager, Scene & scene, int width, int height);

signals:

    void exitGameRequested();

private:
    void createMenuItems();

    void createSubMenus();

    MTFH::MenuManager & m_menuManager;

    Scene & m_scene;
};

#endif // MAINMENU_HPP
