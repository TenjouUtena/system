using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SystemGame.Api.Data.Entities;

namespace SystemGame.Api.Contexts;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Game entities
    public DbSet<Game> Games { get; set; }
    public DbSet<PlayerGame> PlayerGames { get; set; }
    public DbSet<Galaxy> Galaxies { get; set; }
    public DbSet<StarSystem> StarSystems { get; set; }
    public DbSet<Wormhole> Wormholes { get; set; }
    public DbSet<Planet> Planets { get; set; }
    public DbSet<PlanetGrid> PlanetGrids { get; set; }
    public DbSet<GridSquare> GridSquares { get; set; }
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Builder> Builders { get; set; }
    public DbSet<SpaceStation> SpaceStations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure AppUser
        builder.Entity<AppUser>(entity =>
        {
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastLoginAt).IsRequired();
        });

        // Configure Game
        builder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Creator)
                  .WithMany()
                  .HasForeignKey(e => e.CreatorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure Galaxy
        builder.Entity<Galaxy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Game)
                  .WithOne(g => g.Galaxy)
                  .HasForeignKey<Galaxy>(g => g.GameId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Systems)
                  .WithOne(s => s.Galaxy)
                  .HasForeignKey(s => s.GalaxyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PlayerGame
        builder.Entity<PlayerGame>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Game)
                  .WithMany(g => g.Players)
                  .HasForeignKey(e => e.GameId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.GameId }).IsUnique();
        });

        // Configure StarSystem
        builder.Entity<StarSystem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => new { e.GalaxyId, e.X, e.Y }).IsUnique();
        });

        // Configure Wormhole
        builder.Entity<Wormhole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.SystemA)
                  .WithMany(s => s.WormholeAConnections)
                  .HasForeignKey(e => e.SystemAId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.SystemB)
                  .WithMany(s => s.WormholeBConnections)
                  .HasForeignKey(e => e.SystemBId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_Wormhole_DifferentSystems", "\"SystemAId\" != \"SystemBId\""));
        });

        // Configure Planet
        builder.Entity<Planet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.StarSystem)
                  .WithMany(s => s.Planets)
                  .HasForeignKey(e => e.SystemId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.Size).HasDefaultValue(5);
            entity.HasOne(e => e.Grid)
                  .WithOne(g => g.Planet)
                  .HasForeignKey<PlanetGrid>(g => g.PlanetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure PlanetGrid
        builder.Entity<PlanetGrid>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Squares)
                  .WithOne(s => s.PlanetGrid)
                  .HasForeignKey(s => s.PlanetGridId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.PlanetId).IsUnique();
        });

        // Configure GridSquare
        builder.Entity<GridSquare>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlanetGridId, e.X, e.Y }).IsUnique();
            entity.HasMany(e => e.Buildings)
                  .WithOne(b => b.GridSquare)
                  .HasForeignKey(b => b.GridSquareId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Building
        builder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.GridSquare)
                  .WithMany(g => g.Buildings)
                  .HasForeignKey(e => e.GridSquareId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.Property(e => e.ConstructionProgress).HasDefaultValue(0);
            entity.ToTable(t => t.HasCheckConstraint("CK_Building_ProgressRange", "\"ConstructionProgress\" >= 0 AND \"ConstructionProgress\" <= 100"));
        });

        // Configure Builder
        builder.Entity<Builder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Planet)
                  .WithMany(p => p.Builders)
                  .HasForeignKey(e => e.PlanetId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.AssignedBuilding)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedBuildingId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        });

        // Configure SpaceStation
        builder.Entity<SpaceStation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Player)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.StarSystem)
                  .WithMany(s => s.SpaceStations)
                  .HasForeignKey(e => e.SystemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

