using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MineOS.Application.Dtos;

namespace MineOS.Infrastructure.Protocols;

public static class MinecraftPingClient
{
    private const int DefaultProtocolVersion = 47;
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(3);

    public static async Task<PingInfoDto?> PingAsync(string host, int port, CancellationToken cancellationToken)
    {
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(DefaultTimeout);

            using var client = new TcpClient();
            await client.ConnectAsync(host, port, timeoutCts.Token);

            await using var stream = client.GetStream();
            await SendHandshakeAsync(stream, host, port, timeoutCts.Token);
            await SendStatusRequestAsync(stream, timeoutCts.Token);

            var packetLength = await ReadVarIntAsync(stream, timeoutCts.Token);
            if (packetLength <= 0)
            {
                return null;
            }

            var packetId = await ReadVarIntAsync(stream, timeoutCts.Token);
            if (packetId != 0x00)
            {
                return null;
            }

            var jsonLength = await ReadVarIntAsync(stream, timeoutCts.Token);
            var json = await ReadStringAsync(stream, jsonLength, timeoutCts.Token);

            return ParsePing(json);
        }
        catch
        {
            return null;
        }
    }

    private static async Task SendHandshakeAsync(Stream stream, string host, int port, CancellationToken cancellationToken)
    {
        using var packet = new MemoryStream();
        WriteVarInt(packet, 0x00);
        WriteVarInt(packet, DefaultProtocolVersion);
        WriteString(packet, host);
        WriteUnsignedShort(packet, port);
        WriteVarInt(packet, 1);

        await WritePacketAsync(stream, packet.ToArray(), cancellationToken);
    }

    private static async Task SendStatusRequestAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var packet = new MemoryStream();
        WriteVarInt(packet, 0x00);
        await WritePacketAsync(stream, packet.ToArray(), cancellationToken);
    }

    private static async Task WritePacketAsync(Stream stream, byte[] payload, CancellationToken cancellationToken)
    {
        using var packet = new MemoryStream();
        WriteVarInt(packet, payload.Length);
        packet.Write(payload, 0, payload.Length);
        var buffer = packet.ToArray();
        await stream.WriteAsync(buffer, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private static void WriteUnsignedShort(Stream stream, int value)
    {
        stream.WriteByte((byte)((value >> 8) & 0xff));
        stream.WriteByte((byte)(value & 0xff));
    }

    private static void WriteString(Stream stream, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteVarInt(stream, bytes.Length);
        stream.Write(bytes, 0, bytes.Length);
    }

    private static void WriteVarInt(Stream stream, int value)
    {
        var valueUnsigned = (uint)value;
        while (true)
        {
            if ((valueUnsigned & ~0x7Fu) == 0)
            {
                stream.WriteByte((byte)valueUnsigned);
                return;
            }

            stream.WriteByte((byte)((valueUnsigned & 0x7Fu) | 0x80u));
            valueUnsigned >>= 7;
        }
    }

    private static async Task<int> ReadVarIntAsync(Stream stream, CancellationToken cancellationToken)
    {
        var numRead = 0;
        var result = 0;
        byte read;

        do
        {
            read = await ReadByteAsync(stream, cancellationToken);
            var value = read & 0x7f;
            result |= value << (7 * numRead);

            numRead++;
            if (numRead > 5)
            {
                throw new InvalidOperationException("VarInt is too big");
            }
        }
        while ((read & 0x80) != 0);

        return result;
    }

    private static async Task<string> ReadStringAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        if (length <= 0)
        {
            return string.Empty;
        }

        var buffer = new byte[length];
        var offset = 0;
        while (offset < length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, length - offset), cancellationToken);
            if (read == 0)
            {
                throw new EndOfStreamException("Stream ended while reading string");
            }

            offset += read;
        }

        return Encoding.UTF8.GetString(buffer);
    }

    private static async Task<byte> ReadByteAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[1];
        var read = await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken);
        if (read == 0)
        {
            throw new EndOfStreamException("Stream ended while reading byte");
        }

        return buffer[0];
    }

    private static PingInfoDto? ParsePing(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var protocol = root.TryGetProperty("version", out var versionElement) &&
                           versionElement.TryGetProperty("protocol", out var protocolElement)
                ? protocolElement.GetInt32()
                : 0;

            var serverVersion = root.TryGetProperty("version", out versionElement) &&
                                versionElement.TryGetProperty("name", out var nameElement)
                ? nameElement.GetString() ?? "Unknown"
                : "Unknown";

            var playersOnline = root.TryGetProperty("players", out var playersElement) &&
                                playersElement.TryGetProperty("online", out var onlineElement)
                ? onlineElement.GetInt32()
                : 0;

            var playersMax = root.TryGetProperty("players", out playersElement) &&
                             playersElement.TryGetProperty("max", out var maxElement)
                ? maxElement.GetInt32()
                : 0;

            var motd = root.TryGetProperty("description", out var descriptionElement)
                ? ExtractMotd(descriptionElement)
                : string.Empty;

            return new PingInfoDto(protocol, serverVersion, motd, playersOnline, playersMax);
        }
        catch
        {
            return null;
        }
    }

    private static string ExtractMotd(JsonElement descriptionElement)
    {
        if (descriptionElement.ValueKind == JsonValueKind.String)
        {
            return descriptionElement.GetString() ?? string.Empty;
        }

        if (descriptionElement.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        if (descriptionElement.TryGetProperty("text", out var textElement))
        {
            builder.Append(textElement.GetString());
        }

        if (descriptionElement.TryGetProperty("extra", out var extraElement) &&
            extraElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var extra in extraElement.EnumerateArray())
            {
                if (extra.ValueKind == JsonValueKind.Object &&
                    extra.TryGetProperty("text", out var extraText))
                {
                    builder.Append(extraText.GetString());
                }
                else if (extra.ValueKind == JsonValueKind.String)
                {
                    builder.Append(extra.GetString());
                }
            }
        }

        return builder.ToString();
    }
}
