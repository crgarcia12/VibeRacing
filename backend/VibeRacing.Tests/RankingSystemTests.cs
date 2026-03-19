using VibeRacing.Game.Models;
using VibeRacing.Game.Services;
using FluentAssertions;

namespace VibeRacing.Tests;

public class RankingSystemTests
{
    [Fact]
    public void RankPlayers_PrioritizesHigherLapCountsBeforeCheckpointProgress()
    {
        var players = new[]
        {
            new PlayerState { PlayerId = "checkpoint-leader", Lap = 2, CheckpointIndex = 8 },
            new PlayerState { PlayerId = "lap-leader", Lap = 3, CheckpointIndex = 0 },
            new PlayerState { PlayerId = "trailing", Lap = 1, CheckpointIndex = 9 }
        };

        var ranked = RankingSystem.RankPlayers(players, totalCheckpoints: 10);

        ranked.Select(player => player.PlayerId)
            .Should()
            .Equal("lap-leader", "checkpoint-leader", "trailing");
        ranked.Select(player => player.Rank)
            .Should()
            .Equal(1, 2, 3);
    }

    [Fact]
    public void RankPlayers_UsesCheckpointProgressWithinTheSameLap()
    {
        var players = new[]
        {
            new PlayerState { PlayerId = "furthest", Lap = 2, CheckpointIndex = 6 },
            new PlayerState { PlayerId = "middle", Lap = 2, CheckpointIndex = 4 },
            new PlayerState { PlayerId = "behind", Lap = 2, CheckpointIndex = 1 }
        };

        var ranked = RankingSystem.RankPlayers(players, totalCheckpoints: 10);

        ranked.Select(player => player.PlayerId)
            .Should()
            .Equal("furthest", "middle", "behind");
    }

    [Fact]
    public void RankPlayers_UsesFinishStateAndFinishTimeToBreakProgressTies()
    {
        var players = new[]
        {
            new PlayerState
            {
                PlayerId = "later-finisher",
                Lap = 4,
                CheckpointIndex = 0,
                Finished = true,
                FinishTimeMs = 72_000
            },
            new PlayerState
            {
                PlayerId = "unfinished",
                Lap = 4,
                CheckpointIndex = 0,
                Finished = false
            },
            new PlayerState
            {
                PlayerId = "earlier-finisher",
                Lap = 4,
                CheckpointIndex = 0,
                Finished = true,
                FinishTimeMs = 68_000
            }
        };

        var ranked = RankingSystem.RankPlayers(players, totalCheckpoints: 10);

        ranked.Select(player => player.PlayerId)
            .Should()
            .Equal("earlier-finisher", "later-finisher", "unfinished");
        ranked.Select(player => player.Rank)
            .Should()
            .Equal(1, 2, 3);
    }
}
