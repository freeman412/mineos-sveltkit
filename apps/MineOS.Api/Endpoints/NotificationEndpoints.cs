using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MineOS.Domain.Entities;
using MineOS.Infrastructure.Persistence;

namespace MineOS.Api.Endpoints;

public static class NotificationEndpoints
{
    public static RouteGroupBuilder MapNotificationEndpoints(this RouteGroupBuilder api)
    {
        var notifications = api.MapGroup("/notifications").WithTags("Notifications");

        // List notifications
        notifications.MapGet("/", async (
            [FromQuery] string? serverName,
            [FromQuery] bool? includeRead,
            [FromQuery] bool? includeDismissed,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var query = db.SystemNotifications.AsQueryable();

            // Filter by server name if provided
            if (!string.IsNullOrEmpty(serverName))
            {
                query = query.Where(n => n.ServerName == serverName || n.ServerName == null);
            }

            // Filter by read status
            if (includeRead == false)
            {
                query = query.Where(n => !n.IsRead);
            }

            // Filter by dismissed status
            if (includeDismissed == false)
            {
                query = query.Where(n => n.DismissedAt == null);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(100)
                .ToListAsync(cancellationToken);

            return Results.Ok(notifications);
        });

        // Get notification by ID
        notifications.MapGet("/{id:int}", async (
            int id,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notification = await db.SystemNotifications.FindAsync(new object[] { id }, cancellationToken);
            return notification != null ? Results.Ok(notification) : Results.NotFound();
        });

        // Create notification
        notifications.MapPost("/", async (
            SystemNotification notification,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            notification.CreatedAt = DateTimeOffset.UtcNow;
            db.SystemNotifications.Add(notification);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Created($"/api/notifications/{notification.Id}", notification);
        });

        // Mark as read
        notifications.MapPatch("/{id:int}/read", async (
            int id,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notification = await db.SystemNotifications.FindAsync(new object[] { id }, cancellationToken);
            if (notification == null)
            {
                return Results.NotFound(new { error = "Notification not found" });
            }

            notification.IsRead = true;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(notification);
        });

        // Dismiss notification
        notifications.MapPatch("/{id:int}/dismiss", async (
            int id,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notification = await db.SystemNotifications.FindAsync(new object[] { id }, cancellationToken);
            if (notification == null)
            {
                return Results.NotFound(new { error = "Notification not found" });
            }

            notification.DismissedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(notification);
        });

        // Delete notification
        notifications.MapDelete("/{id:int}", async (
            int id,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notification = await db.SystemNotifications.FindAsync(new object[] { id }, cancellationToken);
            if (notification == null)
            {
                return Results.NotFound(new { error = "Notification not found" });
            }

            db.SystemNotifications.Remove(notification);
            await db.SaveChangesAsync(cancellationToken);
            return Results.NoContent();
        });

        // Bulk delete notifications
        notifications.MapDelete("/", async (
            [FromBody] List<int> ids,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notifications = await db.SystemNotifications
                .Where(n => ids.Contains(n.Id))
                .ToListAsync(cancellationToken);

            db.SystemNotifications.RemoveRange(notifications);
            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { deleted = notifications.Count });
        });

        // Bulk dismiss notifications
        notifications.MapPatch("/dismiss", async (
            [FromBody] List<int> ids,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notifications = await db.SystemNotifications
                .Where(n => ids.Contains(n.Id))
                .ToListAsync(cancellationToken);

            var now = DateTimeOffset.UtcNow;
            foreach (var notification in notifications)
            {
                notification.DismissedAt = now;
            }

            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { dismissed = notifications.Count });
        });

        // Bulk mark as read
        notifications.MapPatch("/read", async (
            [FromBody] List<int> ids,
            AppDbContext db,
            CancellationToken cancellationToken) =>
        {
            var notifications = await db.SystemNotifications
                .Where(n => ids.Contains(n.Id))
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await db.SaveChangesAsync(cancellationToken);
            return Results.Ok(new { marked = notifications.Count });
        });

        return api;
    }
}
