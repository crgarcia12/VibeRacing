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
                new() { Index = 1, X = 20, Y = 0, Width = 10, Height = 10, IsFinishLine = false },
                new() { Index = 2, X = 40, Y = 0, Width = 10, Height = 10, IsFinishLine = false }
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
        var system = new CheckpointSystem(track);
        var player = new PlayerState { CheckpointIndex = 0 };

        checkpoints.Should().HaveCount(10);
        checkpoints.Count(cp => !cp.IsFinishLine && cp.X == 768).Should().Be(4);
        checkpoints.Count(cp => !cp.IsFinishLine && cp.X == 96).Should().Be(4);

        ProcessAtCheckpoint(system, player, checkpoints[4]).Should().Be((false, 0));

        for (int index = 1; index < checkpoints.Count; index++)
        {
            ProcessAtCheckpoint(system, player, checkpoints[index]).Should().Be((false, index));
        }

        ProcessAtCheckpoint(system, player, checkpoints[0]).Should().Be((true, 0));
    }

    private static (bool lapCompleted, int newCheckpointIndex) ProcessAtCheckpoint(
        CheckpointSystem system,
        PlayerState player,
        CheckpointData checkpoint)
    {
        player.X = checkpoint.X + (checkpoint.Width / 2.0);
        player.Y = checkpoint.Y + (checkpoint.Height / 2.0);

        var result = system.Process(player, 0);
        player.CheckpointIndex = result.newCheckpointIndex;
        return result;
    }

    private static TrackData LoadTrack(string trackName)
    {
        var tracksDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "shared", "tracks"));

        return new TrackLoader(tracksDirectory).Load(trackName);
    }
}
