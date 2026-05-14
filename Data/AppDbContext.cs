using BuyZaar.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<SellerApplication> SellerApplications { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }    
        public DbSet<ShopProfile> ShopProfiles { get; set; }
        public DbSet<RiderProfile> RiderProfiles { get; set; }
        public DbSet<PayMongoWebhookLog> PayMongoWebhookLogs { get; set; }

public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
public DbSet<SuperAdminAuditLog> SuperAdminAuditLogs { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<CommissionRate> CommissionRates { get; set; }
public DbSet<SellerPayout> SellerPayouts { get; set; }
public DbSet<PlatformEarning> PlatformEarnings { get; set; }
public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
public DbSet<Refund> Refunds { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SellerApplication>()
                .HasOne(sa => sa.User)
                .WithMany(u => u.SellerApplications)
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}