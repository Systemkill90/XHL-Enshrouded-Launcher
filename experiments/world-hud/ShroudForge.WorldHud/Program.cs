using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Loopback by default. Do not make this publicly reachable without an authenticated proxy.
var bind = Environment.GetEnvironmentVariable("SHROUDFORGE_WORLDHUD_BIND")
    ?? "http://127.0.0.1:8765";
builder.WebHost.UseUrls(bind);

var app = builder.Build();
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["Cache-Control"] = "no-store";
    await next();
});
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/health", () => Results.Ok(new
{
    service = "shroudforge-world-hud",
    status = "ready",
    gameTelemetryConnected = false // This endpoint proves HTTP readiness ONLY.
}));

// Read-only file adapter. The server does NOT know Enshrouded time/weather/position.
// A separately validated game telemetry bridge must write a real state snapshot.
app.MapGet("/api/hud", async Task<IResult> (CancellationToken cancellationToken) =>
{
    var path = Environment.GetEnvironmentVariable("SHROUDFORGE_WORLDHUD_STATE")
        ?? Path.Combine(AppContext.BaseDirectory, "state", "current.json");

    if (!File.Exists(path))
        return Results.Ok(new
        {
            status = "unavailable",
            reason = "No game telemetry provider is connected.",
            snapshot = (object?)null
        });

    try
    {
        var length = new FileInfo(path).Length;
        if (length > 65536)
            return Results.Ok(new
            {
                status = "unavailable",
                reason = "Telemetry snapshot exceeds the 64 KiB limit.",
                snapshot = (object?)null
            });

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var snapshot = document.RootElement;
        if (snapshot.ValueKind != JsonValueKind.Object
            || !snapshot.TryGetProperty("updatedAt", out var timestamp)
            || timestamp.ValueKind != JsonValueKind.String
            || !DateTimeOffset.TryParse(timestamp.GetString(), out var updatedAt))
        {
            return Results.Ok(new
            {
                status = "unavailable",
                reason = "Telemetry snapshot lacks a valid updatedAt timestamp.",
                snapshot = (object?)null
            });
        }

        var ageSeconds = (DateTimeOffset.UtcNow - updatedAt).TotalSeconds;
        var fresh = ageSeconds >= -5 && ageSeconds <= 30;
        return Results.Ok(new
        {
            status = fresh ? "fresh" : "stale",
            ageSeconds = Math.Max(0, (int)ageSeconds),
            snapshot = snapshot.Clone()
        });
    }
    catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException)
    {
        return Results.Ok(new
        {
            status = "unavailable",
            reason = "Could not read a valid game telemetry snapshot.",
            snapshot = (object?)null
        });
    }
});

app.Run();
