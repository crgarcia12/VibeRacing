using System.Text.Json;
using DustRacing2D.Game.Models;

namespace DustRacing2D.Game.Services;

public class TrackLoader
{
    private readonly string _tracksDirectory;
    private readonly Dictionary<string, TrackData> _cache = new();

    public TrackLoader(string tracksDirectory)
    {
        _tracksDirectory = tracksDirectory;
    }

    public TrackData Load(string trackName)
    {
        if (_cache.TryGetValue(trackName, out var cached)) return cached;

        var path = Path.Combine(_tracksDirectory, $"{trackName}.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Track not found: {path}");

        var json = File.ReadAllText(path);
        var track = JsonSerializer.Deserialize<TrackData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Failed to deserialize track");

        _cache[trackName] = track;
        return track;
    }
}
