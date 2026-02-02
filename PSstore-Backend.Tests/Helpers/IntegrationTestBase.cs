using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using PSstore.Data;
using PSstore.Interfaces;
using PSstore.Repositories;

namespace PSstore_Backend.Tests.Helpers
{
    public abstract class IntegrationTestBase : IDisposable
    {
        // DbContext
        protected readonly AppDbContext Context;

        // Faker for test data generation
        protected readonly Faker Faker;

        // Real Repositories (using in-memory database)
        protected readonly UserRepository UserRepository;
        protected readonly GameRepository GameRepository;
        protected readonly CategoryRepository CategoryRepository;
        protected readonly RegionRepository RegionRepository;
        protected readonly CountryRepository CountryRepository;
        protected readonly UserPurchaseGameRepository UserPurchaseGameRepository;
        protected readonly SubscriptionPlanRepository SubscriptionPlanRepository;
        protected readonly SubscriptionPlanCountryRepository SubscriptionPlanCountryRepository;
        protected readonly UserSubscriptionPlanRepository UserSubscriptionPlanRepository;
        protected readonly CartRepository CartRepository;
        protected readonly CartItemRepository CartItemRepository;
        protected readonly AdminRepository AdminRepository;
        protected readonly GameCountryRepository GameCountryRepository;
        protected readonly GameSubscriptionRepository GameSubscriptionRepository;

        // Mocked External Dependencies
        protected readonly Mock<IJwtService> MockJwtService;
        protected readonly Mock<IConfiguration> MockConfiguration;

        protected IntegrationTestBase()
        {
            // Setup in-memory database with enhanced configuration
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging()  // Shows actual parameter values in logs for better debugging
                .UseInMemoryDatabase($"{GetType().Name}-InMemoryDb-{Guid.NewGuid()}")  // Descriptive name for easier identification
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))  // Suppress expected in-memory warnings
                .Options;

            Context = new AppDbContext(options);

            // Initialize all real repositories
            UserRepository = new UserRepository(Context);
            GameRepository = new GameRepository(Context);
            CategoryRepository = new CategoryRepository(Context);
            RegionRepository = new RegionRepository(Context);
            CountryRepository = new CountryRepository(Context);
            UserPurchaseGameRepository = new UserPurchaseGameRepository(Context);
            SubscriptionPlanRepository = new SubscriptionPlanRepository(Context);
            SubscriptionPlanCountryRepository = new SubscriptionPlanCountryRepository(Context);
            UserSubscriptionPlanRepository = new UserSubscriptionPlanRepository(Context);
            CartRepository = new CartRepository(Context);
            CartItemRepository = new CartItemRepository(Context);
            AdminRepository = new AdminRepository(Context);
            GameCountryRepository = new GameCountryRepository(Context);
            GameSubscriptionRepository = new GameSubscriptionRepository(Context);

            // Initialize Faker for test data generation
            Faker = new Faker();

            // Setup mocked external dependencies
            MockJwtService = new Mock<IJwtService>();
            MockConfiguration = new Mock<IConfiguration>();
            
            // Default configuration setup
            MockConfiguration.Setup(c => c["JwtSettings:ExpirationMinutes"]).Returns("60");
            MockConfiguration.Setup(c => c["JwtSettings:SecretKey"]).Returns("test-secret-key-for-testing-purposes-only");
        }

        public void Dispose()
        {
            Context.Database.EnsureDeleted();
            Context.Dispose();
        }
    }
}
