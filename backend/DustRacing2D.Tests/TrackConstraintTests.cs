using DustRacing2D.Game.Models;
using DustRacing2D.Game.Services;
using FluentAssertions;

namespace DustRacing2D.Tests;

public class TrackConstraintTests
{
    [Fact]
    public void IsOnRoad_UsesTileOccupancy()
    {
        var track = CreateLoopTrack();

        track.IsOnRoad(240, 144).Should().BeTrue();
        track.IsOnRoad(480, 336).Should().BeFalse();
        track.IsOnRoad(-10, 100).Should().BeFalse();
    }

    [Fact]
    public void Step_ReducesAccelerationWhenOffRoad()
    {
        var track = CreateSingleTileTrack();
        var onRoadPlayer = new PlayerState
        {
            X = 144,
            Y = 144,
            Input = new InputState { Accelerate = true }
        };
        var offRoadPlayer = new PlayerState
        {
            X = 80,
            Y = 144,
            Input = new InputState { Accelerate = true }
        };

        PhysicsEngine.Step(onRoadPlayer, 0.25, track);
        PhysicsEngine.Step(offRoadPlayer, 0.25, track);

        onRoadPlayer.Speed.Should().BeGreaterThan(offRoadPlayer.Speed);
        offRoadPlayer.Speed.Should().BeLessThan(onRoadPlayer.Speed);
    }

    [Fact]
    public void Step_ClampsCarsBackTowardTrackPerimeter()
    {
        var track = CreateSingleTileTrack();
        var player = new PlayerState
        {
            X = 10,
            Y = 10,
            Angle = Math.PI,
            Speed = 250
        };

        PhysicsEngine.Step(player, 0.1, track);

        var margin = PhysicsEngine.GetPlayableMargin(track);
        track.IsInsidePlayableArea(player.X, player.Y, margin).Should().BeTrue();
        player.Speed.Should().BeLessThan(250);
    }

    private static TrackData CreateSingleTileTrack()
    {
        return new TrackData
        {
            Name = "Test",
            Cols = 4,
            Rows = 4,
            TileSize = 96,
            Tiles = new List<TileData>
            {
                new() { Col = 1, Row = 1, Type = "straight", Rotation = 0 }
            }
        };
    }

    private static TrackData CreateLoopTrack()
    {
        return new TrackData
        {
            Name = "Loop",
            Cols = 10,
            Rows = 8,
            TileSize = 96,
            Tiles = new List<TileData>
            {
                new() { Col = 1, Row = 1, Type = "curve", Rotation = 180 },
                new() { Col = 2, Row = 1, Type = "straight", Rotation = 0 },
                new() { Col = 3, Row = 1, Type = "straight", Rotation = 0 },
                new() { Col = 4, Row = 1, Type = "straight", Rotation = 0 },
                new() { Col = 5, Row = 1, Type = "straight", Rotation = 0 },
                new() { Col = 6, Row = 1, Type = "straight", Rotation = 0 },
                new() { Col = 7, Row = 1, Type = "straight", Rotation = 0 },
                new() { Col = 8, Row = 1, Type = "curve", Rotation = 270 },
                new() { Col = 8, Row = 2, Type = "straight", Rotation = 90 },
                new() { Col = 8, Row = 3, Type = "straight", Rotation = 90 },
                new() { Col = 8, Row = 4, Type = "straight", Rotation = 90 },
                new() { Col = 8, Row = 5, Type = "straight", Rotation = 90 },
                new() { Col = 8, Row = 6, Type = "curve", Rotation = 0 },
                new() { Col = 7, Row = 6, Type = "straight", Rotation = 0 },
                new() { Col = 6, Row = 6, Type = "straight", Rotation = 0 },
                new() { Col = 5, Row = 6, Type = "straight", Rotation = 0 },
                new() { Col = 4, Row = 6, Type = "straight", Rotation = 0 },
                new() { Col = 3, Row = 6, Type = "straight", Rotation = 0 },
                new() { Col = 2, Row = 6, Type = "straight", Rotation = 0 },
                new() { Col = 1, Row = 6, Type = "curve", Rotation = 90 },
                new() { Col = 1, Row = 5, Type = "straight", Rotation = 90 },
                new() { Col = 1, Row = 4, Type = "straight", Rotation = 90 },
                new() { Col = 1, Row = 3, Type = "straight", Rotation = 90 },
                new() { Col = 1, Row = 2, Type = "straight", Rotation = 90 }
            }
        };
    }
}
