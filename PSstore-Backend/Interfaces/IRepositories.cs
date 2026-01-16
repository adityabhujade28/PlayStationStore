using PSstore.Models;

namespace PSstore.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithRegionAsync(Guid userId);
        Task<User?> GetUserWithPurchasesAsync(Guid userId);
        Task<User?> GetUserWithSubscriptionsAsync(Guid userId);
        Task<bool> EmailExistsAsync(string email);
        Task SoftDeleteAsync(Guid userId);
        Task<bool> RestoreAsync(Guid userId);
    }

    public interface IGameRepository : IRepository<Game>
    {
        Task<Game?> GetGameWithCategoriesAsync(Guid gameId);
        Task<Game?> GetGameWithSubscriptionsAsync(Guid gameId);
        Task<IEnumerable<Game>> GetGamesByCategoryAsync(Guid categoryId);
        Task<IEnumerable<Game>> GetFreeGamesAsync();
        Task<IEnumerable<Game>> SearchGamesAsync(string searchTerm);
        Task SoftDeleteAsync(Guid gameId);
        Task<bool> RestoreAsync(Guid gameId);
        Task<IEnumerable<Game>> GetAllIncludingDeletedAsync();
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithGamesAsync(Guid categoryId);
        Task<bool> CategoryNameExistsAsync(string categoryName);
        Task SoftDeleteAsync(Guid categoryId);
        Task<bool> RestoreAsync(Guid categoryId);
    }

    public interface IRegionRepository : IRepository<Region>
    {
        Task<Region?> GetByCodeAsync(string regionCode);
    }

    public interface ICountryRepository : IRepository<Country>
    {
        Task<Country?> GetByCodeAsync(string countryCode);
        Task<IEnumerable<Country>> GetCountriesByRegionAsync(Guid regionId);
    }

    public interface IGameCountryRepository : IRepository<GameCountry>
    {
        Task<GameCountry?> GetGamePricingAsync(Guid gameId, Guid countryId);
        Task<IEnumerable<GameCountry>> GetGamePricesByCountryAsync(Guid countryId);
    }

    public interface IUserPurchaseGameRepository : IRepository<UserPurchaseGame>
    {
        Task<IEnumerable<UserPurchaseGame>> GetUserPurchasesAsync(Guid userId);
        Task<UserPurchaseGame?> GetPurchaseDetailsAsync(Guid purchaseId);
        Task<bool> HasUserPurchasedGameAsync(Guid userId, Guid gameId);
    }

    public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
    {
        Task<SubscriptionPlan?> GetPlanWithRegionsAsync(Guid planId);
        Task<SubscriptionPlan?> GetPlanWithGamesAsync(Guid planId);
        Task<IEnumerable<SubscriptionPlan>> GetAllPlansWithDetailsAsync();
    }

    public interface ISubscriptionPlanCountryRepository : IRepository<SubscriptionPlanCountry>
    {
        Task<SubscriptionPlanCountry?> GetPlanCountryDetailsAsync(Guid planCountryId);
        Task<IEnumerable<SubscriptionPlanCountry>> GetPlansByCountryAsync(Guid countryId);
    }

    public interface IUserSubscriptionPlanRepository : IRepository<UserSubscriptionPlan>
    {
        Task<IEnumerable<UserSubscriptionPlan>> GetUserSubscriptionsAsync(Guid userId);
        Task<UserSubscriptionPlan?> GetActiveSubscriptionAsync(Guid userId);
        Task<bool> HasActiveSubscriptionAsync(Guid userId);
        Task<IEnumerable<UserSubscriptionPlan>> GetExpiredSubscriptionsAsync();
    }

    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetUserCartAsync(Guid userId);
        Task<Cart?> GetCartWithItemsAsync(Guid cartId);
        Task ClearCartAsync(Guid cartId);
    }

    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid cartId);
        Task<CartItem?> GetCartItemWithGameAsync(Guid cartItemId);
        Task<bool> IsGameInCartAsync(Guid cartId, Guid gameId);
    }

    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task SoftDeleteAsync(Guid adminId);
        Task<bool> RestoreAsync(Guid adminId);
    }
}
