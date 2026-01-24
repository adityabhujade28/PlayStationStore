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
        
        Task<IEnumerable<GamePricingDTO>> GetGamePricingAsync(Guid gameId);
        Task<GamePricingDTO> UpdateGamePriceAsync(Guid gameCountryId, decimal newPrice);
        Task<GamePricingDTO> AddGamePriceAsync(CreateGamePricingDTO pricingDTO);

        // Pagination Methods
        /// <summary>
        /// Get paginated games with support for filtering, searching, and sorting
        /// </summary>
        Task<PagedResponse<GameDTO>> GetPagedGamesAsync(GamePaginationQuery query, Guid? userId = null);

        /// <summary>
        /// Get paginated games by category
        /// </summary>
        Task<PagedResponse<GameDTO>> GetPagedGamesByCategoryAsync(Guid categoryId, int pageNumber, int pageSize, Guid? userId = null);

        /// <summary>
        /// Get paginated search results
        /// </summary>
        Task<PagedResponse<GameDTO>> GetPagedSearchResultsAsync(string searchTerm, int pageNumber, int pageSize, Guid? userId = null);
    }

    public interface IPurchaseService
    {
        Task<PurchaseResponseDTO> PurchaseGameAsync(Guid userId, CreatePurchaseDTO purchaseDTO);
        Task<IEnumerable<PurchaseHistoryDTO>> GetUserPurchaseHistoryAsync(Guid userId);
        Task<bool> HasUserPurchasedGameAsync(Guid userId, Guid gameId);
        Task<PurchaseResponseDTO?> GetPurchaseDetailsAsync(Guid purchaseId);

        /// <summary>
        /// Get paginated purchase history for a user
        /// </summary>
        Task<PagedResponse<PurchaseHistoryDTO>> GetPagedUserPurchaseHistoryAsync(Guid userId, int pageNumber, int pageSize);
    }

    public interface ISubscriptionService
    {
        Task<SubscriptionResponseDTO> SubscribeAsync(Guid userId, CreateSubscriptionDTO subscriptionDTO);
        Task<UserSubscriptionDTO?> GetActiveSubscriptionAsync(Guid userId);
        Task<IEnumerable<UserSubscriptionDTO>> GetUserSubscriptionHistoryAsync(Guid userId);
        Task<IEnumerable<SubscriptionPlanDTO>> GetAllSubscriptionPlansAsync();
        Task<IEnumerable<SubscriptionPlanCountryDTO>> GetSubscriptionPlanOptionsAsync(Guid subscriptionId, Guid countryId);
        Task<bool> CancelSubscriptionAsync(Guid userId);
        
        // Admin Methods
        Task<SubscriptionPlanDTO> CreateSubscriptionPlanAsync(CreatePlanDTO createPlanDTO);
        Task<SubscriptionPlanDTO?> UpdateSubscriptionPlanAsync(Guid subscriptionId, UpdatePlanDTO updatePlanDTO);
        Task<bool> DeleteSubscriptionPlanAsync(Guid subscriptionId);
        
        // Pricing Management
        Task<IEnumerable<SubscriptionPricingDTO>> GetSubscriptionPricingAsync(Guid subscriptionId);
        Task<SubscriptionPricingDTO> UpdateSubscriptionPriceAsync(Guid planCountryId, decimal newPrice);
        Task<SubscriptionPricingDTO> AddSubscriptionPriceAsync(CreatePlanPricingDTO pricingDTO);
        
        // Game Management
        Task<bool> AddGameToSubscriptionAsync(Guid subscriptionId, Guid gameId);
        Task<bool> RemoveGameFromSubscriptionAsync(Guid subscriptionId, Guid gameId);
        Task<IEnumerable<GameDTO>> GetIncludedGamesAsync(Guid subscriptionId);
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
        
        // Admin Methods
        Task<IEnumerable<UserDTO>> GetAllUsersAsync();

        /// <summary>
        /// Get paginated users with optional filtering
        /// </summary>
        Task<PagedResponse<UserDTO>> GetPagedUsersAsync(UserPaginationQuery query);
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
