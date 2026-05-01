using Microsoft.EntityFrameworkCore;
using RecipeNest.API.Models;

namespace RecipeNest.API.Data;

/// <summary>
/// EF Core database context – the bridge between C# models and SQL Server tables.
/// Registers all three entities and configures their relationships.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // These properties map to SQL Server tables
    public DbSet<User>        Users        { get; set; }
    public DbSet<ChefProfile> ChefProfiles { get; set; }
    public DbSet<Recipe>      Recipes      { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── User → ChefProfile : one-to-one ──────────────────────────────────
        modelBuilder.Entity<User>()
            .HasOne(u => u.ChefProfile)
            .WithOne(c => c.User)
            .HasForeignKey<ChefProfile>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a user removes their profile

        // ── ChefProfile → Recipe : one-to-many ───────────────────────────────
        modelBuilder.Entity<ChefProfile>()
            .HasMany(c => c.Recipes)
            .WithOne(r => r.Chef)
            .HasForeignKey(r => r.ChefId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a chef removes their recipes

        // Unique constraint – one email per account
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
