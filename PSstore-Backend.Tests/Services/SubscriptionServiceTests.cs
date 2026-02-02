using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class SubscriptionServiceTests : IntegrationTestBase
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _subscriptionService = new SubscriptionService(
                UserSubscriptionPlanRepository,
                SubscriptionPlanRepository,
                SubscriptionPlanCountryRepository,
                UserRepository,
                CountryRepository,
                GameSubscriptionRepository
            );
        }

        [Fact]
        public async Task SubscribeAsync_ShouldCreateSubscription_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var planCountryId = Guid.NewGuid();

            var user = new User 
            { 
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = countryId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                SubscriptionId = subscriptionId,
                SubscriptionType = "Premium"
            };

            var planCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = planCountryId,
                SubscriptionId = subscriptionId,
                CountryId = countryId,
                DurationMonths = 12,
                Price = 59.99m
            };

            await Context.Users.AddAsync(user);
            await Context.SubscriptionPlans.AddAsync(subscriptionPlan);
            await Context.SubscriptionPlanCountries.AddAsync(planCountry);
            await Context.SaveChangesAsync();

            var subDTO = new CreateSubscriptionDTO { SubscriptionPlanCountryId = planCountryId };

            // Act
            var result = await _subscriptionService.SubscribeAsync(userId, subDTO);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Subscription activated successfully!");
            result.DurationMonths.Should().Be(12);
            result.Price.Should().Be(59.99m);

            var savedSubscription = await Context.UserSubscriptionPlans
                .FirstOrDefaultAsync(s => s.UserId == userId);
            savedSubscription.Should().NotBeNull();
            savedSubscription!.SubscriptionPlanCountryId.Should().Be(planCountryId);
        }

        [Fact]
        public async Task SubscribeAsync_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subDTO = new CreateSubscriptionDTO { SubscriptionPlanCountryId = Guid.NewGuid() };

            // Act
            var result = await _subscriptionService.SubscribeAsync(userId, subDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("User not found.");
        }

        [Fact]
        public async Task SubscribeAsync_ShouldFail_WhenActiveSubscriptionExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = countryId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var existingPlanCountryId = Guid.NewGuid();
            var existingSubId = Guid.NewGuid();
            
            var existingSubPlan = new SubscriptionPlan
            {
                SubscriptionId = existingSubId,
                SubscriptionType = "Existing"
            };
            
            var existingPlanCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = existingPlanCountryId,
                SubscriptionId = existingSubId,
                CountryId = countryId,
                DurationMonths = 12,
                Price = 29.99m
            };
            
            var activeSub = new UserSubscriptionPlan
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanCountryId = existingPlanCountryId,
                PlanStartDate = DateTime.UtcNow.AddMonths(-1),
                PlanEndDate = DateTime.UtcNow.AddMonths(11)
            };

            await Context.Users.AddAsync(user);
            await Context.SubscriptionPlans.AddAsync(existingSubPlan);
            await Context.SubscriptionPlanCountries.AddAsync(existingPlanCountry);
            await Context.UserSubscriptionPlans.AddAsync(activeSub);
            await Context.SaveChangesAsync();

            var subDTO = new CreateSubscriptionDTO { SubscriptionPlanCountryId = Guid.NewGuid() };

            // Act
            var result = await _subscriptionService.SubscribeAsync(userId, subDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("active subscription that expires");
        }

        [Fact]
        public async Task SubscribeAsync_ShouldFail_WhenPlanCountryNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            var subDTO = new CreateSubscriptionDTO { SubscriptionPlanCountryId = Guid.NewGuid() };

            // Act
            var result = await _subscriptionService.SubscribeAsync(userId, subDTO);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Subscription plan not found.");
        }

        [Fact]
        public async Task GetActiveSubscriptionAsync_ShouldReturnSubscription_WhenActive()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var planCountryId = Guid.NewGuid();

            var subscriptionPlan = new SubscriptionPlan
            {
                SubscriptionId = subscriptionId,
                SubscriptionType = "Premium"
            };

            var planCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = planCountryId,
                SubscriptionId = subscriptionId,
                CountryId = Guid.NewGuid(),
                DurationMonths = 12,
                Price = 59.99m
            };

            var activeSub = new UserSubscriptionPlan
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanCountryId = planCountryId,
                PlanStartDate = DateTime.UtcNow.AddMonths(-1),
                PlanEndDate = DateTime.UtcNow.AddMonths(11)
            };

            await Context.SubscriptionPlans.AddAsync(subscriptionPlan);
            await Context.SubscriptionPlanCountries.AddAsync(planCountry);
            await Context.UserSubscriptionPlans.AddAsync(activeSub);
            await Context.SaveChangesAsync();

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeTrue();
            result.SubscriptionName.Should().Be("Premium");
        }

        [Fact]
        public async Task GetActiveSubscriptionAsync_ShouldReturnNull_WhenNoActiveSubscription()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ShouldCancel_WhenActiveExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subId = Guid.NewGuid();
            var planCountryId = Guid.NewGuid();
            
            var subPlan = new SubscriptionPlan
            {
                SubscriptionId = subId,
                SubscriptionType = "Premium"
            };
            
            var planCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = planCountryId,
                SubscriptionId = subId,
                CountryId = Guid.NewGuid(),
                DurationMonths = 12,
                Price = 59.99m
            };
            
            var activeSub = new UserSubscriptionPlan
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                SubscriptionPlanCountryId = planCountryId,
                PlanStartDate = DateTime.UtcNow.AddMonths(-1),
                PlanEndDate = DateTime.UtcNow.AddMonths(11)
            };

            await Context.SubscriptionPlans.AddAsync(subPlan);
            await Context.SubscriptionPlanCountries.AddAsync(planCountry);
            await Context.UserSubscriptionPlans.AddAsync(activeSub);
            await Context.SaveChangesAsync();

            // Act
            var result = await _subscriptionService.CancelSubscriptionAsync(userId);

            // Assert
            result.Should().BeTrue();

            // Verify end date was updated to roughly now (within 1 second)
            var updated = await Context.UserSubscriptionPlans.FindAsync(activeSub.UserSubscriptionId);
            updated!.PlanEndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ShouldReturnFalse_WhenNoActiveSubscription()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _subscriptionService.CancelSubscriptionAsync(userId);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllSubscriptionPlansAsync_ShouldReturnPlans()
        {
            // Arrange
            var plans = new List<SubscriptionPlan>
            {
                new SubscriptionPlan { SubscriptionId = Guid.NewGuid(), SubscriptionType = "Essential" },
                new SubscriptionPlan { SubscriptionId = Guid.NewGuid(), SubscriptionType = "Extra" }
            };

            await Context.SubscriptionPlans.AddRangeAsync(plans);
            await Context.SaveChangesAsync();

            // Act
            var result = await _subscriptionService.GetAllSubscriptionPlansAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Select(p => p.SubscriptionName).Should().Contain("Essential");
            result.Select(p => p.SubscriptionName).Should().Contain("Extra");
        }

        [Fact]
        public async Task GetSubscriptionPlanOptionsAsync_ShouldReturnFilteredOptions()
        {
            // Arrange
            var subId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            var otherSubId = Guid.NewGuid();
            
            var subPlan = new SubscriptionPlan
            {
                SubscriptionId = subId,
                SubscriptionType = "Premium"
            };
            
            var otherSubPlan = new SubscriptionPlan
            {
                SubscriptionId = otherSubId,
                SubscriptionType = "Basic"
            };

            var options = new List<SubscriptionPlanCountry>
            {
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = subId, CountryId = countryId, DurationMonths = 1, Price = 10 },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = subId, CountryId = countryId, DurationMonths = 12, Price = 100 },
                new SubscriptionPlanCountry { SubscriptionPlanCountryId = Guid.NewGuid(), SubscriptionId = otherSubId, CountryId = countryId, DurationMonths = 1, Price = 30 } // Different sub
            };

            await Context.SubscriptionPlans.AddAsync(subPlan);
            await Context.SubscriptionPlans.AddAsync(otherSubPlan);
            await Context.SubscriptionPlanCountries.AddRangeAsync(options);
            await Context.SaveChangesAsync();

            // Act
            var result = await _subscriptionService.GetSubscriptionPlanOptionsAsync(subId, countryId);

            // Assert
            result.Should().HaveCount(2);
            result.All(o => o.SubscriptionId == subId).Should().BeTrue();
        }

        [Fact]
        public async Task GetUserSubscriptionHistoryAsync_ShouldReturnHistory()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subscriptionId = Guid.NewGuid();
            var planCountryId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            var regionId = Guid.NewGuid();

            var region = new Region
            {
                RegionId = regionId,
                RegionName = "Test Region"
            };

            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = countryId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var country = new Country
            {
                CountryId = countryId,
                CountryCode = "US",
                CountryName = "Test Country",
                Currency = "USD",
                RegionId = regionId
            };

            var subscriptionPlan = new SubscriptionPlan
            {
                SubscriptionId = subscriptionId,
                SubscriptionType = "Premium"
            };

            var planCountry = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = planCountryId,
                SubscriptionId = subscriptionId,
                CountryId = countryId,
                DurationMonths = 12,
                Price = 59.99m
            };

            var history = new List<UserSubscriptionPlan>
            {
                new UserSubscriptionPlan
                {
                    UserSubscriptionId = Guid.NewGuid(),
                    UserId = userId,
                    SubscriptionPlanCountryId = planCountryId,
                    PlanStartDate = DateTime.UtcNow.AddYears(-1),
                    PlanEndDate = DateTime.UtcNow.AddMonths(-6)
                },
                new UserSubscriptionPlan
                {
                    UserSubscriptionId = Guid.NewGuid(),
                    UserId = userId,
                    SubscriptionPlanCountryId = planCountryId,
                    PlanStartDate = DateTime.UtcNow.AddMonths(-1),
                    PlanEndDate = DateTime.UtcNow.AddMonths(11) // Active
                }
            };

            await Context.Regions.AddAsync(region);
            await Context.Users.AddAsync(user);
            await Context.Countries.AddAsync(country);
            await Context.SubscriptionPlans.AddAsync(subscriptionPlan);
            await Context.SubscriptionPlanCountries.AddAsync(planCountry);
            await Context.UserSubscriptionPlans.AddRangeAsync(history);
            await Context.SaveChangesAsync();

            // Act
            var result = await _subscriptionService.GetUserSubscriptionHistoryAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Count(s => s.IsActive).Should().Be(1);
        }
    }
}
