using EventFlowX.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace EventFlowX.Consumer.Data;

public class InboxDbContext : DbContext
{
    public InboxDbContext(DbContextOptions<InboxDbContext> options)
        : base(options)
    {
    }

    public DbSet<InboxEvent> InboxEvents{ get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<InboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Id).IsUnique();  // idempotency garantisi
            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });

        base.OnModelCreating(modelBuilder);
    }
}
