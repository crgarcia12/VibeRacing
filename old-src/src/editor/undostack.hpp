// This file is part of VibeRacing.
// Copyright (C) 2017 Jussi Lind <jussi.lind@iki.fi>
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

#ifndef UNDOSTACK_HPP
#define UNDOSTACK_HPP

#include "trackdata.hpp"

#include <list>

class UndoStack
{
public:
    UndoStack(unsigned int maxHistorySize = 100);

    void pushUndoPoint(TrackDataPtr trackData);

    void pushRedoPoint(TrackDataPtr trackData);

    void clear();

    bool isUndoable() const;

    TrackDataPtr undo();

    bool isRedoable() const;

    TrackDataPtr redo();

private:
    using TrackDataVector = std::list<TrackDataPtr>;

    TrackDataVector m_undoStack;

    TrackDataVector m_redoStack;

    unsigned int m_maxHistorySize;
};

#endif // UNDOSTACK_HPP
