using DustRacing2D.Game.Models;
using DustRacing2D.Game.Services;

namespace DustRacing2D.Game.Services;

public delegate Task BroadcastDelegate(string roomCode, string method, object payload);

/// <summary>
/// Per-room game loop: runs at 60Hz, simulates physics, detects checkpoints/laps,
/// and broadcasts snapshots at 20Hz and scoreboard at 1Hz.
/// </summary>
public class RaceSession : IAsyncDisposable
{
    private const double TickRate = 60.0;
    private const int SnapshotEveryTicks = 3;   // 20Hz broadcast
    private const int ScoreboardEveryTicks = 60; // 1Hz scoreboard

    private readonly Room _room;
    private readonly TrackData _track;
    private readonly CheckpointSystem _checkpoints;
    private readonly BroadcastDelegate _broadcast;
    private readonly CancellationTokenSource _cts = new();

    private long _tick = 0;

    public RaceSession(Room room, TrackData track, BroadcastDelegate broadcast)
    {
        _room = room;
        _track = track;
        _checkpoints = new CheckpointSystem(track);
        _broadcast = broadcast;
    }

    public void Start() => Task.Run(RunLoop);

    private async Task RunLoop()
    {
        var interval = TimeSpan.FromSeconds(1.0 / TickRate);
        var dt = 1.0 / TickRate;
        var token = _cts.Token;

        while (!token.IsCancellationRequested)
        {
            var start = DateTime.UtcNow;

            Tick(dt);

            if (_tick % SnapshotEveryTicks == 0)
                await BroadcastSnapshot();

            if (_tick % ScoreboardEveryTicks == 0)
                await BroadcastScoreboard();

            // Check race end
            var players = _room.Players.Values.ToList();
            if (players.Count > 0 && players.All(p => p.Finished))
            {
                _room.State = RoomState.Finished;
                await BroadcastRaceFinished();
                break;
            }

            var elapsed = DateTime.UtcNow - start;
            var wait = interval - elapsed;
            if (wait > TimeSpan.Zero)
                await Task.Delay(wait, token);
        }
    }

    private void Tick(double dt)
    {
        _tick++;
        long nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        lock (_room.Players)
        {
            foreach (var player in _room.Players.Values)
            {
                if (player.Finished) continue;

                PhysicsEngine.Step(player, dt);

                var (lapCompleted, newCheckpoint) = _checkpoints.Process(player, nowMs);

                if (lapCompleted)
                {
                    long lapTime = nowMs - player.LapStartMs;
                    if (player.BestLapMs == null || lapTime < player.BestLapMs)
                        player.BestLapMs = lapTime;

                    player.LapStartMs = nowMs;
                    player.Lap++;
                    player.CheckpointIndex = 0;

                    if (player.Lap > _room.TotalLaps)
                    {
                        player.Finished = true;
                        player.FinishTimeMs = nowMs - player.RaceStartMs;
                    }
                }
                else if (newCheckpoint != player.CheckpointIndex)
                {
                    player.CheckpointIndex = newCheckpoint;
                }
            }

            RankingSystem.UpdateRankings(_room.Players.Values, _track.Checkpoints.Count);
        }
    }

    private async Task BroadcastSnapshot()
    {
        List<object> playerSnapshots;
        lock (_room.Players)
        {
            playerSnapshots = _room.Players.Values.Select(p => (object)new
            {
                p.PlayerId, p.DisplayName, p.X, p.Y, p.Angle, p.Speed,
                p.Lap, p.CheckpointIndex, p.BestLapMs, p.Finished, p.Rank,
                LapTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - p.LapStartMs
            }).ToList();
        }

        await _broadcast(_room.Code, "GameSnapshot", new
        {
            Tick = _tick,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Players = playerSnapshots
        });
    }

    private async Task BroadcastScoreboard()
    {
        List<object> rankings;
        lock (_room.Players)
        {
            rankings = _room.Players.Values
                .OrderBy(p => p.Rank)
                .Select(p => (object)new
                {
                    p.Rank, p.PlayerId, p.DisplayName, p.Lap, p.BestLapMs, p.Finished
                }).ToList();
        }

        await _broadcast(_room.Code, "ScoreboardUpdate", new { Rankings = rankings });
    }

    private async Task BroadcastRaceFinished()
    {
        List<object> results;
        lock (_room.Players)
        {
            results = _room.Players.Values
                .OrderBy(p => p.Rank)
                .Select(p => (object)new
                {
                    p.Rank, p.PlayerId, p.DisplayName,
                    TotalTimeMs = p.FinishTimeMs,
                    p.BestLapMs
                }).ToList();
        }

        await _broadcast(_room.Code, "RaceFinished", new
        {
            TrackName = _track.Name,
            TotalLaps = _room.TotalLaps,
            Results = results
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _cts.CancelAsync();
        _cts.Dispose();
    }
}
