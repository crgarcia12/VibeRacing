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

#ifndef TRACKOBJECTFACTORY_HPP
#define TRACKOBJECTFACTORY_HPP

#include <MCObjectFactory>
#include <QString>

class TrackObject;

//! Convenience class used to create various objects when loading a race track.
class TrackObjectFactory
{
public:
    //! Constructor.
    TrackObjectFactory(MCObjectFactory & objectFactory);

    /*! Build and return an MCObject wrapped in TrackObject.
     *  \return nullptr on an unknown or deprecated object. */
    TrackObject * build(QString category, QString role, MCVector2dF location, int angle, bool forceStationary);

private:
    MCObjectFactory & m_objectFactory;
};

#endif // TRACKOBJECTFACTORY_HPP
