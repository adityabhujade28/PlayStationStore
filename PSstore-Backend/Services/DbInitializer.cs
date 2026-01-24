using PSstore.Data;
using PSstore.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Bogus;

namespace PSstore.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Check if database is already seeded
        // Check if database is already seeded (Regions only, we might need to seed Admin later)
        bool isDataSeeded = await context.Regions.AnyAsync();

        if (!isDataSeeded)
        {

        // Use fixed Guids for reference data (easier testing)
        var asiaId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var europeId = Guid.Parse("10000000-0000-0000-0000-000000000002");

        var indiaId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        var ukId = Guid.Parse("20000000-0000-0000-0000-000000000002");

        // Seed Regions
        var regions = new[]
        {
            new Region
            {
                RegionId = asiaId,
                RegionCode = "AS",
                RegionName = "Asia"
            },
            new Region
            {
                RegionId = europeId,
                RegionCode = "EU",
                RegionName = "Europe"
            }
        };
        context.Regions.AddRange(regions);

        // Seed Countries
        var countries = new[]
        {
            new Country
            {
                CountryId = indiaId,
                CountryCode = "IN",
                CountryName = "India",
                RegionId = asiaId,
                Currency = "INR",
                Timezone = "IST",
                TaxRate = 0.18m
            },
            new Country
            {
                CountryId = ukId,
                CountryCode = "GB",
                CountryName = "United Kingdom",
                RegionId = europeId,
                Currency = "GBP",
                Timezone = "GMT",
                TaxRate = 0.20m
            }
        };
        context.Countries.AddRange(countries);

        // Seed Categories
        var actionId = Guid.NewGuid();
        var adventureId = Guid.NewGuid();
        var rpgId = Guid.NewGuid();
        var sportsId = Guid.NewGuid();
        var racingId = Guid.NewGuid();
        var shooterId = Guid.NewGuid();
        var horrorId = Guid.NewGuid();
        var platformerId = Guid.NewGuid();

        var categories = new[]
        {
            new Category { CategoryId = actionId, CategoryName = "Action" },
            new Category { CategoryId = adventureId, CategoryName = "Adventure" },
            new Category { CategoryId = rpgId, CategoryName = "RPG" },
            new Category { CategoryId = sportsId, CategoryName = "Sports" },
            new Category { CategoryId = racingId, CategoryName = "Racing" },
            new Category { CategoryId = shooterId, CategoryName = "Shooter" },
            new Category { CategoryId = horrorId, CategoryName = "Horror" },
            new Category { CategoryId = platformerId, CategoryName = "Platformer" }
        };
        context.Categories.AddRange(categories);

        // Seed Subscription Plans
        var essentialId = Guid.NewGuid();
        var extraId = Guid.NewGuid();
        var premiumId = Guid.NewGuid();

        var subscriptions = new[]
        {
            new SubscriptionPlan { SubscriptionId = essentialId, SubscriptionType = "Essential" },
            new SubscriptionPlan { SubscriptionId = extraId, SubscriptionType = "Extra" },
            new SubscriptionPlan { SubscriptionId = premiumId, SubscriptionType = "Premium" }
        };
        context.SubscriptionPlans.AddRange(subscriptions);

        await context.SaveChangesAsync(); // Save reference data first

        // Seed Subscription Plan Countries (pricing for each plan in each country)
        var subscriptionPlanCountries = new List<SubscriptionPlanCountry>();

        // Essential Plan
        subscriptionPlanCountries.AddRange(new[]
        {
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = essentialId, CountryId = indiaId, DurationMonths = 1, Price = 499m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = essentialId, CountryId = indiaId, DurationMonths = 3, Price = 1349m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = essentialId, CountryId = indiaId, DurationMonths = 12, Price = 4999m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = essentialId, CountryId = ukId, DurationMonths = 1, Price = 6.99m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = essentialId, CountryId = ukId, DurationMonths = 3, Price = 19.99m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = essentialId, CountryId = ukId, DurationMonths = 12, Price = 59.99m }
        });

        // Extra Plan
        subscriptionPlanCountries.AddRange(new[]
        {
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = extraId, CountryId = indiaId, DurationMonths = 1, Price = 749m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = extraId, CountryId = indiaId, DurationMonths = 3, Price = 2099m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = extraId, CountryId = indiaId, DurationMonths = 12, Price = 7499m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = extraId, CountryId = ukId, DurationMonths = 1, Price = 10.99m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = extraId, CountryId = ukId, DurationMonths = 3, Price = 31.99m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = extraId, CountryId = ukId, DurationMonths = 12, Price = 99.99m }
        });

        // Premium Plan
        subscriptionPlanCountries.AddRange(new[]
        {
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = premiumId, CountryId = indiaId, DurationMonths = 1, Price = 999m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = premiumId, CountryId = indiaId, DurationMonths = 3, Price = 2799m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = premiumId, CountryId = indiaId, DurationMonths = 12, Price = 9999m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = premiumId, CountryId = ukId, DurationMonths = 1, Price = 13.99m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = premiumId, CountryId = ukId, DurationMonths = 3, Price = 39.99m },
            new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = premiumId, CountryId = ukId, DurationMonths = 12, Price = 119.99m }
        });

        context.SubscriptionPlanCountries.AddRange(subscriptionPlanCountries);

        // Seed Games (20 popular games)
        var games = new[]
        {
            new Game { GameId = Guid.NewGuid(), GameName = "God of War Ragnarök", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2022, 11, 9), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Marvel's Spider-Man Remastered", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2022, 8, 12), FreeToPlay = false, BasePrice = 3999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "The Last of Us Part II", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 6, 19), FreeToPlay = false, BasePrice = 3999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Elden Ring", PublishedBy = "FromSoftware", ReleaseDate = new DateTime(2022, 2, 25), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Horizon Forbidden West", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2022, 2, 18), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Cyberpunk 2077", PublishedBy = "CD Projekt Red", ReleaseDate = new DateTime(2020, 12, 10), FreeToPlay = false, BasePrice = 2999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Red Dead Redemption 2", PublishedBy = "Rockstar Games", ReleaseDate = new DateTime(2018, 10, 26), FreeToPlay = false, BasePrice = 3999m, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Ghost of Tsushima", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2020, 7, 17), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Ratchet & Clank: Rift Apart", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2021, 6, 11), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Grand Theft Auto V", PublishedBy = "Rockstar Games", ReleaseDate = new DateTime(2013, 9, 17), FreeToPlay = false, BasePrice = 1999m, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Call of Duty: Modern Warfare II", PublishedBy = "Activision", ReleaseDate = new DateTime(2022, 10, 28), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "FIFA 23", PublishedBy = "EA Sports", ReleaseDate = new DateTime(2022, 9, 30), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Fortnite", PublishedBy = "Epic Games", ReleaseDate = new DateTime(2017, 7, 25), FreeToPlay = true, BasePrice = null, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Apex Legends", PublishedBy = "EA", ReleaseDate = new DateTime(2019, 2, 4), FreeToPlay = true, BasePrice = null, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Resident Evil 4 Remake", PublishedBy = "Capcom", ReleaseDate = new DateTime(2023, 3, 24), FreeToPlay = false, BasePrice = 3999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Gran Turismo 7", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2022, 3, 4), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = true },
            new Game { GameId = Guid.NewGuid(), GameName = "Returnal", PublishedBy = "Sony Interactive Entertainment", ReleaseDate = new DateTime(2021, 4, 30), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Hogwarts Legacy", PublishedBy = "Warner Bros. Games", ReleaseDate = new DateTime(2023, 2, 10), FreeToPlay = false, BasePrice = 4999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Assassin's Creed Valhalla", PublishedBy = "Ubisoft", ReleaseDate = new DateTime(2020, 11, 10), FreeToPlay = false, BasePrice = 3999m, IsMultiplayer = false },
            new Game { GameId = Guid.NewGuid(), GameName = "Minecraft", PublishedBy = "Mojang Studios", ReleaseDate = new DateTime(2011, 11, 18), FreeToPlay = false, BasePrice = 1999m, IsMultiplayer = true }
        };
        context.Games.AddRange(games);
        await context.SaveChangesAsync();
        // Initialize collections for legacy logic
        var allGames = games.ToList(); 
        var gameList = games.ToList();
        var gameCountries = new List<GameCountry>();
        foreach (var game in allGames)
        {
            if (game.BasePrice.HasValue)
            {
                // India pricing (using BasePrice as reference)
                gameCountries.Add(new GameCountry
                {
                    GameCountryId = Guid.NewGuid(),
                    GameId = game.GameId,
                    CountryId = indiaId,
                    Price = game.BasePrice.Value
                });

                // UK pricing (convert INR to GBP approximately: 1 GBP ≈ 100 INR)
                gameCountries.Add(new GameCountry
                {
                    GameCountryId = Guid.NewGuid(),
                    GameId = game.GameId,
                    CountryId = ukId,
                    Price = Math.Round(game.BasePrice.Value / 100m, 2)
                });
            }
        }
        context.GameCountries.AddRange(gameCountries);

        // Seed GameCategories (categorize games)
        // Seed GameCategories for Random Games
        var gameCategories = new List<GameCategory>();
        // Note: The fixed games are already categorized in the first block or existing DB.
        // We only categorize the *new* randomGames here.

        // God of War Ragnarök - Action, Adventure
        gameCategories.Add(new GameCategory { GameId = gameList[0].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[0].GameId, CategoryId = adventureId });

        // Spider-Man - Action, Adventure
        gameCategories.Add(new GameCategory { GameId = gameList[1].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[1].GameId, CategoryId = adventureId });

        // The Last of Us Part II - Action, Adventure, Horror
        gameCategories.Add(new GameCategory { GameId = gameList[2].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[2].GameId, CategoryId = adventureId });
        gameCategories.Add(new GameCategory { GameId = gameList[2].GameId, CategoryId = horrorId });

        // Elden Ring - Action, RPG
        gameCategories.Add(new GameCategory { GameId = gameList[3].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[3].GameId, CategoryId = rpgId });

        // Horizon Forbidden West - Action, RPG
        gameCategories.Add(new GameCategory { GameId = gameList[4].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[4].GameId, CategoryId = rpgId });

        // Cyberpunk 2077 - Action, RPG
        gameCategories.Add(new GameCategory { GameId = gameList[5].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[5].GameId, CategoryId = rpgId });

        // Red Dead Redemption 2 - Action, Adventure
        gameCategories.Add(new GameCategory { GameId = gameList[6].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[6].GameId, CategoryId = adventureId });

        // Ghost of Tsushima - Action, Adventure
        gameCategories.Add(new GameCategory { GameId = gameList[7].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[7].GameId, CategoryId = adventureId });

        // Ratchet & Clank - Action, Platformer
        gameCategories.Add(new GameCategory { GameId = gameList[8].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[8].GameId, CategoryId = platformerId });

        // GTA V - Action, Adventure
        gameCategories.Add(new GameCategory { GameId = gameList[9].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[9].GameId, CategoryId = adventureId });

        // Call of Duty - Shooter, Action
        gameCategories.Add(new GameCategory { GameId = gameList[10].GameId, CategoryId = shooterId });
        gameCategories.Add(new GameCategory { GameId = gameList[10].GameId, CategoryId = actionId });

        // FIFA 23 - Sports
        gameCategories.Add(new GameCategory { GameId = gameList[11].GameId, CategoryId = sportsId });

        // Fortnite - Shooter, Action
        gameCategories.Add(new GameCategory { GameId = gameList[12].GameId, CategoryId = shooterId });
        gameCategories.Add(new GameCategory { GameId = gameList[12].GameId, CategoryId = actionId });

        // Apex Legends - Shooter, Action
        gameCategories.Add(new GameCategory { GameId = gameList[13].GameId, CategoryId = shooterId });
        gameCategories.Add(new GameCategory { GameId = gameList[13].GameId, CategoryId = actionId });

        // Resident Evil 4 - Horror, Action
        gameCategories.Add(new GameCategory { GameId = gameList[14].GameId, CategoryId = horrorId });
        gameCategories.Add(new GameCategory { GameId = gameList[14].GameId, CategoryId = actionId });

        // Gran Turismo 7 - Racing, Sports
        gameCategories.Add(new GameCategory { GameId = gameList[15].GameId, CategoryId = racingId });
        gameCategories.Add(new GameCategory { GameId = gameList[15].GameId, CategoryId = sportsId });

        // Returnal - Action, Shooter
        gameCategories.Add(new GameCategory { GameId = gameList[16].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[16].GameId, CategoryId = shooterId });

        // Hogwarts Legacy - Adventure, RPG
        gameCategories.Add(new GameCategory { GameId = gameList[17].GameId, CategoryId = adventureId });
        gameCategories.Add(new GameCategory { GameId = gameList[17].GameId, CategoryId = rpgId });

        // Assassin's Creed Valhalla - Action, Adventure
        gameCategories.Add(new GameCategory { GameId = gameList[18].GameId, CategoryId = actionId });
        gameCategories.Add(new GameCategory { GameId = gameList[18].GameId, CategoryId = adventureId });

        // Remove manual categorization of fixed games (it's in the first block or done)
        // ... (Skipping manual mapping lines which were inside the first if block initially)
        


        context.GameCategories.AddRange(gameCategories);

        // Seed GameSubscriptions (map games to subscription tiers)
        var gameSubscriptions = new List<GameSubscription>();
        
        // Essential tier games (first 5 games)
        for (int i = 0; i < 5 && i < gameList.Count; i++)
        {
            gameSubscriptions.Add(new GameSubscription { GameId = gameList[i].GameId, SubscriptionId = essentialId });
        }

        // Extra tier games (first 12 games) - includes Essential games
        for (int i = 0; i < 12 && i < gameList.Count; i++)
        {
            if (!gameSubscriptions.Any(gs => gs.GameId == gameList[i].GameId && gs.SubscriptionId == extraId))
            {
                gameSubscriptions.Add(new GameSubscription { GameId = gameList[i].GameId, SubscriptionId = extraId });
            }
        }

        // Premium tier games (all games)
        foreach (var game in gameList)
        {
            if (!gameSubscriptions.Any(gs => gs.GameId == game.GameId && gs.SubscriptionId == premiumId))
            {
                gameSubscriptions.Add(new GameSubscription { GameId = game.GameId, SubscriptionId = premiumId });
            }
        } 


        context.GameSubscriptions.AddRange(gameSubscriptions);

    } // End of if (!isDataSeeded)

        // --- EXTENDED SEEDING: Check for Random Data Expansion ---
        // Runs even if Regions exist, to fill up games if needed.
        if (await context.Games.CountAsync() < 30)
        {
             Console.WriteLine("⚡ Expanding Seed Data with Random Games & Users...");
             
             // Fetch IDs needed for relationships
             var essentialId = await context.SubscriptionPlans.Where(s => s.SubscriptionType == "Essential").Select(s => s.SubscriptionId).FirstOrDefaultAsync();
             var extraId = await context.SubscriptionPlans.Where(s => s.SubscriptionType == "Extra").Select(s => s.SubscriptionId).FirstOrDefaultAsync();
             var premiumId = await context.SubscriptionPlans.Where(s => s.SubscriptionType == "Premium").Select(s => s.SubscriptionId).FirstOrDefaultAsync();
             
             var indiaId = await context.Countries.Where(c => c.CountryCode == "IN").Select(c => c.CountryId).FirstOrDefaultAsync();
             var ukId = await context.Countries.Where(c => c.CountryCode == "GB").Select(c => c.CountryId).FirstOrDefaultAsync();
             
             var allCatIds = await context.Categories.Select(c => c.CategoryId).ToListAsync();
             var countriesIds = new[] { indiaId, ukId };

            // 1. Generate Random Games
             var gameFaker = new Faker<Game>()
                .RuleFor(g => g.GameId, f => Guid.NewGuid())
                .RuleFor(g => g.GameName, f => $"{f.Commerce.ProductAdjective()} {f.Hacker.Noun()}") 
                .RuleFor(g => g.PublishedBy, f => f.Company.CompanyName())
                .RuleFor(g => g.ReleaseDate, f => f.Date.Past(5))
                .RuleFor(g => g.FreeToPlay, f => f.Random.Bool(0.1f))
                .RuleFor(g => g.BasePrice, (f, g) => g.FreeToPlay ? null : decimal.Parse(f.Commerce.Price(1000, 5000)))
                .RuleFor(g => g.IsMultiplayer, f => f.Random.Bool());

            var randomGames = gameFaker.Generate(30);
            randomGames.ForEach(g => g.GameName = g.GameName + " " + g.GameId.ToString().Substring(0, 4));
            
            context.Games.AddRange(randomGames);
            await context.SaveChangesAsync();

            // 2. Pricing for Random Games
            var gameCountries = new List<GameCountry>();
            foreach (var game in randomGames)
            {
                if (game.BasePrice.HasValue)
                {
                    gameCountries.Add(new GameCountry { GameCountryId = Guid.NewGuid(), GameId = game.GameId, CountryId = indiaId, Price = game.BasePrice.Value });
                    gameCountries.Add(new GameCountry { GameCountryId = Guid.NewGuid(), GameId = game.GameId, CountryId = ukId, Price = Math.Round(game.BasePrice.Value / 100m, 2) });
                }
            }
            context.GameCountries.AddRange(gameCountries);

            // 3. Categories for Random Games
            var gameCategories = new List<GameCategory>();
            if (allCatIds.Any())
            {
                 foreach (var randomGame in randomGames)
                {
                    var randomCatIds = allCatIds.OrderBy(x => Guid.NewGuid()).Take(new Random().Next(1, 3)).ToList();
                    foreach (var catId in randomCatIds)
                    {
                        gameCategories.Add(new GameCategory { GameId = randomGame.GameId, CategoryId = catId });
                    }
                }
                context.GameCategories.AddRange(gameCategories);
            }

            // 4. Subscriptions for Random Games (Add all to Premium)
            var gameSubscriptions = new List<GameSubscription>();
            if (premiumId != Guid.Empty)
            {
                foreach (var rGame in randomGames)
                {
                    gameSubscriptions.Add(new GameSubscription { GameId = rGame.GameId, SubscriptionId = premiumId });
                }
                context.GameSubscriptions.AddRange(gameSubscriptions);
            }

            // 5. Random Users
            var defaultHash = BCrypt.Net.BCrypt.HashPassword("Pass@123", workFactor: 12);
            var userFaker = new Faker<User>()
                .RuleFor(u => u.UserId, f => Guid.NewGuid())
                .RuleFor(u => u.UserName, f => f.Internet.UserName())
                .RuleFor(u => u.UserEmail, (f, u) => f.Internet.Email(u.UserName))
                .RuleFor(u => u.UserPassword, defaultHash)
                .RuleFor(u => u.Age, f => f.Random.Int(18, 60))
                .RuleFor(u => u.CountryId, f => f.PickRandom(countriesIds))
                .RuleFor(u => u.CreatedAt, f => f.Date.Past(2));

            var randomUsers = userFaker.Generate(25);
            context.Users.AddRange(randomUsers);
            
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ Seeded {randomGames.Count} extra games and {randomUsers.Count} extra users.");
        }
            


        // Seed Default Admin (Runs independently of other data)
        if (!await context.Admins.AnyAsync())
        {
            var adminUser = new Admin
            {
                AdminId = Guid.NewGuid(),
                AdminEmail = "admin@store.com",
                AdminPassword = BCrypt.Net.BCrypt.HashPassword("Admin@123", workFactor: 12),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            context.Admins.Add(adminUser);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Default Admin seeded: admin@store.com / Admin@123");
        }
    }
}
