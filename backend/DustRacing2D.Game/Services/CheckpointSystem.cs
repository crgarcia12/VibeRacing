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
    private readonly double _finishLineTolerance;

    public CheckpointSystem(TrackData track)
    {
        _checkpoints = track.Checkpoints.OrderBy(c => c.Index).ToList();
        _totalCheckpoints = _checkpoints.Count;
        _finishLineTolerance = track.TileSize > 0 ? track.TileSize / 20.0 : 0.0;
    }

    /// <summary>
    /// Returns (lapCompleted, newCheckpointIndex) after processing the player's current position.
    /// The player's checkpoint index is the most recently cleared checkpoint.
    /// </summary>
    public (bool lapCompleted, int newCheckpointIndex) Process(PlayerState player, long nowMs)
    {
        if (_checkpoints.Count == 0) return (false, player.CheckpointIndex);

        int nextIndex = (player.CheckpointIndex + 1) % _totalCheckpoints;
        var next = _checkpoints[nextIndex];

        double tolerance = next.IsFinishLine ? _finishLineTolerance : 0.0;
        if (!Intersects(player, next, tolerance)) return (false, player.CheckpointIndex);

        bool isFinish = next.IsFinishLine;

        if (isFinish && player.CheckpointIndex == _totalCheckpoints - 1)
        {
            // Completed a full lap
            return (true, 0);
        }

        if (!isFinish)
        {
            return (false, nextIndex);
        }

        return (false, player.CheckpointIndex);
    }

    private static bool Intersects(PlayerState player, CheckpointData cp, double tolerance)
    {
        double left = cp.X - tolerance;
        double top = cp.Y - tolerance;
        double right = cp.X + cp.Width + tolerance;
        double bottom = cp.Y + cp.Height + tolerance;

        // Use the server-side car collision circle so thin gates still register when the car body overlaps them.
        double closestX = Math.Clamp(player.X, left, right);
        double closestY = Math.Clamp(player.Y, top, bottom);
        double dx = player.X - closestX;
        double dy = player.Y - closestY;

        return (dx * dx) + (dy * dy) <= PhysicsEngine.CarRadius * PhysicsEngine.CarRadius;
    }
}
