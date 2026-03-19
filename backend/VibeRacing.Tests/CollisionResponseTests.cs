using VibeRacing.Game.Models;
using VibeRacing.Game.Services;
using FluentAssertions;

namespace VibeRacing.Tests;

public class CollisionResponseTests
{
    [Fact]
    public void ResolveCollisions_HeadOnCrash_DoesNotFlipHeading()
    {
        var carA = new PlayerState
        {
            X = 100,
            Y = 100,
            Angle = 0,
            Speed = 220
        };
        var carB = new PlayerState
        {
            X = 132,
            Y = 100,
            Angle = Math.PI,
            Speed = 220
        };

        PhysicsEngine.ResolveCollisions([carA, carB]);

        HeadingDot(carA.Angle, 0).Should().BeGreaterThan(0.9);
        HeadingDot(carB.Angle, Math.PI).Should().BeGreaterThan(0.9);
        carA.Speed.Should().BeLessThan(220);
        carB.Speed.Should().BeLessThan(220);
    }

    [Fact]
    public void Step_HeadOnCanvasWallCrash_DoesNotReverseHeading()
    {
        var track = CreateFilledTrack(cols: 5, rows: 4);
        var player = new PlayerState
        {
            X = track.Width - (PhysicsEngine.CarWidth / 2.0) - 2.0,
            Y = track.Height / 2.0,
            Angle = 0,
            Speed = 260
        };

        PhysicsEngine.Step(player, 0.1, track);

        HeadingDot(player.Angle, 0).Should().BeGreaterThan(0.99);
        player.Speed.Should().BeApproximately(0, 1e-6);
    }

    [Fact]
    public void Step_TrackBoundaryCrash_KeepsForwardProgress()
    {
        var track = CreateSingleTileTrack();
        const double originalAngle = 0.25;
        var player = new PlayerState
        {
            X = 210,
            Y = 160,
            Angle = originalAngle,
            Speed = 200
        };

        PhysicsEngine.Step(player, 0.1, track);

        HeadingDot(player.Angle, originalAngle).Should().BeGreaterThan(0);
        player.Speed.Should().BeLessThan(200);
    }

    private static double HeadingDot(double angleA, double angleB)
    {
        return (Math.Cos(angleA) * Math.Cos(angleB)) + (Math.Sin(angleA) * Math.Sin(angleB));
    }

    private static TrackData CreateFilledTrack(int cols, int rows)
    {
        var tiles = new List<TileData>();
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                tiles.Add(new TileData { Col = col, Row = row, Type = "straight", Rotation = 0 });
            }
        }

        return new TrackData
        {
            Name = "Filled",
            Cols = cols,
            Rows = rows,
            TileSize = 96,
            Tiles = tiles
        };
    }

    private static TrackData CreateSingleTileTrack()
    {
        return new TrackData
        {
            Name = "SingleTile",
            Cols = 4,
            Rows = 4,
            TileSize = 96,
            Tiles =
            [
                new TileData { Col = 1, Row = 1, Type = "straight", Rotation = 0 }
            ]
        };
    }
}
