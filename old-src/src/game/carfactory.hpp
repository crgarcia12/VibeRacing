// This file is part of VibeRacing.
// Copyright (C) 2013 Jussi Lind <jussi.lind@iki.fi>
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

#ifndef CARFACTORY_HPP
#define CARFACTORY_HPP

#include "car.hpp"
#include "game.hpp"

#include <memory>

namespace CarFactory {
std::unique_ptr<Car> buildCar(size_t index, size_t carCount, Game & game);
}

#endif // CARFACTORY_HPP
