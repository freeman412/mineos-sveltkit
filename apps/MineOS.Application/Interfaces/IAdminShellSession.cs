using System.Net.WebSockets;

namespace MineOS.Application.Interfaces;

public interface IAdminShellSession
{
    Task RunAsync(WebSocket socket, CancellationToken cancellationToken);
}
