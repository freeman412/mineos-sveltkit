using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using MineOS.Application.Interfaces;

namespace MineOS.Api.Endpoints;

public static class HostEndpoints
{
    public static RouteGroupBuilder MapHostEndpoints(this RouteGroupBuilder api)
    {
        var host = api.MapGroup("/host");

        host.MapGet("/metrics", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetMetricsAsync(cancellationToken)));

        host.MapGet("/metrics/stream",
            async (HttpContext context,
                IHostService hostService,
                IOptions<JsonOptions> jsonOptions,
                CancellationToken cancellationToken) =>
            {
                // Configure SSE headers
                context.Response.ContentType = "text/event-stream";
                context.Response.Headers["Cache-Control"] = "no-cache";
                context.Response.Headers["Connection"] = "keep-alive";
                context.Response.Headers["X-Accel-Buffering"] = "no";
                context.Response.Headers.Remove("Content-Length");

                // Start the response immediately to send headers and prevent buffering
                await context.Response.StartAsync(cancellationToken);

                var intervalMs = 2000;
                if (int.TryParse(context.Request.Query["intervalMs"], out var parsed) && parsed > 100)
                {
                    intervalMs = parsed;
                }

                var interval = TimeSpan.FromMilliseconds(intervalMs);
                while (!cancellationToken.IsCancellationRequested)
                {
                    var metrics = await hostService.GetMetricsAsync(cancellationToken);
                    var payload = JsonSerializer.Serialize(metrics, jsonOptions.Value.SerializerOptions);
                    await context.Response.WriteAsync($"data: {payload}\n\n", cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);

                    try
                    {
                        await Task.Delay(interval, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            });

        host.MapGet("/servers", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetServersAsync(cancellationToken)));

        host.MapGet("/profiles", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetProfilesAsync(cancellationToken)));

        host.MapPost("/profiles/{id}/download", () =>
            EndpointHelpers.NotImplementedFeature("host.profiles.download"));

        host.MapPost("/profiles/buildtools", () =>
            EndpointHelpers.NotImplementedFeature("host.profiles.buildtools"));

        host.MapDelete("/profiles/buildtools/{id}", () =>
            EndpointHelpers.NotImplementedFeature("host.profiles.buildtools.delete"));

        host.MapPost("/profiles/{id}/copy-to-server", () =>
            EndpointHelpers.NotImplementedFeature("host.profiles.copy-to-server"));

        host.MapGet("/imports", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetImportsAsync(cancellationToken)));

        host.MapPost("/imports/{filename}/create-server", () =>
            EndpointHelpers.NotImplementedFeature("host.imports.create-server"));

        host.MapGet("/locales", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetLocalesAsync(cancellationToken)));

        host.MapGet("/users", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetUsersAsync(cancellationToken)));

        host.MapGet("/groups", async (IHostService hostService, CancellationToken cancellationToken) =>
            Results.Ok(await hostService.GetGroupsAsync(cancellationToken)));

        return api;
    }
}
