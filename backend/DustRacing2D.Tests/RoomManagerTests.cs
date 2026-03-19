using DustRacing2D.Game.Services;
using FluentAssertions;

namespace DustRacing2D.Tests;

public class RoomManagerTests
{
    [Fact]
    public void GetTrack_ReturnsConfiguredRoomTrack()
    {
        var roomManager = new RoomManager(CreateTrackLoader());
        var roomCode = "ROOM01";
        var (room, _) = roomManager.JoinOrCreate(roomCode, "player-1", "Racer 1");
        room.TrackName = "dusty-fields";

        var track = roomManager.GetTrack(roomCode);

        track.Should().NotBeNull();
        track!.Name.Should().Be("Dusty Fields");
        track.Tiles.Should().NotBeEmpty();
        track.StartPositions.Should().NotBeEmpty();
    }

    [Fact]
    public void GetTrack_ReturnsNull_WhenRoomDoesNotExist()
    {
        var roomManager = new RoomManager(CreateTrackLoader());

        var track = roomManager.GetTrack("MISSING");

        track.Should().BeNull();
    }

    private static TrackLoader CreateTrackLoader()
    {
        var tracksDirectory = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "shared", "tracks"));

        return new TrackLoader(tracksDirectory);
    }
}
