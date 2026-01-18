using PSstore.DTOs;

namespace PSstore.Interfaces
{
    public interface IEntitlementService
    {
        /// <summary>
        /// Check if a user can access a specific game
        /// </summary>
        Task<GameAccessResultDTO> CanUserAccessGameAsync(Guid userId, Guid gameId);

        /// <summary>
        /// Get all games a user can currently access
        /// </summary>
        Task<UserLibraryDTO> GetUserLibraryAsync(Guid userId);

        /// <summary>
        /// Get all games included in a subscription plan
        /// </summary>
        Task<IEnumerable<GameDTO>> GetSubscriptionGamesAsync(Guid subscriptionPlanId);

        /// <summary>
        /// Check if user has any active entitlements (purchased or subscription)
        /// </summary>
        Task<bool> HasAnyEntitlementsAsync(Guid userId);
    }
}
