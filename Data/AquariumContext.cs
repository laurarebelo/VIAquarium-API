using Microsoft.EntityFrameworkCore;
using VIAquarium_API.Models;

public class AquariumContext : DbContext
{
    public AquariumContext(DbContextOptions<AquariumContext> options) : base(options) { }

    public DbSet<Fish> Fish { get; set; }
    public DbSet<DeadFish> DeadFish { get; set; }  // New DbSet for DeadFish

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fish>()
            .HasKey(f => f.Id);

        modelBuilder.Entity<Fish>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();

        // Configuration for DeadFish
        modelBuilder.Entity<DeadFish>()
            .HasKey(df => df.Id);

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.Name)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.DateOfBirth)
            .IsRequired();

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.DateOfDeath)
            .IsRequired();

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.CauseOfDeath)
            .IsRequired()
            .HasMaxLength(255);

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.DaysLived)
            .IsRequired();

        modelBuilder.Entity<DeadFish>()
            .Property(df => df.RespectCount)
            .IsRequired()
            .HasDefaultValue(0);
    }
}