using System.Text.Json;
using FluentAssertions;
using VibeRacing.Game.Models;
using VibeRacing.Game.Services;

namespace VibeRacing.Tests;

public class RaceSessionTests
{
    private static readonly JsonSerializerOptions CamelCaseJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public async Task RunLoop_EndsTheRaceForEveryoneWhenTheFirstPlayerFinishes()
    {
        var room = new Room
        {
            Code = "ROOM01",
            State = RoomState.Racing,
            TotalLaps = 3
        };

        room.Players["winner"] = new PlayerState
        {
            PlayerId = "winner",
            DisplayName = "Winner",
            X = 16,
            Y = 16,
            Lap = 4,
            CheckpointIndex = 0,
            BestLapMs = 29_500,
            RaceStartMs = 1,
            LapStartMs = 1,
            Finished = true,
            FinishTimeMs = 91_000
        };

        room.Players["chaser"] = new PlayerState
        {
            PlayerId = "chaser",
            DisplayName = "Chaser",
            X = 16,
            Y = 16,
            Lap = 3,
            CheckpointIndex = 2,
            BestLapMs = 31_000,
            RaceStartMs = 1,
            LapStartMs = 1
        };

        var raceFinishedPayload = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var session = new RaceSession(
            room,
            CreateTrack(),
            (_, method, payload) =>
            {
                if (method == "RaceFinished")
                    raceFinishedPayload.TrySetResult(JsonSerializer.SerializeToElement(payload, CamelCaseJson));

                return Task.CompletedTask;
            });

        session.Start();

        var payload = await raceFinishedPayload.Task.WaitAsync(TimeSpan.FromSeconds(2));
        var results = payload.GetProperty("results");

        room.State.Should().Be(RoomState.Finished);
        payload.GetProperty("message").GetString().Should().Be("We are done! Winner finished lap 3 first.");
        results.GetArrayLength().Should().Be(2);
        results[0].GetProperty("playerId").GetString().Should().Be("winner");
        results[0].GetProperty("totalTimeMs").GetInt64().Should().Be(91_000);
        results[1].GetProperty("playerId").GetString().Should().Be("chaser");
        results[1].GetProperty("totalTimeMs").ValueKind.Should().Be(JsonValueKind.Null);
    }

    private static TrackData CreateTrack()
    {
        return new TrackData
        {
            Name = "test-track",
            Cols = 10,
            Rows = 10,
            TileSize = 64,
            Tiles = new List<TileData>
            {
                new() { Col = 0, Row = 0, Type = "road", Rotation = 0 }
            }
        };
    }
}
