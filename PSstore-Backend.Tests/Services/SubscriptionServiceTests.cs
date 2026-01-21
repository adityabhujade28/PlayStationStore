using Bogus;
using FluentAssertions;
using Moq;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IUserSubscriptionPlanRepository> _mockUserSubRepo;
        private readonly Mock<ISubscriptionPlanRepository> _mockSubPlanRepo;
        private readonly Mock<ISubscriptionPlanCountryRepository> _mockPlanCountryRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ICountryRepository> _mockCountryRepo;
        private readonly Mock<IGameSubscriptionRepository> _mockGameSubRepo;
        private readonly SubscriptionService _subscriptionService;
        private readonly Faker _faker;

        public SubscriptionServiceTests()
        {
            _mockUserSubRepo = new Mock<IUserSubscriptionPlanRepository>();
            _mockSubPlanRepo = new Mock<ISubscriptionPlanRepository>();
            _mockPlanCountryRepo = new Mock<ISubscriptionPlanCountryRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _mockCountryRepo = new Mock<ICountryRepository>();
            _mockGameSubRepo = new Mock<IGameSubscriptionRepository>();

            _subscriptionService = new SubscriptionService(
                _mockUserSubRepo.Object,
                _mockSubPlanRepo.Object,
                _mockPlanCountryRepo.Object,
                _mockUserRepo.Object,
                _mockCountryRepo.Object,
                _mockGameSubRepo.Object
            );

            _faker = new Faker();
        }

        [Fact]
        public async Task SubscribeAsync_ShouldCreateSubscription_WhenRequestIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planCountryId = Guid.NewGuid();
            var subDTO = new CreateSubscriptionDTO { SubscriptionPlanCountryId = planCountryId };

            var user = new User { UserId = userId };
            var planCountry = new SubscriptionPlanCountry 
            { 
                SubscriptionPlanCountryId = planCountryId,
                DurationMonths = 12,
                Price = 59.99m
            };

            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync((UserSubscriptionPlan?)null);
            _mockPlanCountryRepo.Setup(r => r.GetByIdAsync(planCountryId)).ReturnsAsync(planCountry);

            // Act
            var result = await _subscriptionService.SubscribeAsync(userId, subDTO);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Subscription activated successfully!");
            result.DurationMonths.Should().Be(12);
            result.Price.Should().Be(59.99m);

            _mockUserSubRepo.Verify(r => r.AddAsync(It.Is<UserSubscriptionPlan>(s => 
                s.UserId == userId && 
                s.SubscriptionPlanCountryId == planCountryId &&
                s.PlanEndDate > s.PlanStartDate
            )), Times.Once);
            _mockUserSubRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task SubscribeAsync_ShouldFail_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var subDTO = new CreateSubscriptionDTO();
            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync((User?)null);

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
            var subDTO = new CreateSubscriptionDTO();
            var user = new User { UserId = userId };
            var activeSub = new UserSubscriptionPlan 
            { 
                UserId = userId, 
                PlanEndDate = DateTime.UtcNow.AddMonths(1) 
            };

            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync(activeSub);

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
            var subDTO = new CreateSubscriptionDTO { SubscriptionPlanCountryId = Guid.NewGuid() };
            var user = new User { UserId = userId };

            _mockUserRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync((UserSubscriptionPlan?)null);
            _mockPlanCountryRepo.Setup(r => r.GetByIdAsync(subDTO.SubscriptionPlanCountryId)).ReturnsAsync((SubscriptionPlanCountry?)null);

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
            var activeSub = new UserSubscriptionPlan
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                PlanStartDate = DateTime.UtcNow.AddMonths(-1),
                PlanEndDate = DateTime.UtcNow.AddMonths(11),
                SubscriptionPlanCountry = new SubscriptionPlanCountry
                {
                    SubscriptionPlan = new SubscriptionPlan { SubscriptionType = "Premium" }
                }
            };

            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync(activeSub);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.IsActive.Should().BeTrue();
            result.SubscriptionName.Should().Be("Premium");
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ShouldCancel_WhenActiveExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var activeSub = new UserSubscriptionPlan
            {
                UserSubscriptionId = Guid.NewGuid(),
                UserId = userId,
                PlanEndDate = DateTime.UtcNow.AddMonths(6)
            };

            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync(activeSub);

            // Act
            var result = await _subscriptionService.CancelSubscriptionAsync(userId);

            // Assert
            result.Should().BeTrue();
            
            // Verify end date was updated to roughly now (within 1 second)
            activeSub.PlanEndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            
            _mockUserSubRepo.Verify(r => r.Update(activeSub), Times.Once);
            _mockUserSubRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_ShouldReturnFalse_WhenNoActiveSubscription()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync((UserSubscriptionPlan?)null);

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

            _mockSubPlanRepo.Setup(r => r.GetAllPlansWithDetailsAsync()).ReturnsAsync(plans);

            // Act
            var result = await _subscriptionService.GetAllSubscriptionPlansAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Select(p => p.SubscriptionName).Should().Contain("Essential");
        }

        [Fact]
        public async Task GetSubscriptionPlanOptionsAsync_ShouldReturnFilteredOptions()
        {
            // Arrange
            var subId = Guid.NewGuid();
            var countryId = Guid.NewGuid();
            
            var allOptions = new List<SubscriptionPlanCountry>
            {
                new SubscriptionPlanCountry { SubscriptionId = subId, CountryId = countryId, Price = 10 },
                new SubscriptionPlanCountry { SubscriptionId = subId, CountryId = countryId, Price = 20 },
                new SubscriptionPlanCountry { SubscriptionId = Guid.NewGuid(), CountryId = countryId, Price = 30 } // Different sub
            };

            _mockPlanCountryRepo.Setup(r => r.GetPlansByCountryAsync(countryId)).ReturnsAsync(allOptions);

            // Act
            var result = await _subscriptionService.GetSubscriptionPlanOptionsAsync(subId, countryId);

            // Assert
            result.Should().HaveCount(2);
            result.All(o => o.SubscriptionId == subId).Should().BeTrue();
        }
        [Fact]
        public async Task GetActiveSubscriptionAsync_ShouldReturnNull_WhenNoActiveSubscription()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserSubRepo.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync((UserSubscriptionPlan?)null);

            // Act
            var result = await _subscriptionService.GetActiveSubscriptionAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserSubscriptionHistoryAsync_ShouldReturnHistory()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var history = new List<UserSubscriptionPlan>
            {
                new UserSubscriptionPlan 
                { 
                    UserSubscriptionId = Guid.NewGuid(),
                    UserId = userId,
                    PlanStartDate = DateTime.UtcNow.AddYears(-1),
                    PlanEndDate = DateTime.UtcNow.AddMonths(-6)
                },
                new UserSubscriptionPlan 
                { 
                    UserSubscriptionId = Guid.NewGuid(),
                    UserId = userId,
                    PlanStartDate = DateTime.UtcNow.AddMonths(-1),
                    PlanEndDate = DateTime.UtcNow.AddMonths(1) // Active
                }
            };

            _mockUserSubRepo.Setup(r => r.GetUserSubscriptionsAsync(userId)).ReturnsAsync(history);

            // Act
            var result = await _subscriptionService.GetUserSubscriptionHistoryAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.Count(s => s.IsActive).Should().Be(1);
        }
    }
}
