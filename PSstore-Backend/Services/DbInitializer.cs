using PSstore.Data;
using PSstore.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace PSstore.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context)
    {
        // Check if database is already seeded
        if (await context.Regions.AnyAsync())
        {
            return; // Database already seeded
        }

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

        // Seed GameCountries (pricing for each game in each country)
        var gameCountries = new List<GameCountry>();
        foreach (var game in games)
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
        var gameCategories = new List<GameCategory>();
        var gameList = games.ToList();

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

        // Minecraft - Adventure, Platformer
        gameCategories.Add(new GameCategory { GameId = gameList[19].GameId, CategoryId = adventureId });
        gameCategories.Add(new GameCategory { GameId = gameList[19].GameId, CategoryId = platformerId });

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

        // Seed Test Users (5 users with known password: Pass@123)
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Pass@123", workFactor: 12);
        var testUsers = new[]
        {
            new User
            {
                UserId = Guid.NewGuid(),
                UserName = "John Doe",
                UserEmail = "john@example.com",
                UserPassword = hashedPassword,
                Age = 25,
                CountryId = indiaId,
                SubscriptionStatus = null,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = Guid.NewGuid(),
                UserName = "Jane Smith",
                UserEmail = "jane@example.com",
                UserPassword = hashedPassword,
                Age = 30,
                CountryId = ukId,
                SubscriptionStatus = null,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = Guid.NewGuid(),
                UserName = "Raj Kumar",
                UserEmail = "raj@example.com",
                UserPassword = hashedPassword,
                Age = 28,
                CountryId = indiaId,
                SubscriptionStatus = null,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = Guid.NewGuid(),
                UserName = "Emma Wilson",
                UserEmail = "emma@example.com",
                UserPassword = hashedPassword,
                Age = 22,
                CountryId = ukId,
                SubscriptionStatus = null,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                UserId = Guid.NewGuid(),
                UserName = "Priya Sharma",
                UserEmail = "priya@example.com",
                UserPassword = hashedPassword,
                Age = 27,
                CountryId = indiaId,
                SubscriptionStatus = null,
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Users.AddRange(testUsers);

        await context.SaveChangesAsync();

        Console.WriteLine("✅ Database seeded successfully with Guid-based data!");
        Console.WriteLine($"   - {regions.Length} Regions");
        Console.WriteLine($"   - {countries.Length} Countries");
        Console.WriteLine($"   - {categories.Length} Categories");
        Console.WriteLine($"   - {subscriptions.Length} Subscription Plans");
        Console.WriteLine($"   - {subscriptionPlanCountries.Count} Subscription Pricing Options");
        Console.WriteLine($"   - {games.Length} Games");
        Console.WriteLine($"   - {gameCountries.Count} Game Pricing Entries");
        Console.WriteLine($"   - {gameCategories.Count} Game-Category Mappings");
        Console.WriteLine($"   - {gameSubscriptions.Count} Game-Subscription Mappings");
        Console.WriteLine($"   - {testUsers.Length} Test Users (Password: Pass@123)");
    }
}
