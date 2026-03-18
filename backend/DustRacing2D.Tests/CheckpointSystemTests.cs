using DustRacing2D.Game.Models;
using DustRacing2D.Game.Services;
using FluentAssertions;

namespace DustRacing2D.Tests;

public class CheckpointSystemTests
{
    [Fact]
    public void Process_RequiresTheNextCheckpointAfterThePreviouslyClearedOne()
    {
        var track = new TrackData
        {
            Checkpoints = new List<CheckpointData>
            {
                new() { Index = 0, X = 0, Y = 0, Width = 10, Height = 10, IsFinishLine = true },
                new() { Index = 1, X = 40, Y = 0, Width = 10, Height = 10, IsFinishLine = false },
                new() { Index = 2, X = 80, Y = 0, Width = 10, Height = 10, IsFinishLine = false }
            }
        };

        var system = new CheckpointSystem(track);
        var player = new PlayerState { CheckpointIndex = 0 };

        ProcessAtCheckpoint(system, player, track.Checkpoints[0]).Should().Be((false, 0));
        ProcessAtCheckpoint(system, player, track.Checkpoints[1]).Should().Be((false, 1));
        ProcessAtCheckpoint(system, player, track.Checkpoints[2]).Should().Be((false, 2));
        ProcessAtCheckpoint(system, player, track.Checkpoints[0]).Should().Be((true, 0));
    }

    [Fact]
    public void DustyFields_RequiresEveryConfiguredGateBeforeCountingTheLap()
    {
        var track = LoadTrack("dusty-fields");
        var checkpoints = track.Checkpoints.OrderBy(cp => cp.Index).ToList();
        var finishLine = checkpoints.Single(cp => cp.IsFinishLine);
        var system = new CheckpointSystem(track);
        var player = new PlayerState { CheckpointIndex = 0 };

        checkpoints.Should().HaveCount(10);
        finishLine.Should().BeEquivalentTo(new
        {
            X = 432d,
            Y = 96d,
            Width = 32d,
            Height = 96d,
            IsFinishLine = true
        });
        finishLine.Height.Should().BeGreaterThan(finishLine.Width);
        checkpoints.Count(cp => !cp.IsFinishLine && cp.X == 800).Should().Be(4);
        checkpoints.Count(cp => !cp.IsFinishLine && cp.X == 128).Should().Be(4);
        checkpoints.Single(cp => cp.Index == 5).Y.Should().Be(608);

        ProcessAtCheckpoint(system, player, checkpoints[4]).Should().Be((false, 0));

        for (int index = 1; index < checkpoints.Count; index++)
        {
            ProcessAtCheckpoint(system, player, checkpoints[index]).Should().Be((false, index));
        }

        ProcessAtCheckpoint(system, player, checkpoints[0]).Should().Be((true, 0));
    }

    [Fact]
    public void DustyFields_CountsLapWhenServerTracksLaneCenterPasses()
    {
        var track = LoadTrack("dusty-fields");
        var checkpoints = track.Checkpoints.OrderBy(cp => cp.Index).ToList();
        var system = new CheckpointSystem(track);
        var player = new PlayerState
        {
            Lap = 1,
            CheckpointIndex = 0
        };

        foreach (var checkpoint in checkpoints.Skip(1))
        {
            var laneCenter = GetDustyFieldsLaneCenter(track, checkpoint);
            ProcessAtPosition(system, player, laneCenter.x, laneCenter.y).Should().Be((false, checkpoint.Index));
        }

        var finishLineCenter = GetDustyFieldsLaneCenter(track, checkpoints[0]);
        ProcessAtPosition(system, player, finishLineCenter.x, finishLineCenter.y).Should().Be((true, 0));
    }

    [Fact]
    public void DustyFields_CountsLapWhenDriverRunsSlightlyOffTheLaneCenter()
    {
        var track = LoadTrack("dusty-fields");
        var checkpoints = track.Checkpoints.OrderBy(cp => cp.Index).ToList();
        var system = new CheckpointSystem(track);
        var player = new PlayerState
        {
            Lap = 1,
            CheckpointIndex = 0
        };

        foreach (var checkpoint in checkpoints.Skip(1))
        {
            var lanePosition = GetDustyFieldsOffsetLanePosition(track, checkpoint, 12.0);

            track.IsOnRoad(lanePosition.x, lanePosition.y).Should().BeTrue();
            ProcessAtPosition(system, player, lanePosition.x, lanePosition.y).Should().Be((false, checkpoint.Index));
        }

        var finishLinePosition = GetDustyFieldsOffsetLanePosition(track, checkpoints[0], 12.0);
        ProcessAtPosition(system, player, finishLinePosition.x, finishLinePosition.y).Should().Be((true, 0));
    }

    [Fact]
    public void DustyFields_CountsLapWhenFinishLineIsCrossedFromTheGrassShoulder()
    {
        var track = LoadTrack("dusty-fields");
        var checkpoints = track.Checkpoints.OrderBy(cp => cp.Index).ToList();
        var finishLine = checkpoints.Single(cp => cp.IsFinishLine);
        var system = new CheckpointSystem(track);
        var player = new PlayerState
        {
            Lap = 1,
            CheckpointIndex = 0
        };

        foreach (var checkpoint in checkpoints.Skip(1))
        {
            var laneCenter = GetDustyFieldsLaneCenter(track, checkpoint);
            ProcessAtPosition(system, player, laneCenter.x, laneCenter.y).Should().Be((false, checkpoint.Index));
        }

        double playableMargin = PhysicsEngine.GetPlayableMargin(track);
        double finishLineCenterX = finishLine.X + (finishLine.Width / 2.0);
        double grassShoulderY = finishLine.Y + finishLine.Height + playableMargin - 1.0;

        track.IsInsidePlayableArea(finishLineCenterX, grassShoulderY, playableMargin).Should().BeTrue();
        track.IsOnRoad(finishLineCenterX, grassShoulderY).Should().BeFalse();

        ProcessAtPosition(system, player, finishLineCenterX, grassShoulderY).Should().Be((true, 0));
    }

    private static (bool lapCompleted, int newCheckpointIndex) ProcessAtCheckpoint(
        CheckpointSystem system,
        PlayerState player,
        CheckpointData checkpoint)
    {
        return ProcessAtPosition(
            system,
            player,
            checkpoint.X + (checkpoint.Width / 2.0),
            checkpoint.Y + (checkpoint.Height / 2.0));
    }

    private static (bool lapCompleted, int newCheckpointIndex) ProcessAtPosition(
        CheckpointSystem system,
        PlayerState player,
        double x,
        double y)
    {
        player.X = x;
        player.Y = y;

        var result = system.Process(player, 0);
        player.CheckpointIndex = result.newCheckpointIndex;
        return result;
    }

    private static (double x, double y) GetDustyFieldsLaneCenter(TrackData track, CheckpointData checkpoint)
    {
        double checkpointCenterX = checkpoint.X + (checkpoint.Width / 2.0);
        double checkpointCenterY = checkpoint.Y + (checkpoint.Height / 2.0);

        if (checkpoint.IsFinishLine)
            return (checkpointCenterX, checkpointCenterY);

        if (checkpoint.Height > checkpoint.Width)
        {
            int col = (int)(checkpointCenterX / track.TileSize);
            return ((col * track.TileSize) + (track.TileSize / 2.0), checkpointCenterY);
        }

        int row = (int)(checkpointCenterY / track.TileSize);
        return (checkpointCenterX, (row * track.TileSize) + (track.TileSize / 2.0));
    }

    private static (double x, double y) GetDustyFieldsOffsetLanePosition(
        TrackData track,
        CheckpointData checkpoint,
        double crossTrackOffset)
    {
        var (x, y) = GetDustyFieldsLaneCenter(track, checkpoint);

        if (checkpoint.IsFinishLine)
            return (x, y);

        if (checkpoint.Height > checkpoint.Width)
            return (x + crossTrackOffset, y);

        return (x, y + crossTrackOffset);
    }

    private static TrackData LoadTrack(string trackName)
    {
        var tracksDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "shared", "tracks"));

        return new TrackLoader(tracksDirectory).Load(trackName);
    }
}
