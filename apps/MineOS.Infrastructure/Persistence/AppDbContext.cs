using Microsoft.EntityFrameworkCore;
using MineOS.Domain.Entities;

namespace MineOS.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<User> Users => Set<User>();
    public DbSet<JobRecord> Jobs => Set<JobRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Key).IsUnique();
            entity.Property(x => x.Key).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(128);
            entity.Property(x => x.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(128);
            entity.Property(x => x.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<JobRecord>(entity =>
        {
            entity.ToTable("Jobs");
            entity.HasKey(x => x.JobId);
            entity.Property(x => x.JobId).HasMaxLength(64);
            entity.Property(x => x.Type).HasMaxLength(64);
            entity.Property(x => x.ServerName).HasMaxLength(256);
            entity.Property(x => x.Status).HasMaxLength(32);
        });
    }
}
