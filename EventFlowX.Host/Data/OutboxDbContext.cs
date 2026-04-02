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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status)
                  .HasConversion<string>();   // DB'de "Pending" gibi okunabilir saklansın
            entity.HasIndex(e => e.Status);
        });
        base.OnModelCreating(modelBuilder);
    }
}
