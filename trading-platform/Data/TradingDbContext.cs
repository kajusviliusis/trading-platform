using Microsoft.EntityFrameworkCore;
using trading_platform.Models.Entities;

namespace trading_platform.Data
{
    public class TradingDbContext : DbContext
    {
        public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Holding> Holdings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Wallet)
                .WithOne(w => w.User)
                .HasForeignKey<Wallet>(w => w.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Stock>()
                .HasMany(s => s.Orders)
                .WithOne(o => o.Stock)
                .HasForeignKey(o => o.StockId);
            modelBuilder.Entity<Holding>()
                .HasOne(h => h.Stock)
                .WithMany()
                .HasForeignKey(h => h.StockId);

            base.OnModelCreating(modelBuilder);
        }

    }
}
