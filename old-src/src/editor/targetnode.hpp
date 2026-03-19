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

#ifndef TARGETNODE_HPP
#define TARGETNODE_HPP

#include "../common/targetnodebase.hpp"
#include <QGraphicsItem>

class QGraphicsLineItem;

//! Freely placeable target node.
class TargetNode : public QGraphicsItem, public TargetNodeBase
{
public:
    //! Constructor.
    TargetNode();

    //! Copy constructor.
    TargetNode(const TargetNode & other);

    virtual QRectF boundingRect() const override;

    virtual void paint(QPainter * painter,
                       const QStyleOptionGraphicsItem * option, QWidget * widget = 0) override;

    //! Sets the TargetNode and QGraphicsItem locations.
    virtual void setLocation(QPointF newLocation) override;

    //! Set a line item that is drawn from a node to the next node.
    void setRouteLine(QGraphicsLineItem * routeLine);

    //! Get the route line.
    //! \returns Valid pointer or nullptr if not set.
    QGraphicsLineItem * routeLine() const;

    void updateRouteLine();

private:
    //! Line that is drawn from a node to the next node
    //! according to the route vector.
    QGraphicsLineItem * m_routeLine;
};

using TargetNodePtr = std::shared_ptr<TargetNode>;

#endif // TARGETNODE_HPP
