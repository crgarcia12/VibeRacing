using DustRacing2D.Game.Models;

namespace DustRacing2D.Game.Services;

/// <summary>
/// Manages ranking of all players using:
///   raceProgress = lap * totalCheckpoints + checkpointIndex
/// Finished players rank above unfinished, sorted by finish time.
/// </summary>
public static class RankingSystem
{
    public static void UpdateRankings(IEnumerable<PlayerState> players, int totalCheckpoints)
    {
        var sorted = players
            .OrderByDescending(p => p.Finished)
            .ThenBy(p => p.Finished ? p.FinishTimeMs ?? long.MaxValue : long.MaxValue)
            .ThenByDescending(p => p.Lap * totalCheckpoints + p.CheckpointIndex)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
            sorted[i].Rank = i + 1;
    }
}
