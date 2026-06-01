using Microsoft.EntityFrameworkCore;
using NailArtHub.Models;

namespace NailArtHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Shop> Shops { get; set; }
        public DbSet<NailTag> NailTags { get; set; }
        public DbSet<NailTrend> NailTrends { get; set; }
    }
}
