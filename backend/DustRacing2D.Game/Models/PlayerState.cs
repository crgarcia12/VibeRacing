namespace DustRacing2D.Game.Models;

public class PlayerState
{
    public string PlayerId { get; init; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    // Physics
    public double X { get; set; }
    public double Y { get; set; }
    public double Angle { get; set; }
    public double Speed { get; set; }

    // Race progress
    public int Lap { get; set; } = 0;
    public int CheckpointIndex { get; set; } = 0;
    public long LapStartMs { get; set; } = 0;
    public long? BestLapMs { get; set; } = null;
    public long RaceStartMs { get; set; } = 0;
    public bool Finished { get; set; } = false;
    public long? FinishTimeMs { get; set; } = null;
    public int Rank { get; set; } = 0;

    public InputState Input { get; set; } = new();
    public bool IsReady { get; set; } = false;
}
