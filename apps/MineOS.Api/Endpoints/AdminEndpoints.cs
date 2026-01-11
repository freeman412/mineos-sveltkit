using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MineOS.Api.Middleware;
using MineOS.Application.Interfaces;

namespace MineOS.Api.Endpoints;

public static class AdminEndpoints
{
    public static RouteGroupBuilder MapAdminEndpoints(this RouteGroupBuilder api)
    {
        var admin = api.MapGroup("/admin")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "admin" });

        admin.MapGet("/shell/ws",
                async (HttpContext context, IAdminShellSession shellSession, CancellationToken cancellationToken) =>
                {
                    if (!context.WebSockets.IsWebSocketRequest)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("WebSocket requests only.", cancellationToken);
                        return;
                    }

                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
                    await shellSession.RunAsync(socket, cancellationToken);
                })
            .WithMetadata(new SkipApiKeyAttribute());

        return api;
    }
}
