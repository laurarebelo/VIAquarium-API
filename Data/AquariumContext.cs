using Microsoft.EntityFrameworkCore;

public class AquariumContext : DbContext
{
    public AquariumContext(DbContextOptions<AquariumContext> options) : base(options) { }

    public DbSet<Fish> Fish { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fish>()
            .HasKey(f => f.Id);

        modelBuilder.Entity<Fish>()
            .Property(f => f.Id)
            .ValueGeneratedOnAdd();
    }
}