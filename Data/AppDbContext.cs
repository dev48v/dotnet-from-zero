// ================================================================
// STEP 5 — EF Core DbContext.
// A DbContext is the bridge between C# objects and SQL rows. Each
// `DbSet<T>` is a table; LINQ queries over those sets translate to
// SQL automatically. We use SQLite — a single file on disk — so
// there is no separate database server to install.
// ================================================================
using Microsoft.EntityFrameworkCore;
using SpaceExplorer.Models;

namespace SpaceExplorer.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Favourite> Favourites => Set<Favourite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique index on Date so the user can't bookmark the same
        // picture twice. SQLite enforces this with a UNIQUE
        // constraint on the column.
        modelBuilder.Entity<Favourite>()
            .HasIndex(f => f.Date)
            .IsUnique();
    }
}
