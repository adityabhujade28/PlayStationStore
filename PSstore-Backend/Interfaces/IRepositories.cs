using PSstore.Models;

namespace PSstore.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserWithRegionAsync(int userId);
        Task<User?> GetUserWithPurchasesAsync(int userId);
        Task<User?> GetUserWithSubscriptionsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task SoftDeleteAsync(int userId);
        Task<bool> RestoreAsync(int userId);
    }

    public interface IGameRepository : IRepository<Game>
    {
        Task<Game?> GetGameWithCategoriesAsync(int gameId);
        Task<Game?> GetGameWithSubscriptionsAsync(int gameId);
        Task<IEnumerable<Game>> GetGamesByCategoryAsync(int categoryId);
        Task<IEnumerable<Game>> GetFreeGamesAsync();
        Task<IEnumerable<Game>> SearchGamesAsync(string searchTerm);
        Task SoftDeleteAsync(int gameId);
        Task<bool> RestoreAsync(int gameId);
        Task<IEnumerable<Game>> GetAllIncludingDeletedAsync();
    }

    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetCategoryWithGamesAsync(int categoryId);
        Task<bool> CategoryNameExistsAsync(string categoryName);
        Task SoftDeleteAsync(int categoryId);
        Task<bool> RestoreAsync(int categoryId);
    }

    public interface IRegionRepository : IRepository<Region>
    {
        Task<Region?> GetByCodeAsync(string regionCode);
    }

    public interface IUserPurchaseGameRepository : IRepository<UserPurchaseGame>
    {
        Task<IEnumerable<UserPurchaseGame>> GetUserPurchasesAsync(int userId);
        Task<UserPurchaseGame?> GetPurchaseDetailsAsync(int purchaseId);
        Task<bool> HasUserPurchasedGameAsync(int userId, int gameId);
    }

    public interface ISubscriptionPlanRepository : IRepository<SubscriptionPlan>
    {
        Task<SubscriptionPlan?> GetPlanWithRegionsAsync(int planId);
        Task<SubscriptionPlan?> GetPlanWithGamesAsync(int planId);
        Task<IEnumerable<SubscriptionPlan>> GetAllPlansWithDetailsAsync();
    }

    public interface ISubscriptionPlanRegionRepository : IRepository<SubscriptionPlanRegion>
    {
        Task<SubscriptionPlanRegion?> GetPlanRegionDetailsAsync(int planRegionId);
        Task<IEnumerable<SubscriptionPlanRegion>> GetPlansByRegionAsync(int regionId);
    }

    public interface IUserSubscriptionPlanRepository : IRepository<UserSubscriptionPlan>
    {
        Task<IEnumerable<UserSubscriptionPlan>> GetUserSubscriptionsAsync(int userId);
        Task<UserSubscriptionPlan?> GetActiveSubscriptionAsync(int userId);
        Task<bool> HasActiveSubscriptionAsync(int userId);
        Task<IEnumerable<UserSubscriptionPlan>> GetExpiredSubscriptionsAsync();
    }

    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetUserCartAsync(int userId);
        Task<Cart?> GetCartWithItemsAsync(int cartId);
        Task ClearCartAsync(int cartId);
    }

    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<IEnumerable<CartItem>> GetCartItemsAsync(int cartId);
        Task<CartItem?> GetCartItemWithGameAsync(int cartItemId);
        Task<bool> IsGameInCartAsync(int cartId, int gameId);
    }

    public interface IAdminRepository : IRepository<Admin>
    {
        Task<Admin?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
        Task SoftDeleteAsync(int adminId);
        Task<bool> RestoreAsync(int adminId);
    }
}
