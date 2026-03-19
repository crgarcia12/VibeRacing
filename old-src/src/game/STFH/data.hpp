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

#ifndef DATA_HPP
#define DATA_HPP

#include <memory>
#include <string>

namespace STFH {

class Data
{
public:
    //! Constructor.
    Data();

    //! Load the data. Re-implement and call parent. Throw on failure.
    virtual void load(const std::string & path);

    //! Destructor.
    virtual ~Data();

    //! \return path given to load.
    const std::string & path() const;

private:
    std::string m_path;
};

typedef std::shared_ptr<Data> DataPtr;

} // namespace STFH

#endif // DATA_HPP
