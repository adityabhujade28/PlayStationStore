using Microsoft.EntityFrameworkCore;
using PSstore.Models;

namespace PSstore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<GameCountry> GameCountries { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<SubscriptionPlanCountry> SubscriptionPlanCountries { get; set; }
        public DbSet<UserSubscriptionPlan> UserSubscriptionPlans { get; set; }
        public DbSet<GameSubscription> GameSubscriptions { get; set; }
        public DbSet<UserPurchaseGame> UserPurchaseGames { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Region
            modelBuilder.Entity<Region>(entity =>
            {
                entity.HasKey(r => r.RegionId);
                entity.HasIndex(r => r.RegionCode).IsUnique();
                entity.Property(r => r.RegionCode).IsRequired().HasMaxLength(10);
            });

            // Configure Country
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(c => c.CountryId);
                entity.HasIndex(c => c.CountryCode).IsUnique();
                entity.Property(c => c.CountryCode).IsRequired().HasMaxLength(10);
                entity.Property(c => c.Currency).IsRequired().HasMaxLength(10);

                entity.HasOne(c => c.Region)
                    .WithMany(r => r.Countries)
                    .HasForeignKey(c => c.RegionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.UserEmail).IsUnique();

                entity.HasOne(u => u.Country)
                    .WithMany(c => c.Users)
                    .HasForeignKey(u => u.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Game
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.GameId);
            });

            // Configure GameCountry (game pricing per country)
            modelBuilder.Entity<GameCountry>(entity =>
            {
                entity.HasKey(gc => gc.GameCountryId);
                entity.HasIndex(gc => new { gc.GameId, gc.CountryId }).IsUnique();
                entity.Property(gc => gc.Price).HasColumnType("decimal(10,2)");

                entity.HasOne(gc => gc.Game)
                    .WithMany(g => g.GameCountries)
                    .HasForeignKey(gc => gc.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gc => gc.Country)
                    .WithMany(c => c.GameCountries)
                    .HasForeignKey(gc => gc.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);
            });

            // Configure GameCategory (many-to-many)
            modelBuilder.Entity<GameCategory>(entity =>
            {
                entity.HasKey(gc => new { gc.GameId, gc.CategoryId });

                entity.HasOne(gc => gc.Game)
                    .WithMany(g => g.GameCategories)
                    .HasForeignKey(gc => gc.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gc => gc.Category)
                    .WithMany(c => c.GameCategories)
                    .HasForeignKey(gc => gc.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure SubscriptionPlan
            modelBuilder.Entity<SubscriptionPlan>(entity =>
            {
                entity.HasKey(sp => sp.SubscriptionId);
            });

            // Configure SubscriptionPlanCountry
            modelBuilder.Entity<SubscriptionPlanCountry>(entity =>
            {
                entity.HasKey(spc => spc.SubscriptionPlanCountryId);
                entity.HasIndex(spc => new { spc.SubscriptionId, spc.CountryId, spc.DurationMonths }).IsUnique();
                entity.Property(spc => spc.Price).HasColumnType("decimal(10,2)");

                entity.HasOne(spc => spc.SubscriptionPlan)
                    .WithMany(sp => sp.SubscriptionPlanCountries)
                    .HasForeignKey(spc => spc.SubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(spc => spc.Country)
                    .WithMany(c => c.SubscriptionPlanCountries)
                    .HasForeignKey(spc => spc.CountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GameSubscription (many-to-many)
            modelBuilder.Entity<GameSubscription>(entity =>
            {
                entity.HasKey(gs => new { gs.GameId, gs.SubscriptionId });

                entity.HasOne(gs => gs.Game)
                    .WithMany(g => g.GameSubscriptions)
                    .HasForeignKey(gs => gs.GameId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gs => gs.SubscriptionPlan)
                    .WithMany(sp => sp.GameSubscriptions)
                    .HasForeignKey(gs => gs.SubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure UserSubscriptionPlan
            modelBuilder.Entity<UserSubscriptionPlan>(entity =>
            {
                entity.HasKey(usp => usp.UserSubscriptionId);

                entity.HasOne(usp => usp.User)
                    .WithMany(u => u.UserSubscriptionPlans)
                    .HasForeignKey(usp => usp.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(usp => usp.SubscriptionPlanCountry)
                    .WithMany(spc => spc.UserSubscriptionPlans)
                    .HasForeignKey(usp => usp.SubscriptionPlanCountryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure UserPurchaseGame (IMMUTABLE - Restrict delete)
            modelBuilder.Entity<UserPurchaseGame>(entity =>
            {
                entity.HasKey(upg => upg.PurchaseId);
                entity.Property(upg => upg.PurchasePrice).HasColumnType("decimal(10,2)");

                entity.HasOne(upg => upg.User)
                    .WithMany(u => u.UserPurchasedGames)
                    .HasForeignKey(upg => upg.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(upg => upg.Game)
                    .WithMany(g => g.UserPurchases)
                    .HasForeignKey(upg => upg.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Cart
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(c => c.CartId);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Carts)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);  // Cannot delete user with cart
            });

            // Configure CartItem
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.CartItemId);
                entity.Property(ci => ci.UnitPrice).HasColumnType("decimal(10,2)");
                entity.Property(ci => ci.TotalPrice).HasColumnType("decimal(10,2)");

                entity.HasOne(ci => ci.Cart)
                    .WithMany(c => c.CartItems)
                    .HasForeignKey(ci => ci.CartId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Game)
                    .WithMany(g => g.CartItems)
                    .HasForeignKey(ci => ci.GameId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Global query filters for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Game>().HasQueryFilter(g => !g.IsDeleted);
            modelBuilder.Entity<Admin>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is User || e.Entity is Game || e.Entity is SubscriptionPlan)
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Use reflection or interface if possible, but identifying by type is safe here
                if (entry.Entity is User user) user.UpdatedAt = DateTime.UtcNow;
                if (entry.Entity is Game game) game.UpdatedAt = DateTime.UtcNow;
                if (entry.Entity is SubscriptionPlan plan) plan.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
