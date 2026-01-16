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

            // Seed data - temporarily commented out for GUID migration
            // SeedData(modelBuilder);

            // Global query filters for soft delete
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            modelBuilder.Entity<Game>().HasQueryFilter(g => !g.IsDeleted);
            modelBuilder.Entity<Admin>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
        }

        /*
        // TEMP: Seed data method commented out for GUID migration
        // Will be rewritten with Guid IDs or removed in favor of runtime seeding
        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Regions
            modelBuilder.Entity<Region>().HasData(
                new Region { RegionId = 1, RegionCode = "ASIA", RegionName = "Asia" },
                new Region { RegionId = 2, RegionCode = "EU", RegionName = "Europe" }
            );

            // Seed Countries (India and United Kingdom)
            modelBuilder.Entity<Country>().HasData(
                new Country { CountryId = 1, CountryCode = "IN", CountryName = "India", RegionId = 1, Currency = "INR", Timezone = "Asia/Kolkata", TaxRate = 0.18m },
                new Country { CountryId = 2, CountryCode = "UK", CountryName = "United Kingdom", RegionId = 2, Currency = "GBP", Timezone = "Europe/London", TaxRate = 0.20m }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Action", IsDeleted = false },
                new Category { CategoryId = 2, CategoryName = "Adventure", IsDeleted = false },
                new Category { CategoryId = 3, CategoryName = "RPG", IsDeleted = false },
                new Category { CategoryId = 4, CategoryName = "Sports", IsDeleted = false },
                new Category { CategoryId = 5, CategoryName = "Racing", IsDeleted = false },
                new Category { CategoryId = 6, CategoryName = "Shooter", IsDeleted = false },
                new Category { CategoryId = 7, CategoryName = "Horror", IsDeleted = false },
                new Category { CategoryId = 8, CategoryName = "Platformer", IsDeleted = false }
            );

            // Seed 50 Real PlayStation Games
            modelBuilder.Entity<Game>().HasData(
                new Game { GameId = 1, GameName = "God of War", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2018, 4, 20), FreeToPlay = false, BasePrice = 49.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 2, GameName = "Spider-Man", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2018, 9, 7), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 3, GameName = "The Last of Us Part II", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 6, 19), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 4, GameName = "Ghost of Tsushima", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 7, 17), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 5, GameName = "Horizon Zero Dawn", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2017, 2, 28), FreeToPlay = false, BasePrice = 49.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 6, GameName = "Uncharted 4: A Thief's End", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2016, 5, 10), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 7, GameName = "Bloodborne", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2015, 3, 24), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 8, GameName = "Ratchet & Clank: Rift Apart", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2021, 6, 11), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 9, GameName = "Demon's Souls", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 11, 12), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 10, GameName = "Returnal", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2021, 4, 30), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 11, GameName = "Gran Turismo 7", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2022, 3, 4), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 12, GameName = "Horizon Forbidden West", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2022, 2, 18), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 13, GameName = "Sackboy: A Big Adventure", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 11, 12), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 14, GameName = "Days Gone", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2019, 4, 26), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 15, GameName = "Death Stranding", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2019, 11, 8), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 16, GameName = "Spider-Man: Miles Morales", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 11, 12), FreeToPlay = false, BasePrice = 49.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 17, GameName = "Astro's Playroom", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 11, 12), FreeToPlay = true, BasePrice = 0m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 18, GameName = "Infamous Second Son", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2014, 3, 21), FreeToPlay = false, BasePrice = 29.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 19, GameName = "Until Dawn", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2015, 8, 25), FreeToPlay = false, BasePrice = 29.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 20, GameName = "The Order: 1886", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2015, 2, 20), FreeToPlay = false, BasePrice = 19.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 21, GameName = "Fortnite", PublishedBy = "Epic Games", ReleaseDate = new DateTime(2017, 7, 25), FreeToPlay = true, BasePrice = 0m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 22, GameName = "Call of Duty: Modern Warfare II", PublishedBy = "Activision", ReleaseDate = new DateTime(2022, 10, 28), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 23, GameName = "FIFA 23", PublishedBy = "Electronic Arts", ReleaseDate = new DateTime(2022, 9, 30), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 24, GameName = "NBA 2K23", PublishedBy = "2K Sports", ReleaseDate = new DateTime(2022, 9, 9), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 25, GameName = "Red Dead Redemption 2", PublishedBy = "Rockstar Games", ReleaseDate = new DateTime(2018, 10, 26), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 26, GameName = "Grand Theft Auto V", PublishedBy = "Rockstar Games", ReleaseDate = new DateTime(2013, 9, 17), FreeToPlay = false, BasePrice = 29.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 27, GameName = "The Witcher 3: Wild Hunt", PublishedBy = "CD Projekt", ReleaseDate = new DateTime(2015, 5, 19), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 28, GameName = "Cyberpunk 2077", PublishedBy = "CD Projekt", ReleaseDate = new DateTime(2020, 12, 10), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 29, GameName = "Assassin's Creed Valhalla", PublishedBy = "Ubisoft", ReleaseDate = new DateTime(2020, 11, 10), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 30, GameName = "Far Cry 6", PublishedBy = "Ubisoft", ReleaseDate = new DateTime(2021, 10, 7), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 31, GameName = "Resident Evil Village", PublishedBy = "Capcom", ReleaseDate = new DateTime(2021, 5, 7), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 32, GameName = "Elden Ring", PublishedBy = "Bandai Namco", ReleaseDate = new DateTime(2022, 2, 25), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 33, GameName = "Dark Souls III", PublishedBy = "Bandai Namco", ReleaseDate = new DateTime(2016, 4, 12), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 34, GameName = "Sekiro: Shadows Die Twice", PublishedBy = "Activision", ReleaseDate = new DateTime(2019, 3, 22), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 35, GameName = "Monster Hunter World", PublishedBy = "Capcom", ReleaseDate = new DateTime(2018, 1, 26), FreeToPlay = false, BasePrice = 29.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 36, GameName = "Final Fantasy VII Remake", PublishedBy = "Square Enix", ReleaseDate = new DateTime(2020, 4, 10), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 37, GameName = "Final Fantasy XVI", PublishedBy = "Square Enix", ReleaseDate = new DateTime(2023, 6, 22), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 38, GameName = "Persona 5 Royal", PublishedBy = "Atlus", ReleaseDate = new DateTime(2019, 10, 31), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 39, GameName = "Dragon Ball Z: Kakarot", PublishedBy = "Bandai Namco", ReleaseDate = new DateTime(2020, 1, 17), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 40, GameName = "Mortal Kombat 11", PublishedBy = "Warner Bros", ReleaseDate = new DateTime(2019, 4, 23), FreeToPlay = false, BasePrice = 49.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 41, GameName = "Street Fighter 6", PublishedBy = "Capcom", ReleaseDate = new DateTime(2023, 6, 2), FreeToPlay = false, BasePrice = 69.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 42, GameName = "Tekken 7", PublishedBy = "Bandai Namco", ReleaseDate = new DateTime(2017, 6, 2), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 43, GameName = "Control", PublishedBy = "505 Games", ReleaseDate = new DateTime(2019, 8, 27), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 44, GameName = "It Takes Two", PublishedBy = "Electronic Arts", ReleaseDate = new DateTime(2021, 3, 26), FreeToPlay = false, BasePrice = 39.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 45, GameName = "Apex Legends", PublishedBy = "Electronic Arts", ReleaseDate = new DateTime(2019, 2, 4), FreeToPlay = true, BasePrice = 0m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 46, GameName = "Overwatch 2", PublishedBy = "Blizzard Entertainment", ReleaseDate = new DateTime(2022, 10, 4), FreeToPlay = true, BasePrice = 0m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 47, GameName = "Battlefield 2042", PublishedBy = "Electronic Arts", ReleaseDate = new DateTime(2021, 11, 19), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 48, GameName = "Doom Eternal", PublishedBy = "Bethesda", ReleaseDate = new DateTime(2020, 3, 20), FreeToPlay = false, BasePrice = 59.99m, IsMultiplayer = true, IsDeleted = false },
                new Game { GameId = 49, GameName = "Hades", PublishedBy = "Supergiant Games", ReleaseDate = new DateTime(2020, 9, 17), FreeToPlay = false, BasePrice = 24.99m, IsMultiplayer = false, IsDeleted = false },
                new Game { GameId = 50, GameName = "Stray", PublishedBy = "Annapurna Interactive", ReleaseDate = new DateTime(2022, 7, 19), FreeToPlay = false, BasePrice = 29.99m, IsMultiplayer = false, IsDeleted = false }
            );

            // Seed Game Country Pricing (India INR and UK GBP)
            var gameCountryData = new List<object>();
            for (int gameId = 1; gameId <= 50; gameId++)
            {
                // Get base price from games
                decimal basePrice = gameId switch
                {
                    1 or 5 => 49.99m,
                    2 or 3 or 4 or 12 or 15 or 25 or 27 or 28 or 29 or 30 or 31 or 32 or 34 or 36 or 38 or 39 or 47 or 48 => 59.99m,
                    6 or 7 or 14 or 33 or 35 or 43 or 44 or 50 => 39.99m,
                    8 or 9 or 10 or 11 or 22 or 23 or 24 or 37 or 41 => 69.99m,
                    13 or 16 or 40 or 42 => 49.99m,
                    17 or 21 or 45 or 46 => 0m,
                    18 or 19 => 29.99m,
                    20 => 19.99m,
                    26 => 29.99m,
                    49 => 24.99m,
                    _ => 49.99m
                };

                // India pricing (1 USD ≈ 83 INR)
                decimal inrPrice = basePrice == 0 ? 0 : Math.Round(basePrice * 83, 2);
                gameCountryData.Add(new { GameCountryId = gameId, GameId = gameId, CountryId = 1, Price = inrPrice });

                // UK pricing (1 USD ≈ 0.79 GBP)
                decimal gbpPrice = basePrice == 0 ? 0 : Math.Round(basePrice * 0.79m, 2);
                gameCountryData.Add(new { GameCountryId = gameId + 50, GameId = gameId, CountryId = 2, Price = gbpPrice });
            }
            modelBuilder.Entity<GameCountry>().HasData(gameCountryData.ToArray());

            // Seed Subscription Plans
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan { SubscriptionId = 1, SubscriptionType = "PlayStation Plus Essential" },
                new SubscriptionPlan { SubscriptionId = 2, SubscriptionType = "PlayStation Plus Extra" },
                new SubscriptionPlan { SubscriptionId = 3, SubscriptionType = "PlayStation Plus Premium" }
            );

            // Seed Subscription Plan Country Pricing
            modelBuilder.Entity<SubscriptionPlanCountry>().HasData(
                // Essential - India (INR)
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 1, SubscriptionId = 1, CountryId = 1, DurationMonths = 1, Price = 749m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 2, SubscriptionId = 1, CountryId = 1, DurationMonths = 3, Price = 1999m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 3, SubscriptionId = 1, CountryId = 1, DurationMonths = 12, Price = 4999m },
                // Essential - UK (GBP)
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 4, SubscriptionId = 1, CountryId = 2, DurationMonths = 1, Price = 6.99m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 5, SubscriptionId = 1, CountryId = 2, DurationMonths = 3, Price = 19.99m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 6, SubscriptionId = 1, CountryId = 2, DurationMonths = 12, Price = 49.99m },
                // Extra - India (INR)
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 7, SubscriptionId = 2, CountryId = 1, DurationMonths = 1, Price = 1249m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 8, SubscriptionId = 2, CountryId = 1, DurationMonths = 3, Price = 3299m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 9, SubscriptionId = 2, CountryId = 1, DurationMonths = 12, Price = 8299m },
                // Extra - UK (GBP)
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 10, SubscriptionId = 2, CountryId = 2, DurationMonths = 1, Price = 10.99m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 11, SubscriptionId = 2, CountryId = 2, DurationMonths = 3, Price = 31.99m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 12, SubscriptionId = 2, CountryId = 2, DurationMonths = 12, Price = 83.99m },
                // Premium - India (INR)
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 13, SubscriptionId = 3, CountryId = 1, DurationMonths = 1, Price = 1499m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 14, SubscriptionId = 3, CountryId = 1, DurationMonths = 3, Price = 3999m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 15, SubscriptionId = 3, CountryId = 1, DurationMonths = 12, Price = 9999m },
                // Premium - UK (GBP)
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 16, SubscriptionId = 3, CountryId = 2, DurationMonths = 1, Price = 13.49m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 17, SubscriptionId = 3, CountryId = 2, DurationMonths = 3, Price = 39.99m },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = 18, SubscriptionId = 3, CountryId = 2, DurationMonths = 12, Price = 99.99m }
            );

            // Seed Game-Subscription Associations
            // Essential tier - 15 games (older/indie titles)
            var essentialGames = new[] { 17, 18, 19, 20, 21, 45, 46, 49, 50, 7, 14, 26, 35, 42, 43 };
            // Extra tier - additional 5 games (20 total including Essential)
            var extraGames = new[] { 6, 13, 16, 27, 40 };
            // Premium tier - additional 10 games (30 total including Extra)
            var premiumGames = new[] { 1, 2, 3, 4, 5, 8, 9, 10, 11, 12 };

            var gameSubscriptions = new List<object>();
            int gsId = 1;

            // Essential games in all tiers
            foreach (var gameId in essentialGames)
            {
                gameSubscriptions.Add(new { GameId = gameId, SubscriptionId = 1 });
                gameSubscriptions.Add(new { GameId = gameId, SubscriptionId = 2 });
                gameSubscriptions.Add(new { GameId = gameId, SubscriptionId = 3 });
            }
            // Extra games in Extra and Premium
            foreach (var gameId in extraGames)
            {
                gameSubscriptions.Add(new { GameId = gameId, SubscriptionId = 2 });
                gameSubscriptions.Add(new { GameId = gameId, SubscriptionId = 3 });
            }
            // Premium exclusive games
            foreach (var gameId in premiumGames)
            {
                gameSubscriptions.Add(new { GameId = gameId, SubscriptionId = 3 });
            }
            modelBuilder.Entity<GameSubscription>().HasData(gameSubscriptions.ToArray());


            // Seed 30 Test Users (15 India, 15 UK)
            var users = new List<object>();
            var indianNames = new[] { "Arjun", "Priya", "Raj", "Ananya", "Vikram", "Sneha", "Rohan", "Diya", "Karan", "Meera", "Aditya", "Kavya", "Siddharth", "Isha", "Rahul" };
            var ukNames = new[] { "James", "Emma", "Oliver", "Sophie", "William", "Charlotte", "Harry", "Amelia", "George", "Emily", "Jack", "Isabella", "Thomas", "Mia", "Joshua" };

            for (int i = 0; i < 15; i++)
            {
                // India users
                users.Add(new
                {
                    UserId = i + 1,
                    UserName = indianNames[i].ToLower() + "_ind",
                    UserPassword = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm", // Pass@123
                    Age = 20 + i,
                    UserEmail = $"{indianNames[i].ToLower()}.india@example.com",
                    SubscriptionStatus = (string?)null,
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null,
                    CountryId = (int?)1
                });

                // UK users
                users.Add(new
                {
                    UserId = i + 16,
                    UserName = ukNames[i].ToLower() + "_uk",
                    UserPassword = "$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYIjU6Oe1Xm", // Pass@123
                    Age = 22 + i,
                    UserEmail = $"{ukNames[i].ToLower()}.uk@example.com",
                    SubscriptionStatus = (string?)null,
                    CreatedAt = DateTime.UtcNow.AddDays(-30 + i),
                    IsDeleted = false,
                    DeletedAt = (DateTime?)null,
                    CountryId = (int?)2
                });
            }
            modelBuilder.Entity<User>().HasData(users.ToArray());

            // Seed Game Categories
            var gameCategories = new List<object>
            {
                new { GameId = 1, CategoryId = 1 }, new { GameId = 1, CategoryId = 2 }, // God of War
                new { GameId = 2, CategoryId = 1 }, new { GameId = 2, CategoryId = 2 }, // Spider-Man
                new { GameId = 3, CategoryId = 1 }, new { GameId = 3, CategoryId = 2 }, new { GameId = 3, CategoryId = 7 }, // The Last of Us Part II
                new { GameId = 4, CategoryId = 1 }, new { GameId = 4, CategoryId = 2 }, // Ghost of Tsushima
                new { GameId = 5, CategoryId = 1 }, new { GameId = 5, CategoryId = 2 }, new { GameId = 5, CategoryId = 3 }, // Horizon Zero Dawn
                new { GameId = 6, CategoryId = 1 }, new { GameId = 6, CategoryId = 2 }, new { GameId = 6, CategoryId = 6 }, // Uncharted 4
                new { GameId = 7, CategoryId = 1 }, new { GameId = 7, CategoryId = 3 }, new { GameId = 7, CategoryId = 7 }, // Bloodborne
                new { GameId = 8, CategoryId = 1 }, new { GameId = 8, CategoryId = 8 }, // Ratchet & Clank
                new { GameId = 9, CategoryId = 1 }, new { GameId = 9, CategoryId = 3 }, // Demon's Souls
                new { GameId = 10, CategoryId = 1 }, new { GameId = 10, CategoryId = 6 }, // Returnal
                new { GameId = 11, CategoryId = 5 }, new { GameId = 11, CategoryId = 4 }, // Gran Turismo 7
                new { GameId = 12, CategoryId = 1 }, new { GameId = 12, CategoryId = 2 }, new { GameId = 12, CategoryId = 3 }, // Horizon Forbidden West
                new { GameId = 21, CategoryId = 1 }, new { GameId = 21, CategoryId = 6 }, // Fortnite
                new { GameId = 22, CategoryId = 1 }, new { GameId = 22, CategoryId = 6 }, // COD MW2
                new { GameId = 23, CategoryId = 4 }, // FIFA 23
                new { GameId = 25, CategoryId = 1 }, new { GameId = 25, CategoryId = 2 }, // RDR2
                new { GameId = 27, CategoryId = 3 }, new { GameId = 27, CategoryId = 2 }, // Witcher 3
                new { GameId = 32, CategoryId = 3 }, new { GameId = 32, CategoryId = 1 }, // Elden Ring
                new { GameId = 36, CategoryId = 3 }, new { GameId = 36, CategoryId = 1 }, // FF7 Remake
                new { GameId = 38, CategoryId = 3 }, // Persona 5 Royal
                new { GameId = 41, CategoryId = 1 }, // Street Fighter 6
                new { GameId = 45, CategoryId = 1 }, new { GameId = 45, CategoryId = 6 }, // Apex Legends
                new { GameId = 49, CategoryId = 1 }, new { GameId = 49, CategoryId = 3 } // Hades
            };
            modelBuilder.Entity<GameCategory>().HasData(gameCategories.ToArray());

            // Seed Some Purchases
            var purchases = new List<object>
            {
                new { PurchaseId = 1, UserId = 1, GameId = 1, PurchasePrice = 4149.17m, PurchaseDate = DateTime.UtcNow.AddDays(-10) }, // India user bought God of War
                new { PurchaseId = 2, UserId = 2, GameId = 27, PurchasePrice = 3319.17m, PurchaseDate = DateTime.UtcNow.AddDays(-8) }, // India user bought Witcher 3
                new { PurchaseId = 3, UserId = 16, GameId = 2, PurchasePrice = 47.39m, PurchaseDate = DateTime.UtcNow.AddDays(-5) }, // UK user bought Spider-Man
                new { PurchaseId = 4, UserId = 17, GameId = 32, PurchasePrice = 47.39m, PurchaseDate = DateTime.UtcNow.AddDays(-3) }, // UK user bought Elden Ring
                new { PurchaseId = 5, UserId = 3, GameId = 22, PurchasePrice = 5809.17m, PurchaseDate = DateTime.UtcNow.AddDays(-2) } // India user bought COD MW2
            };
            modelBuilder.Entity<UserPurchaseGame>().HasData(purchases.ToArray());

            // Seed Some Active Subscriptions
            var subscriptions = new List<object>
            {
                new { UserSubscriptionId = 1, UserId = 4, SubscriptionPlanCountryId = 3, PlanStartDate = DateTime.UtcNow.AddDays(-20), PlanEndDate = DateTime.UtcNow.AddDays(345) }, // India user - Essential 12mo
                new { UserSubscriptionId = 2, UserId = 5, SubscriptionPlanCountryId = 9, PlanStartDate = DateTime.UtcNow.AddDays(-15), PlanEndDate = DateTime.UtcNow.AddDays(350) }, // India user - Extra 12mo
                new { UserSubscriptionId = 3, UserId = 18, SubscriptionPlanCountryId = 6, PlanStartDate = DateTime.UtcNow.AddDays(-25), PlanEndDate = DateTime.UtcNow.AddDays(340) }, // UK user - Essential 12mo
                new { UserSubscriptionId = 4, UserId = 19, SubscriptionPlanCountryId = 18, PlanStartDate = DateTime.UtcNow.AddDays(-10), PlanEndDate = DateTime.UtcNow.AddDays(355) } // UK user - Premium 12mo
            };
            modelBuilder.Entity<UserSubscriptionPlan>().HasData(subscriptions.ToArray());
        }
        // END OF COMMENTED SEED DATA METHOD */
    }
}
