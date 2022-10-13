using Microsoft.EntityFrameworkCore;

namespace DAPIs.Db;

public class HeroDbContext : DbContext
{
    public HeroDbContext(DbContextOptions<HeroDbContext> options) : base(options)
    {
    }

    public DbSet<Hero> Heroes { set; get; }
    
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .LogTo(Console.WriteLine)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging();
    
}