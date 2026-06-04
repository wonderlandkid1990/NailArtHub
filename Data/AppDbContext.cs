using Microsoft.EntityFrameworkCore;
using NailArtHub.Models;
using NailArtHub.Models.NailArtHub.Models;

namespace NailArtHub.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<NewApply> NewApplies { get; set; }
        public DbSet<ShopTagBridge> ShopTagBridges { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<NailTag> NailTags { get; set; }
        public DbSet<NailTrend> NailTrends { get; set; }
    }
}
