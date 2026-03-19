namespace DustRacing2D.Game.Models;

public enum RoomState { Lobby, Countdown, Racing, Finished }

public class Room
{
    public string Code { get; init; } = string.Empty;
    public string TrackName { get; set; } = "dusty-fields";
    public int TotalLaps { get; set; } = 3;
    public RoomState State { get; set; } = RoomState.Lobby;
    public Dictionary<string, PlayerState> Players { get; } = new();
    public long RaceStartTick { get; set; }
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
}
