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

        checkpoints.Should().HaveCount(4);
        finishLine.Should().BeEquivalentTo(new
        {
            X = 432d,
            Y = 96d,
            Width = 32d,
            Height = 96d,
            IsFinishLine = true
        });
        finishLine.Height.Should().BeGreaterThan(finishLine.Width);
        checkpoints.Single(cp => cp.Index == 1).Should().BeEquivalentTo(new
        {
            X = 768d,
            Y = 416d,
            Width = 96d,
            Height = 32d,
            IsFinishLine = false
        });
        checkpoints.Single(cp => cp.Index == 2).Should().BeEquivalentTo(new
        {
            X = 512d,
            Y = 576d,
            Width = 32d,
            Height = 96d,
            IsFinishLine = false
        });
        checkpoints.Single(cp => cp.Index == 3).Should().BeEquivalentTo(new
        {
            X = 96d,
            Y = 320d,
            Width = 96d,
            Height = 32d,
            IsFinishLine = false
        });

        ProcessAtCheckpoint(system, player, checkpoints[2]).Should().Be((false, 0));

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
            var laneCenter = GetDustyFieldsLaneCenter(checkpoint);
            ProcessAtPosition(system, player, laneCenter.x, laneCenter.y).Should().Be((false, checkpoint.Index));
        }

        var finishLineCenter = GetDustyFieldsLaneCenter(checkpoints[0]);
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
            var lanePosition = GetDustyFieldsOffsetLanePosition(checkpoint, 12.0);

            track.IsOnRoad(lanePosition.x, lanePosition.y).Should().BeTrue();
            ProcessAtPosition(system, player, lanePosition.x, lanePosition.y).Should().Be((false, checkpoint.Index));
        }

        var finishLinePosition = GetDustyFieldsOffsetLanePosition(checkpoints[0], 12.0);
        ProcessAtPosition(system, player, finishLinePosition.x, finishLinePosition.y).Should().Be((true, 0));
    }

    [Fact]
    public void DustyFields_CountsCheckpointsWhenNonFinishGatesAreCrossedFromTheGrassShoulder()
    {
        var track = LoadTrack("dusty-fields");
        var checkpoints = track.Checkpoints.OrderBy(cp => cp.Index).ToList();
        var system = new CheckpointSystem(track);
        var player = new PlayerState
        {
            Lap = 1,
            CheckpointIndex = 0
        };

        double playableMargin = PhysicsEngine.GetPlayableMargin(track);

        foreach (var checkpoint in checkpoints.Skip(1))
        {
            var grassShoulderPosition = GetDustyFieldsGrassShoulderPosition(checkpoint, playableMargin);

            track.IsInsidePlayableArea(grassShoulderPosition.x, grassShoulderPosition.y, playableMargin).Should().BeTrue();
            track.IsOnRoad(grassShoulderPosition.x, grassShoulderPosition.y).Should().BeFalse();
            ProcessAtPosition(system, player, grassShoulderPosition.x, grassShoulderPosition.y).Should().Be((false, checkpoint.Index));
        }
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
            var laneCenter = GetDustyFieldsLaneCenter(checkpoint);
            ProcessAtPosition(system, player, laneCenter.x, laneCenter.y).Should().Be((false, checkpoint.Index));
        }

        double playableMargin = PhysicsEngine.GetPlayableMargin(track);
        double finishLineCenterX = finishLine.X + (finishLine.Width / 2.0);
        double grassShoulderY = finishLine.Y + finishLine.Height + playableMargin - 1.0;

        track.IsInsidePlayableArea(finishLineCenterX, grassShoulderY, playableMargin).Should().BeTrue();
        track.IsOnRoad(finishLineCenterX, grassShoulderY).Should().BeFalse();

        ProcessAtPosition(system, player, finishLineCenterX, grassShoulderY).Should().Be((true, 0));
    }

    [Fact]
    public void GetNextCheckpointIndex_ReturnsTheConfiguredCheckpointIdentifier()
    {
        var track = new TrackData
        {
            Checkpoints = new List<CheckpointData>
            {
                new() { Index = 10, X = 0, Y = 0, Width = 10, Height = 10, IsFinishLine = true },
                new() { Index = 30, X = 40, Y = 0, Width = 10, Height = 10, IsFinishLine = false },
                new() { Index = 20, X = 80, Y = 0, Width = 10, Height = 10, IsFinishLine = false }
            }
        };

        var system = new CheckpointSystem(track);

        system.GetNextCheckpointIndex(new PlayerState { CheckpointIndex = 0 }).Should().Be(20);
        system.GetNextCheckpointIndex(new PlayerState { CheckpointIndex = 1 }).Should().Be(30);
        system.GetNextCheckpointIndex(new PlayerState { CheckpointIndex = 2 }).Should().Be(10);
        system.GetNextCheckpointIndex(new PlayerState { CheckpointIndex = 2, Finished = true }).Should().BeNull();
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

    private static (double x, double y) GetDustyFieldsLaneCenter(CheckpointData checkpoint)
    {
        return (
            checkpoint.X + (checkpoint.Width / 2.0),
            checkpoint.Y + (checkpoint.Height / 2.0));
    }

    private static (double x, double y) GetDustyFieldsOffsetLanePosition(
        CheckpointData checkpoint,
        double crossTrackOffset)
    {
        var (x, y) = GetDustyFieldsLaneCenter(checkpoint);

        if (checkpoint.Height > checkpoint.Width)
            return (x, y + crossTrackOffset);

        return (x + crossTrackOffset, y);
    }

    private static (double x, double y) GetDustyFieldsGrassShoulderPosition(
        CheckpointData checkpoint,
        double playableMargin)
    {
        var (x, y) = GetDustyFieldsLaneCenter(checkpoint);
        double shoulderOffset = playableMargin - 1.0;

        if (checkpoint.Height > checkpoint.Width)
            return (x, checkpoint.Y + checkpoint.Height + shoulderOffset);

        return (checkpoint.X + checkpoint.Width + shoulderOffset, y);
    }

    private static TrackData LoadTrack(string trackName)
    {
        var tracksDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "shared", "tracks"));

        return new TrackLoader(tracksDirectory).Load(trackName);
    }
}
