using DustRacing2D.Game.Services;
using DustRacing2D.Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

var tracksDir = ResolveTracksDirectory(builder.Configuration);
var allowedOrigins = ResolveAllowedOrigins(builder.Configuration);

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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

app.UseCors();
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

static string[] ResolveAllowedOrigins(IConfiguration configuration)
{
    var origins = new[]
    {
        "http://localhost:5173",
        "http://localhost:5174",
        "http://localhost:3000",
        "http://127.0.0.1:5173",
        "http://127.0.0.1:5174"
    }.ToList();

    string? configuredOrigins = configuration["CORS_ALLOWED_ORIGINS"];
    if (!string.IsNullOrWhiteSpace(configuredOrigins))
    {
        origins.AddRange(configuredOrigins
            .Split(new[] { ',', ';', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    return origins
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
