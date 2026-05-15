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

        // =========================
        // MAIN TABLES
        // =========================

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Payment> Payments { get; set; }

        // =========================
        // USER / PROFILE TABLES
        // =========================

        public DbSet<SellerApplication> SellerApplications { get; set; }

        public DbSet<ShopProfile> ShopProfiles { get; set; }

        public DbSet<RiderProfile> RiderProfiles { get; set; }

        // =========================
        // AUDIT LOGS
        // =========================

        public DbSet<AuditLog> AuditLogs { get; set; }

        public DbSet<SuperAdminAuditLog> SuperAdminAuditLogs { get; set; }

        // =========================
        // SYSTEM SETTINGS
        // =========================

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<CommissionRate> CommissionRates { get; set; }

        // =========================
        // PAYMENTS / EARNINGS
        // =========================

        public DbSet<PayMongoWebhookLog> PayMongoWebhookLogs { get; set; }

        public DbSet<SellerPayout> SellerPayouts { get; set; }

        public DbSet<PlatformEarning> PlatformEarnings { get; set; }

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<Refund> Refunds { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // =========================
            // SELLER APPLICATION
            // =========================

            builder.Entity<SellerApplication>()
                .HasOne(sa => sa.User)
                .WithMany(u => u.SellerApplications)
                .HasForeignKey(sa => sa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // SUPER ADMIN AUDIT LOG
            // =========================

            builder.Entity<SuperAdminAuditLog>()
                .HasOne(a => a.SuperAdmin)
                .WithMany()
                .HasForeignKey(a => a.SuperAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // TABLE NAMES
            // =========================

            builder.Entity<AuditLog>()
                .ToTable("AuditLogs");

            builder.Entity<SuperAdminAuditLog>()
                .ToTable("SuperAdminAuditLogs");

            builder.Entity<SystemSetting>()
                .ToTable("SystemSettings");
        }
    }
}