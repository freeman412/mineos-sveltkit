using MineOS.Api.Contracts;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

static IResult NotImplementedFeature(string feature) =>
    Results.Problem($"Not implemented: {feature}", statusCode: StatusCodes.Status501NotImplemented);

var api = app.MapGroup("/api/v1");

api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

var auth = api.MapGroup("/auth");
auth.MapPost("/login", () => NotImplementedFeature("auth.login"));
auth.MapPost("/logout", () => NotImplementedFeature("auth.logout"));
auth.MapGet("/me", () => NotImplementedFeature("auth.me"));

var host = api.MapGroup("/host");
host.MapGet("/metrics", () =>
{
    var uptimeSeconds = (long)(Environment.TickCount64 / 1000);
    var metrics = new HostMetrics(
        uptimeSeconds,
        FreeMemBytes: 0,
        LoadAvg: new[] { 0d, 0d, 0d },
        Disk: new DiskMetrics(0, 0, 0));

    return Results.Ok(metrics);
});
host.MapGet("/servers", () => Results.Ok(Array.Empty<ServerSummary>()));
host.MapGet("/profiles", () => Results.Ok(Array.Empty<Profile>()));
host.MapPost("/profiles/{id}/download", () => NotImplementedFeature("host.profiles.download"));
host.MapPost("/profiles/buildtools", () => NotImplementedFeature("host.profiles.buildtools"));
host.MapDelete("/profiles/buildtools/{id}", () => NotImplementedFeature("host.profiles.buildtools.delete"));
host.MapPost("/profiles/{id}/copy-to-server", () => NotImplementedFeature("host.profiles.copy-to-server"));
host.MapGet("/imports", () => Results.Ok(Array.Empty<ArchiveEntry>()));
host.MapPost("/imports/{filename}/create-server", () => NotImplementedFeature("host.imports.create-server"));
host.MapGet("/locales", () => Results.Ok(new[] { "en_US" }));
host.MapGet("/users", () => Results.Ok(Array.Empty<object>()));
host.MapGet("/groups", () => Results.Ok(Array.Empty<object>()));

var servers = api.MapGroup("/servers");
servers.MapPost("/", (CreateServerRequest _) => NotImplementedFeature("servers.create"));
servers.MapDelete("/{name}", (string name, DeleteServerRequest _) => NotImplementedFeature($"servers.delete:{name}"));
servers.MapGet("/{name}/status", (string name) =>
{
    var heartbeat = new ServerHeartbeat(
        Up: false,
        Memory: null,
        Ping: null,
        Query: null,
        Timestamp: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

    return Results.Ok(heartbeat);
});
servers.MapPost("/{name}/actions/{action}", (string name, string action, ActionRequest _) =>
    NotImplementedFeature($"servers.action:{name}:{action}"));
servers.MapPost("/{name}/console", (string name, ConsoleCommand _) =>
    NotImplementedFeature($"servers.console:{name}"));
servers.MapGet("/{name}/server-properties", (string name) =>
    Results.Ok(new Dictionary<string, string>()));
servers.MapPut("/{name}/server-properties", (string name, Dictionary<string, string> _) =>
    NotImplementedFeature($"servers.server-properties.put:{name}"));
servers.MapGet("/{name}/server-config", (string name) =>
    Results.Ok(new Dictionary<string, Dictionary<string, string>>()));
servers.MapPut("/{name}/server-config", (string name, Dictionary<string, Dictionary<string, string>> _) =>
    NotImplementedFeature($"servers.server-config.put:{name}"));
servers.MapGet("/{name}/archives", (string name) => Results.Ok(Array.Empty<ArchiveEntry>()));
servers.MapGet("/{name}/backups", (string name) => Results.Ok(Array.Empty<IncrementEntry>()));
servers.MapGet("/{name}/backups/sizes", (string name) => Results.Ok(Array.Empty<IncrementEntry>()));

var cron = api.MapGroup("/servers/{name}/cron");
cron.MapGet("/", (string name) => Results.Ok(Array.Empty<CronJob>()));
cron.MapPost("/", (string name, CreateCronRequest _) => NotImplementedFeature($"cron.create:{name}"));
cron.MapPatch("/{hash}", (string name, string hash, UpdateCronRequest _) => NotImplementedFeature($"cron.update:{name}:{hash}"));
cron.MapDelete("/{hash}", (string name, string hash) => NotImplementedFeature($"cron.delete:{name}:{hash}"));

var logs = api.MapGroup("/servers/{name}/logs");
logs.MapGet("/", (string name) => Results.Ok(new { paths = Array.Empty<string>() }));
logs.MapGet("/head/{*path}", (string name, string path) => Results.Ok(new { payload = "" }));

app.Run();
