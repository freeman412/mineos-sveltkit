using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MineOS.Application.Dtos;
using MineOS.Application.Interfaces;

namespace MineOS.Api.Endpoints;

public static class FileEndpoints
{
    public static RouteGroupBuilder MapFileEndpoints(this RouteGroupBuilder servers)
    {
        servers.MapGet("/{name}/files", async (
            string name,
            IFileService fileService,
            CancellationToken cancellationToken) =>
        {
            return await HandleFileBrowseAsync(name, "/", fileService, cancellationToken);
        });

        servers.MapGet("/{name}/files/{*path}", async (
            string name,
            string path,
            IFileService fileService,
            CancellationToken cancellationToken) =>
        {
            var normalizedPath = NormalizePath(path);
            return await HandleFileBrowseAsync(name, normalizedPath, fileService, cancellationToken);
        });

        // Upload file content
        servers.MapPost("/{name}/files/{*path}", async (
            string name,
            string path,
            HttpRequest request,
            IFileService fileService,
            CancellationToken cancellationToken) =>
        {
            var normalizedPath = NormalizePath(path);

            try
            {
                if (request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var payload = await JsonSerializer.DeserializeAsync<FileUploadRequest>(
                        request.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                        cancellationToken);

                    if (payload == null || (string.IsNullOrWhiteSpace(payload.Content) && string.IsNullOrWhiteSpace(payload.ContentBase64)))
                    {
                        return Results.BadRequest(new { error = "File content is required" });
                    }

                    if (!string.IsNullOrWhiteSpace(payload.ContentBase64))
                    {
                        var bytes = Convert.FromBase64String(payload.ContentBase64);
                        await fileService.WriteFileBytesAsync(name, normalizedPath, bytes, cancellationToken);
                    }
                    else
                    {
                        await fileService.WriteFileAsync(name, normalizedPath, payload.Content ?? string.Empty, cancellationToken);
                    }
                }
                else
                {
                    using var buffer = new MemoryStream();
                    await request.Body.CopyToAsync(buffer, cancellationToken);
                    await fileService.WriteFileBytesAsync(name, normalizedPath, buffer.ToArray(), cancellationToken);
                }

                return Results.Ok(new { message = $"File '{normalizedPath}' uploaded" });
            }
            catch (FormatException ex)
            {
                return Results.BadRequest(new { error = $"Invalid base64 content: {ex.Message}" });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Update file content (text)
        servers.MapPut("/{name}/files/{*path}", async (
            string name,
            string path,
            HttpRequest request,
            IFileService fileService,
            CancellationToken cancellationToken) =>
        {
            var normalizedPath = NormalizePath(path);

            try
            {
                string? content = null;

                if (request.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var payload = await JsonSerializer.DeserializeAsync<FileUpdateRequest>(
                        request.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                        cancellationToken);
                    content = payload?.Content;
                }
                else
                {
                    using var reader = new StreamReader(request.Body);
                    content = await reader.ReadToEndAsync(cancellationToken);
                }

                if (content == null)
                {
                    return Results.BadRequest(new { error = "File content is required" });
                }

                await fileService.WriteFileAsync(name, normalizedPath, content, cancellationToken);
                return Results.Ok(new { message = $"File '{normalizedPath}' updated" });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        // Delete file or directory
        servers.MapDelete("/{name}/files/{*path}", async (
            string name,
            string path,
            IFileService fileService,
            CancellationToken cancellationToken) =>
        {
            var normalizedPath = NormalizePath(path);
            try
            {
                await fileService.DeleteFileAsync(name, normalizedPath, cancellationToken);
                return Results.Ok(new { message = $"Deleted '{normalizedPath}'" });
            }
            catch (FileNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        return servers;
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var trimmed = path.Trim();
        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }

    private static async Task<IResult> HandleFileBrowseAsync(
        string name,
        string path,
        IFileService fileService,
        CancellationToken cancellationToken)
    {
        try
        {
            var files = await fileService.ListFilesAsync(name, path, cancellationToken);
            return Results.Ok(new FileBrowseResultDto(path, "directory", files, null));
        }
        catch (DirectoryNotFoundException)
        {
            try
            {
                var file = await fileService.ReadFileAsync(name, path, cancellationToken);
                return Results.Ok(new FileBrowseResultDto(path, "file", null, file));
            }
            catch (FileNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}

public record FileUploadRequest(string? Content, string? ContentBase64);
public record FileUpdateRequest(string Content);
