using DustRacing2D.Game.Models;

namespace DustRacing2D.Game.Services;

/// <summary>
/// Detects checkpoint crossings and lap completions.
/// Checkpoints must be crossed in order; lap completes when finish line crossed after all others.
/// </summary>
public class CheckpointSystem
{
    private readonly List<CheckpointData> _checkpoints;
    private readonly int _totalCheckpoints;

    public CheckpointSystem(TrackData track)
    {
        _checkpoints = track.Checkpoints.OrderBy(c => c.Index).ToList();
        _totalCheckpoints = _checkpoints.Count;
    }

    /// <summary>
    /// Returns (lapCompleted, newCheckpointIndex) after processing the player's current position.
    /// </summary>
    public (bool lapCompleted, int newCheckpointIndex) Process(PlayerState player, long nowMs)
    {
        if (_checkpoints.Count == 0) return (false, player.CheckpointIndex);

        int nextIndex = (player.CheckpointIndex) % _totalCheckpoints;
        var next = _checkpoints[nextIndex];

        if (!Intersects(player, next)) return (false, player.CheckpointIndex);

        bool isFinish = next.IsFinishLine;
        int newIndex = (nextIndex + 1) % _totalCheckpoints;

        if (isFinish && player.CheckpointIndex == _totalCheckpoints - 1)
        {
            // Completed a full lap
            return (true, 0);
        }

        if (!isFinish)
        {
            return (false, newIndex);
        }

        return (false, player.CheckpointIndex);
    }

    private static bool Intersects(PlayerState player, CheckpointData cp)
    {
        double px = player.X, py = player.Y;
        return px >= cp.X && px <= cp.X + cp.Width
            && py >= cp.Y && py <= cp.Y + cp.Height;
    }
}
