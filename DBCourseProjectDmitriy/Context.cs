using DBCourseProjectDmitriy.Models;
using Microsoft.EntityFrameworkCore;

namespace DBCourseProjectDmitriy
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Manufacturer>().HasIndex(p => p.Name).IsUnique();
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ElectronicsType> ElectronicsTypes { get; set; }
        public DbSet<Electronics> Electronics { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set;}
        public DbSet<Bill> Bills { get; set; }
        public DbSet<OrderedGoods> OrderedGoods { get; set; }
    }
}
