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
        public DbSet<UsersRegion> UsersRegions { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<SubscriptionPlanRegion> SubscriptionPlanRegions { get; set; }
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

            // Configure User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.UserEmail).IsUnique();
            });

            // Configure UsersRegion
            modelBuilder.Entity<UsersRegion>(entity =>
            {
                entity.HasKey(ur => ur.UserRegionId);
                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UsersRegions)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Region)
                    .WithMany(r => r.UsersRegions)
                    .HasForeignKey(ur => ur.RegionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Game
            modelBuilder.Entity<Game>(entity =>
            {
                entity.HasKey(g => g.GameId);
                entity.Property(g => g.Price).HasColumnType("decimal(10,2)");
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

            // Configure SubscriptionPlanRegion
            modelBuilder.Entity<SubscriptionPlanRegion>(entity =>
            {
                entity.HasKey(spr => spr.SubscriptionPlanRegionId);
                entity.HasIndex(spr => new { spr.SubscriptionId, spr.RegionId, spr.DurationMonths }).IsUnique();
                entity.Property(spr => spr.Price).HasColumnType("decimal(10,2)");

                entity.HasOne(spr => spr.SubscriptionPlan)
                    .WithMany(sp => sp.SubscriptionPlanRegions)
                    .HasForeignKey(spr => spr.SubscriptionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(spr => spr.Region)
                    .WithMany(r => r.SubscriptionPlanRegions)
                    .HasForeignKey(spr => spr.RegionId)
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

                entity.HasOne(usp => usp.SubscriptionPlanRegion)
                    .WithMany(spr => spr.UserSubscriptionPlans)
                    .HasForeignKey(usp => usp.SubscriptionPlanRegionId)
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

            // Seed data
            SeedData(modelBuilder);

            // Global query filters for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Game>().HasQueryFilter(g => !g.IsDeleted);
            modelBuilder.Entity<Admin>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Regions
            modelBuilder.Entity<Region>().HasData(
                new Region { RegionId = 1, RegionCode = "US", RegionName = "United States", Currency = "USD", RegionTimezone = "America/New_York" },
                new Region { RegionId = 2, RegionCode = "EU", RegionName = "Europe", Currency = "EUR", RegionTimezone = "Europe/London" },
                new Region { RegionId = 3, RegionCode = "JP", RegionName = "Japan", Currency = "JPY", RegionTimezone = "Asia/Tokyo" },
                new Region { RegionId = 4, RegionCode = "IN", RegionName = "India", Currency = "INR", RegionTimezone = "Asia/Kolkata" }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Action", IsDeleted = false },
                new Category { CategoryId = 2, CategoryName = "Adventure", IsDeleted = false },
                new Category { CategoryId = 3, CategoryName = "RPG", IsDeleted = false },
                new Category { CategoryId = 4, CategoryName = "Sports", IsDeleted = false },
                new Category { CategoryId = 5, CategoryName = "Racing", IsDeleted = false }
            );

            // Seed Games
            modelBuilder.Entity<Game>().HasData(
                new Game { GameId = 1, GameName = "God of War", PublishedBy = "Sony", ReleaseDate = new DateTime(2018, 4, 20), FreeToPlay = false, Price = 49.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 2, GameName = "Spider-Man", PublishedBy = "Sony", ReleaseDate = new DateTime(2018, 9, 7), FreeToPlay = false, Price = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 3, GameName = "Fortnite", PublishedBy = "Epic Games", ReleaseDate = new DateTime(2017, 7, 25), FreeToPlay = true, Price = 0m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 4, GameName = "Gran Turismo 7", PublishedBy = "Sony", ReleaseDate = new DateTime(2022, 3, 4), FreeToPlay = false, Price = 69.99m, IsMultiplayer = true, IsDeleted = false }
            );

            // Seed GameCategory
            modelBuilder.Entity<GameCategory>().HasData(
                new GameCategory { GameId = 1, CategoryId = 1 }, 
                new GameCategory { GameId = 1, CategoryId = 2 }, // God of War - Adventure
                new GameCategory { GameId = 2, CategoryId = 1 }, // Spider-Man - Action
                new GameCategory { GameId = 2, CategoryId = 2 }, // Spider-Man - Adventure
                new GameCategory { GameId = 3, CategoryId = 1 }, // Fortnite - Action
                new GameCategory { GameId = 4, CategoryId = 5 }  // Gran Turismo - Racing
            );

            // Seed SubscriptionPlans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { SubscriptionId = 1, SubscriptionType = "PlayStation Plus Essential" },
                new SubscriptionPlan { SubscriptionId = 2, SubscriptionType = "PlayStation Plus Extra" },
                new SubscriptionPlan { SubscriptionId = 3, SubscriptionType = "PlayStation Plus Premium" }
            );

            // Seed SubscriptionPlanRegion (region-specific pricing with duration options)
            modelBuilder.Entity<SubscriptionPlanRegion>().HasData(
                // Essential - US - 1 month
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 1, SubscriptionId = 1, RegionId = 1, DurationMonths = 1, Price = 9.99m, Currency = "USD" },
                // Essential - US - 3 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 2, SubscriptionId = 1, RegionId = 1, DurationMonths = 3, Price = 24.99m, Currency = "USD" },
                // Essential - US - 12 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 3, SubscriptionId = 1, RegionId = 1, DurationMonths = 12, Price = 59.99m, Currency = "USD" },
                // Essential - EU - 1 month
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 4, SubscriptionId = 1, RegionId = 2, DurationMonths = 1, Price = 8.99m, Currency = "EUR" },
                // Essential - EU - 3 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 5, SubscriptionId = 1, RegionId = 2, DurationMonths = 3, Price = 22.99m, Currency = "EUR" },
                // Essential - EU - 12 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 6, SubscriptionId = 1, RegionId = 2, DurationMonths = 12, Price = 54.99m, Currency = "EUR" },
                // Extra - US - 1 month
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 7, SubscriptionId = 2, RegionId = 1, DurationMonths = 1, Price = 14.99m, Currency = "USD" },
                // Extra - US - 3 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 8, SubscriptionId = 2, RegionId = 1, DurationMonths = 3, Price = 39.99m, Currency = "USD" },
                // Extra - US - 12 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 9, SubscriptionId = 2, RegionId = 1, DurationMonths = 12, Price = 99.99m, Currency = "USD" },
                // Extra - EU - 1 month
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 10, SubscriptionId = 2, RegionId = 2, DurationMonths = 1, Price = 13.99m, Currency = "EUR" },
                // Extra - EU - 3 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 11, SubscriptionId = 2, RegionId = 2, DurationMonths = 3, Price = 36.99m, Currency = "EUR" },
                // Extra - EU - 12 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 12, SubscriptionId = 2, RegionId = 2, DurationMonths = 12, Price = 89.99m, Currency = "EUR" },
                // Premium - US - 1 month
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 13, SubscriptionId = 3, RegionId = 1, DurationMonths = 1, Price = 17.99m, Currency = "USD" },
                // Premium - US - 3 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 14, SubscriptionId = 3, RegionId = 1, DurationMonths = 3, Price = 49.99m, Currency = "USD" },
                // Premium - US - 12 months
                new SubscriptionPlanRegion { SubscriptionPlanRegionId = 15, SubscriptionId = 3, RegionId = 1, DurationMonths = 12, Price = 119.99m, Currency = "USD" }
            );

            // Seed GameSubscription (games included in subscription plans)
            modelBuilder.Entity<GameSubscription>().HasData(
                new GameSubscription { GameId = 1, SubscriptionId = 2 }, // God of War in Extra
                new GameSubscription { GameId = 1, SubscriptionId = 3 }, // God of War in Premium
                new GameSubscription { GameId = 2, SubscriptionId = 3 }  // Spider-Man in Premium
            );

            // Seed Admin
            modelBuilder.Entity<Admin>().HasData(
                new Admin { AdminId = 1, AdminEmail = "admin@psstore.com", AdminPassword = "Admin@123", CreatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            // Seed Test Users
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, UserName = "john_doe", UserPassword = "Pass@123", Age = 28, UserEmail = "john@example.com", SubscriptionStatus = null, CreatedAt = DateTime.UtcNow, IsDeleted = false },
                new User { UserId = 2, UserName = "jane_smith", UserPassword = "Pass@456", Age = 25, UserEmail = "jane@example.com", SubscriptionStatus = "Active", CreatedAt = DateTime.UtcNow, IsDeleted = false }
            );

            // Seed UsersRegion (assign users to regions)
            modelBuilder.Entity<UsersRegion>().HasData(
                new UsersRegion { UserRegionId = 1, UserId = 1, RegionId = 1, StartDate = DateTime.UtcNow, IsActive = true },
                new UsersRegion { UserRegionId = 2, UserId = 2, RegionId = 2, StartDate = DateTime.UtcNow, IsActive = true }
            );
        }
    }
}
