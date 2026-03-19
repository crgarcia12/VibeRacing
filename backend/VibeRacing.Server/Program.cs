using VibeRacing.Game.Services;
using VibeRacing.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var tracksDir = ResolveTracksDirectory(builder.Configuration);

builder.Services.AddSingleton(new TrackLoader(tracksDir));
builder.Services.AddSingleton<RoomManager>();
builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    // The frontend sends camelCase JSON (e.g. { roomCode, displayName }).
    // Without this, System.Text.Json's case-sensitive default fails to bind
    // positional record constructor parameters (RoomCode, DisplayName), leaving
    // them null — so dto.RoomCode is always null and a new room is always created
    // instead of joining the one the user specified.
    options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
    options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<RaceHub>("/racehub");
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();

static string ResolveTracksDirectory(IConfiguration configuration)
{
    string? configuredTracksDirectory = configuration["TRACKS_DIR"];
    if (!string.IsNullOrWhiteSpace(configuredTracksDirectory))
        return Path.GetFullPath(configuredTracksDirectory);

    // Resolve shared tracks directory (../../shared/tracks relative to this project)
    return Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "shared", "tracks"));
}
