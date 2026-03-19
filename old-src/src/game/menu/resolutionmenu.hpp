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

#ifndef RESOLUTIONMENU_HPP
#define RESOLUTIONMENU_HPP

#include "confirmationmenu.hpp"
#include "surfacemenu.hpp"

class ConfirmationMenu;

class ResolutionMenu : public SurfaceMenu
{
public:
    //! Constructor.
    ResolutionMenu(ConfirmationMenuPtr confirmationMenu, std::string id, int width, int height, bool fullScreen);

protected:
    //! \reimp
    virtual void enter() override;

private:
    ConfirmationMenuPtr m_confirmationMenu;
};

#endif // RESOLUTIONMENU_HPP
