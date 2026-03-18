using DustRacing2D.Game.Models;

namespace DustRacing2D.Game.Services;

/// <summary>
/// Manages ranking of all players using authoritative server-side race order:
/// larger lap counts first, then checkpoint progress within the lap,
/// then finish state and finish time as tie-breakers.
/// </summary>
public static class RankingSystem
{
    public static void UpdateRankings(IEnumerable<PlayerState> players, int totalCheckpoints)
    {
        _ = RankPlayers(players, totalCheckpoints);
    }

    public static IReadOnlyList<PlayerState> RankPlayers(IEnumerable<PlayerState> players, int totalCheckpoints)
    {
        ArgumentNullException.ThrowIfNull(players);

        var sorted = players
            .OrderByDescending(p => p.Lap)
            .ThenByDescending(p => GetCheckpointProgressWithinLap(p, totalCheckpoints))
            .ThenByDescending(p => p.Finished)
            .ThenBy(p => p.Finished ? p.FinishTimeMs ?? long.MaxValue : long.MaxValue)
            .ThenBy(p => p.PlayerId, StringComparer.Ordinal)
            .ToList();

        for (int i = 0; i < sorted.Count; i++)
        {
            sorted[i].Rank = i + 1;
        }

        return sorted;
    }

    private static int GetCheckpointProgressWithinLap(PlayerState player, int totalCheckpoints)
    {
        if (totalCheckpoints <= 0)
            return Math.Max(player.CheckpointIndex, 0);

        return Math.Clamp(player.CheckpointIndex, 0, totalCheckpoints - 1);
    }
}
