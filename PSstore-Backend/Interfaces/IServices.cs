using PSstore.DTOs;

namespace PSstore.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string userName, string role);
        int? ValidateToken(string token);
    }

    public interface IGameService
    {
        Task<GameDTO?> GetGameByIdAsync(int gameId);
        Task<IEnumerable<GameDTO>> GetAllGamesAsync(bool includeDeleted = false);
        Task<IEnumerable<GameDTO>> SearchGamesAsync(string searchTerm);
        Task<IEnumerable<GameDTO>> GetGamesByCategoryAsync(int categoryId);
        Task<IEnumerable<GameDTO>> GetFreeToPlayGamesAsync();
        Task<GameWithAccessDTO?> GetGameWithAccessAsync(int gameId, int userId);
        Task<GameDTO> CreateGameAsync(CreateGameDTO createGameDTO);
        Task<GameDTO?> UpdateGameAsync(int gameId, UpdateGameDTO updateGameDTO);
        Task<bool> SoftDeleteGameAsync(int gameId);
        Task<bool> RestoreGameAsync(int gameId);
    }

    public interface IPurchaseService
    {
        Task<PurchaseResponseDTO> PurchaseGameAsync(int userId, CreatePurchaseDTO purchaseDTO);
        Task<IEnumerable<PurchaseHistoryDTO>> GetUserPurchaseHistoryAsync(int userId);
        Task<bool> HasUserPurchasedGameAsync(int userId, int gameId);
        Task<PurchaseResponseDTO?> GetPurchaseDetailsAsync(int purchaseId);
    }

    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDTO> SubscribeAsync(int userId, CreateSubscriptionDTO subscriptionDTO);
        Task<UserSubscriptionDTO?> GetActiveSubscriptionAsync(int userId);
        Task<IEnumerable<UserSubscriptionDTO>> GetUserSubscriptionHistoryAsync(int userId);
        Task<IEnumerable<SubscriptionPlanDTO>> GetAllSubscriptionPlansAsync();
        Task<IEnumerable<SubscriptionPlanCountryDTO>> GetSubscriptionPlanOptionsAsync(int subscriptionId, int countryId);
        Task<bool> CancelSubscriptionAsync(int userId);
    }

    public interface ICartService
    {
        Task<CartDTO?> GetUserCartAsync(int userId);
        Task<CartItemDTO> AddItemToCartAsync(int userId, CreateCartItemDTO cartItemDTO);
        Task<bool> RemoveItemFromCartAsync(int userId, int cartItemId);
        Task<bool> UpdateCartItemQuantityAsync(int userId, int cartItemId, int quantity);
        Task<bool> ClearCartAsync(int userId);
        Task<CheckoutResultDTO> CheckoutAsync(int userId);
    }

    public interface IUserService
    {
        Task<UserDTO?> GetUserByIdAsync(int userId);
        Task<UserDTO?> GetUserByEmailAsync(string email);
        Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDTO);
        Task<UserDTO?> UpdateUserAsync(int userId, UpdateUserDTO updateUserDTO);
        Task<bool> SoftDeleteUserAsync(int userId);
        Task<bool> RestoreUserAsync(int userId);
        Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDTO);
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    }

    public interface ICategoryService
    {
        Task<CategoryDTO?> GetCategoryByIdAsync(int categoryId);
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(bool includeDeleted = false);
        Task<CategoryDTO> CreateCategoryAsync(CreateCategoryDTO createCategoryDTO);
        Task<CategoryDTO?> UpdateCategoryAsync(int categoryId, UpdateCategoryDTO updateCategoryDTO);
        Task<bool> SoftDeleteCategoryAsync(int categoryId);
        Task<bool> RestoreCategory(int categoryId);
    }
}
