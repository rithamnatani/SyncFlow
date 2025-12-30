using Api.Shared.Domain;
using Microsoft.EntityFrameworkCore;

namespace Api.Shared.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<JoinRequest> JoinRequests => Set<JoinRequest>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project Configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.InviteCode).HasMaxLength(8);
            entity.HasIndex(p => p.OwnerId);
            entity.HasIndex(p => p.InviteCode).IsUnique();
        });

        // ProjectMember Configuration (Composite PK)
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(pm => new { pm.ProjectId, pm.UserId });
            entity.Property(pm => pm.Role).HasMaxLength(20);

            entity.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // JoinRequest Configuration
        modelBuilder.Entity<JoinRequest>(entity =>
        {
            entity.HasKey(jr => jr.Id);
            entity.Property(jr => jr.Status).HasMaxLength(20);
            entity.HasIndex(jr => new { jr.ProjectId, jr.UserId });

            entity.HasOne(jr => jr.Project)
                .WithMany(p => p.JoinRequests)
                .HasForeignKey(jr => jr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Ticket Configuration
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(4000);
            entity.Property(t => t.Type).HasMaxLength(50);
            entity.Property(t => t.Color).HasMaxLength(20);

            // Indexes for common queries
            entity.HasIndex(t => t.ProjectId);
            entity.HasIndex(t => t.Status);
            entity.HasIndex(t => new { t.ProjectId, t.TicketNumber }).IsUnique();

            // Enum conversions (stored as strings for readability)
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Priority).HasConversion<string>();

            // Tags as JSONB
            entity.Property(t => t.Tags).HasColumnType("jsonb");

            entity.HasOne(t => t.Project)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
