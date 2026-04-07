using EventFlowX.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFlowX.Host.Data;

public class OutboxDbContext : DbContext
{
    public OutboxDbContext(DbContextOptions<OutboxDbContext> options)
        : base(options)
    {

    }

    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    public DbSet<Pod> Pods { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ProcessingBy).HasMaxLength(100); 
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => e.Status);
            entity.OwnsOne(e => e.Data, data =>
            {
                data.Property(d => d.EventId).HasColumnName("EventId");
                data.Property(d => d.EventType).HasColumnName("EventType");
                data.Property(d => d.Payload).HasColumnName("Payload");
                data.Property(d => d.OccurredAt).HasColumnName("OccurredAt");
            });            
        });
    
        modelBuilder.Entity<Pod>(entity =>
        {
            entity.HasKey(e => e.InstanceId);
            entity.Property(e => e.InstanceId).HasMaxLength(100); 
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => e.Status);

            entity.HasMany<OutboxEvent>()
                  .WithOne(e => e.Pod)
                  .HasForeignKey(e => e.ProcessingBy)
                  .HasPrincipalKey(p => p.InstanceId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

}
