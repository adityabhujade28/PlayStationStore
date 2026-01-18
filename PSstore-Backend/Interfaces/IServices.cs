using PSstore.DTOs;

namespace PSstore.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(Guid userId, string role);
        Guid? ValidateToken(string token);
    }

    public interface IGameService
    {
        Task<GameDTO?> GetGameByIdAsync(Guid gameId, Guid? userId = null);
        Task<IEnumerable<GameDTO>> GetAllGamesAsync(bool includeDeleted = false, Guid? userId = null);
        Task<IEnumerable<GameDTO>> SearchGamesAsync(string searchTerm, Guid? userId = null);
        Task<IEnumerable<GameDTO>> GetGamesByCategoryAsync(Guid categoryId, Guid? userId = null);
        Task<IEnumerable<GameDTO>> GetFreeToPlayGamesAsync(Guid? userId = null);
        Task<GameWithAccessDTO?> GetGameWithAccessAsync(Guid gameId, Guid userId);
        Task<GameDTO> CreateGameAsync(CreateGameDTO createGameDTO);
        Task<GameDTO?> UpdateGameAsync(Guid gameId, UpdateGameDTO updateGameDTO);
        Task<bool> SoftDeleteGameAsync(Guid gameId);
        Task<bool> RestoreGameAsync(Guid gameId);
    }

    public interface IPurchaseService
    {
        Task<PurchaseResponseDTO> PurchaseGameAsync(Guid userId, CreatePurchaseDTO purchaseDTO);
        Task<IEnumerable<PurchaseHistoryDTO>> GetUserPurchaseHistoryAsync(Guid userId);
        Task<bool> HasUserPurchasedGameAsync(Guid userId, Guid gameId);
        Task<PurchaseResponseDTO?> GetPurchaseDetailsAsync(Guid purchaseId);
    }

    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDTO> SubscribeAsync(Guid userId, CreateSubscriptionDTO subscriptionDTO);
        Task<UserSubscriptionDTO?> GetActiveSubscriptionAsync(Guid userId);
        Task<IEnumerable<UserSubscriptionDTO>> GetUserSubscriptionHistoryAsync(Guid userId);
        Task<IEnumerable<SubscriptionPlanDTO>> GetAllSubscriptionPlansAsync();
        Task<IEnumerable<SubscriptionPlanCountryDTO>> GetSubscriptionPlanOptionsAsync(Guid subscriptionId, Guid countryId);
        Task<bool> CancelSubscriptionAsync(Guid userId);
    }

    public interface ICartService
    {
        Task<CartDTO?> GetUserCartAsync(Guid userId);
        Task<CartItemDTO> AddItemToCartAsync(Guid userId, CreateCartItemDTO cartItemDTO);
        Task<bool> RemoveItemFromCartAsync(Guid userId, Guid cartItemId);
        Task<bool> UpdateCartItemQuantityAsync(Guid userId, Guid cartItemId, int quantity);
        Task<bool> ClearCartAsync(Guid userId);
        Task<CheckoutResultDTO> CheckoutAsync(Guid userId);
    }

    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(Guid userId);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<UserDTO?> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDTO);
        Task<bool> SoftDeleteUserAsync(Guid userId);
        Task<bool> RestoreUserAsync(Guid userId);
        Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDTO);
        Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    }

    public interface ICategoryService
    {
        Task<CategoryDTO?> GetCategoryByIdAsync(Guid categoryId);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(bool includeDeleted = false);
        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO createCategoryDTO);
        Task<CategoryDTO?> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDTO updateCategoryDTO);
        Task<bool> SoftDeleteCategoryAsync(Guid categoryId);
        Task<bool> RestoreCategory(Guid categoryId);
    }
}
