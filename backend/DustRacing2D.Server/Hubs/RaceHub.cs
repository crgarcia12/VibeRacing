using DustRacing2D.Game.Models;
using DustRacing2D.Game.Services;
using DustRacing2D.Server.Dto;
using Microsoft.AspNetCore.SignalR;

namespace DustRacing2D.Server.Hubs;

/// <summary>
/// SignalR hub ΓÇö entry point for all clientΓåöserver communication.
/// Connection ID is used as PlayerId.
/// </summary>
public class RaceHub : Hub
{
    private readonly RoomManager _rooms;
    private readonly IHubContext<RaceHub> _hubContext;
    // Maps connectionId ΓåÆ roomCode so we can clean up on disconnect
    private static readonly Dictionary<string, string> _connectionRoom = new();
    private static readonly object _mapLock = new();

    public RaceHub(RoomManager rooms, IHubContext<RaceHub> hubContext)
    {
        _rooms = rooms;
        _hubContext = hubContext;
    }

    public async Task JoinLobby(JoinLobbyDto dto)
    {
        string playerId = Context.ConnectionId;
        string roomCode = string.IsNullOrWhiteSpace(dto.RoomCode)
            ? RoomManager.GenerateRoomCode()
            : dto.RoomCode.ToUpperInvariant().Trim();

        string displayName = string.IsNullOrWhiteSpace(dto.DisplayName)
            ? $"Racer{playerId[..4]}"
            : dto.DisplayName.Trim()[..Math.Min(20, dto.DisplayName.Trim().Length)];

        Room room;
        try
        {
            (room, _) = _rooms.JoinOrCreate(roomCode, playerId, displayName);
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("ErrorMessage", new { Message = ex.Message });
            return;
        }

        lock (_mapLock) _connectionRoom[playerId] = roomCode;

        await Groups.AddToGroupAsync(playerId, roomCode);

        // Tell the caller their room/player info
        await Clients.Caller.SendAsync("LobbyJoined", new
        {
            RoomCode = roomCode,
            PlayerId = playerId,
            DisplayName = displayName
        });

        // Tell everyone else
        await Clients.OthersInGroup(roomCode).SendAsync("PlayerJoined", new
        {
            PlayerId = playerId,
            DisplayName = displayName
        });

        // Send full lobby state to the new joiner
        var players = room.Players.Values.Select(p => new
        {
            p.PlayerId, p.DisplayName, p.IsReady
        });
        await Clients.Caller.SendAsync("LobbyState", new { Players = players, RoomCode = roomCode });
    }

    public async Task ReadyUp()
    {
        string playerId = Context.ConnectionId;
        string? roomCode = GetRoomCode(playerId);
        if (roomCode == null) return;

        bool allReady = _rooms.SetReady(roomCode, playerId);
        await Clients.Group(roomCode).SendAsync("PlayerReady", new { PlayerId = playerId });

        if (allReady)
        {
            // Start race (countdown + loop) asynchronously
            _ = _rooms.StartRaceAsync(roomCode, BroadcastToGroup);
        }
    }

    public void SendInput(SendInputDto dto)
    {
        string playerId = Context.ConnectionId;
        string? roomCode = GetRoomCode(playerId);
        if (roomCode == null) return;

        _rooms.UpdateInput(roomCode, playerId, new InputState
        {
            Accelerate = dto.Accelerate,
            Brake = dto.Brake,
            TurnLeft = dto.TurnLeft,
            TurnRight = dto.TurnRight
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string playerId = Context.ConnectionId;
        string? roomCode = GetRoomCode(playerId);

        if (roomCode != null)
        {
            _rooms.RemovePlayer(roomCode, playerId);
            lock (_mapLock) _connectionRoom.Remove(playerId);
            await Groups.RemoveFromGroupAsync(playerId, roomCode);
            await Clients.Group(roomCode).SendAsync("PlayerLeft", new { PlayerId = playerId });
        }

        await base.OnDisconnectedAsync(exception);
    }

    private Task BroadcastToGroup(string roomCode, string method, object payload)
        => _hubContext.Clients.Group(roomCode).SendAsync(method, payload);

    private static string? GetRoomCode(string playerId)
    {
        lock (_mapLock)
        {
            _connectionRoom.TryGetValue(playerId, out var code);
            return code;
        }
    }
}
