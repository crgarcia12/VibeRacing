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

#include <memory>

#include "data.hpp"
#include "location.hpp"

namespace STFH {

class Source
{
public:
    //! Constructor.
    Source();

    //! Destructor.
    virtual ~Source();

    //! Set the sound data. Throw on failure.
    virtual void setData(DataPtr data);

    //! Play the sound.
    virtual void play(bool loop = false) = 0;

    //! Stop the sound.
    virtual void stop() = 0;

    /*! Set volume.
     *  \param volume 0.0..1.0 */
    virtual void setVolume(float volume);

    //! \return volume.
    virtual float volume() const;

    /*! Set pitch.
     *  \param pitch 0.0..1.0 */
    virtual void setPitch(float pitch);

    //! \return pitch.
    virtual float pitch() const;

    //! Set location.
    virtual void setLocation(const Location & location);

    //! \return location.
    virtual const Location & location() const;

    //! Set maximum distance.
    virtual void setMaxDist(float maxDist);

    //! Set reference distance.
    virtual void setReferenceDist(float refDist);

private:
    Location m_location;
    float m_pitch;
    float m_volume;
    float m_maxDist;
    float m_refDist;
    DataPtr m_data;
};

typedef std::shared_ptr<Source> SourcePtr;

} // namespace STFH
