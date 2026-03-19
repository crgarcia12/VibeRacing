using System.Security.Cryptography;
using DustRacing2D.Game.Models;
using DustRacing2D.Game.Services;

namespace DustRacing2D.Game.Services;

/// <summary>
/// Manages all active rooms: creation, joining, state transitions, and cleanup.
/// </summary>
public class RoomManager
{
    private readonly Dictionary<string, Room> _rooms = new();
    private readonly Dictionary<string, RaceSession> _sessions = new();
    private readonly TrackLoader _trackLoader;
    private readonly object _lock = new();

    public RoomManager(TrackLoader trackLoader)
    {
        _trackLoader = trackLoader;
    }

    public (Room room, bool created) JoinOrCreate(string roomCode, string playerId, string displayName)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomCode, out var room))
            {
                room = new Room { Code = roomCode };
                _rooms[roomCode] = room;
            }

            if (room.State != RoomState.Lobby)
                throw new InvalidOperationException("Race already in progress");

            if (!room.Players.ContainsKey(playerId))
            {
                room.Players[playerId] = new PlayerState
                {
                    PlayerId = playerId,
                    DisplayName = displayName
                };
            }

            room.LastActivityAt = DateTime.UtcNow;
            bool created = room.Players.Count == 1;
            return (room, created);
        }
    }

    public bool SetReady(string roomCode, string playerId)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomCode, out var room)) return false;
            if (!room.Players.TryGetValue(playerId, out var player)) return false;
            player.IsReady = true;
            return room.Players.Values.All(p => p.IsReady) && room.Players.Count >= 1;
        }
    }

    public async Task<Room?> StartRaceAsync(string roomCode, BroadcastDelegate broadcast)
    {
        Room room;
        TrackData track;

        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomCode, out room!)) return null;
            if (room.State != RoomState.Lobby) return null;
            room.State = RoomState.Countdown;
            track = _trackLoader.Load(room.TrackName);

            // Assign start positions
            var slots = track.StartPositions.OrderBy(s => s.Slot).ToList();
            int i = 0;
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var player in room.Players.Values)
            {
                var slot = slots.Count > i ? slots[i] : slots[0];
                player.X = slot.X;
                player.Y = slot.Y;
                player.Angle = slot.Angle;
                player.Speed = 0;
                player.Lap = 1;
                player.CheckpointIndex = 0;
                player.LapStartMs = 0;
                player.RaceStartMs = 0;
                player.Finished = false;
                i++;
            }
        }

        // Countdown: 3 seconds
        for (int c = 3; c >= 1; c--)
        {
            await broadcast(roomCode, "RaceCountdown", new { SecondsRemaining = c });
            await Task.Delay(1000);
        }

        lock (_lock)
        {
            room.State = RoomState.Racing;
            long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            room.RaceStartTick = nowMs;
            foreach (var player in room.Players.Values)
            {
                player.LapStartMs = nowMs;
                player.RaceStartMs = nowMs;
            }
            track = _trackLoader.Load(room.TrackName);
        }

        await broadcast(roomCode, "RaceStarted", new { });

        var session = new RaceSession(room, track, broadcast);
        lock (_lock) _sessions[roomCode] = session;
        session.Start();

        return room;
    }

    public TrackData? GetTrack(string roomCode)
    {
        string? trackName;

        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomCode, out var room))
                return null;

            trackName = room.TrackName;
        }

        return _trackLoader.Load(trackName);
    }

    public void UpdateInput(string roomCode, string playerId, InputState input)
    {
        lock (_lock)
        {
            if (_rooms.TryGetValue(roomCode, out var room) &&
                room.Players.TryGetValue(playerId, out var player))
            {
                player.Input = input;
            }
        }
    }

    public Room? GetRoom(string roomCode)
    {
        lock (_lock)
        {
            _rooms.TryGetValue(roomCode, out var room);
            return room;
        }
    }

    public void RemovePlayer(string roomCode, string playerId)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(roomCode, out var room)) return;
            room.Players.Remove(playerId);
            if (room.Players.Count == 0)
                _rooms.Remove(roomCode);
        }
    }

    public static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
            .ToArray());
    }
}
