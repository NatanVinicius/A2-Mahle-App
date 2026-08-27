using A2MahleApp.Domain.Features.Inspection.Entities;
using A2MahleApp.Domain.Features.Production.Entities;

using Microsoft.EntityFrameworkCore;

namespace A2MahleApp.Infrastructure.Persistence;

public sealed class MahleDbContext : DbContext
{
    public MahleDbContext(DbContextOptions<MahleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Production> Productions => Set<Production>();

    public DbSet<Inspection> Inspections => Set<Inspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Production>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Date).IsUnique();
            entity.Property(x => x.Date).IsRequired();
        });

        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Image).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().IsRequired();
            entity.Property(x => x.DateTime).IsRequired();
            entity.Property(x => x.CycleTimeMilliseconds).IsRequired();
            entity.HasIndex(x => x.DateTime);
        });
    }
}
