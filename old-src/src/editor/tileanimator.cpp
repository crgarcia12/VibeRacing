// This file is part of VibeRacing.
// Copyright (C) 2011 Jussi Lind <jussi.lind@iki.fi>
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

#include "tileanimator.hpp"
#include "tracktile.hpp"

#include <QTimeLine>

const int FRAMES = 30;

TileAnimator::TileAnimator(TrackTile * tile)
  : QTimeLine(250)
  , m_tile(tile)
{
    setFrameRange(0, FRAMES);

    connect(this, &TileAnimator::frameChanged, this, &TileAnimator::setTileRotation);
}

bool TileAnimator::rotate90CW()
{
    if (state() == QTimeLine::NotRunning)
    {
        m_a0 = m_tile->rotation();
        m_a1 = m_tile->rotation() + 90;

        start();

        return true;
    }

    return false;
}

bool TileAnimator::rotate90CCW()
{
    if (state() == QTimeLine::NotRunning)
    {
        m_a0 = m_tile->rotation();
        m_a1 = m_tile->rotation() - 90;

        start();

        return true;
    }

    return false;
}

void TileAnimator::setTileRotation(int frame)
{
    m_tile->setRotation(m_a0 + frame * (m_a1 - m_a0) / FRAMES);
}
