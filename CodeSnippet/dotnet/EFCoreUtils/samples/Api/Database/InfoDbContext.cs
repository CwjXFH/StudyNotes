using Microsoft.EntityFrameworkCore;

namespace Api.Database;

public class InfoDbContext : DbContext
{
    public InfoDbContext(DbContextOptions<InfoDbContext> options)
        : base(options)
    {
    }

    public DbSet<InfoEntity> Infos { set; get; }
}