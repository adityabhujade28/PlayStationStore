using Microsoft.EntityFrameworkCore;
using PSstore.Data;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Services
{
    public class EntitlementService : IEntitlementService
    {
        private readonly AppDbContext _context;
        private readonly IGameRepository _gameRepository;
        private readonly IUserPurchaseGameRepository _purchaseRepository;
        private readonly IUserSubscriptionPlanRepository _subscriptionRepository;

        public EntitlementService(
            AppDbContext context,
            IGameRepository gameRepository,
            IUserPurchaseGameRepository purchaseRepository,
            IUserSubscriptionPlanRepository subscriptionRepository)
        {
            _context = context;
            _gameRepository = gameRepository;
            _purchaseRepository = purchaseRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<GameAccessResultDTO> CanUserAccessGameAsync(int userId, int gameId)
        {
            // Get game details
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
            {
                return new GameAccessResultDTO
                {
                    GameId = gameId,
                    GameName = "Unknown",
                    CanAccess = false,
                    AccessType = "NO_ACCESS",
                    Message = "Game not found"
                };
            }

            // Rule 1: Free-to-play games are accessible to everyone
            if (game.FreeToPlay)
            {
                return new GameAccessResultDTO
                {
                    GameId = game.GameId,
                    GameName = game.GameName,
                    CanAccess = true,
                    AccessType = "FREE",
                    Message = "This is a free-to-play game"
                };
            }

            // Rule 2: Check if user has purchased the game (permanent ownership)
            var hasPurchased = await _purchaseRepository.HasUserPurchasedGameAsync(userId, gameId);
            if (hasPurchased)
            {
                var purchase = await _context.UserPurchaseGames
                    .FirstOrDefaultAsync(upg => upg.UserId == userId && upg.GameId == gameId);

                return new GameAccessResultDTO
                {
                    GameId = game.GameId,
                    GameName = game.GameName,
                    CanAccess = true,
                    AccessType = "PURCHASED",
                    Message = "You own this game permanently",
                    PurchasedOn = purchase?.PurchaseDate
                };
            }

            // Rule 3: Check if user has active subscription that includes this game
            var activeSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (activeSubscription != null)
            {
                // Check if game is included in the subscription plan
                var gameInPlan = await _context.GameSubscriptions
                    .AnyAsync(gs => gs.GameId == gameId && 
                                   gs.SubscriptionId == activeSubscription.SubscriptionPlanCountry.SubscriptionId);

                if (gameInPlan)
                {
                    return new GameAccessResultDTO
                    {
                        GameId = game.GameId,
                        GameName = game.GameName,
                        CanAccess = true,
                        AccessType = "SUBSCRIPTION",
                        Message = $"Included in your {activeSubscription.SubscriptionPlanCountry.SubscriptionPlan.SubscriptionType} subscription",
                        SubscriptionPlan = activeSubscription.SubscriptionPlanCountry.SubscriptionPlan.SubscriptionType,
                        SubscriptionExpiresOn = activeSubscription.PlanEndDate
                    };
                }
            }

            // Rule 4: No access
            return new GameAccessResultDTO
            {
                GameId = game.GameId,
                GameName = game.GameName,
                CanAccess = false,
                AccessType = "NO_ACCESS",
                Message = "You need to purchase this game or subscribe to a plan that includes it"
            };
        }

        public async Task<UserLibraryDTO> GetUserLibraryAsync(int userId)
        {
            var library = new UserLibraryDTO
            {
                UserId = userId,
                AccessibleGames = new List<GameAccessResultDTO>()
            };

            // Get all games (not soft-deleted)
            var allGames = await _gameRepository.GetAllAsync();

            // Check access for each game
            foreach (var game in allGames)
            {
                var accessResult = await CanUserAccessGameAsync(userId, game.GameId);
                if (accessResult.CanAccess)
                {
                    library.AccessibleGames.Add(accessResult);

                    // Count by access type
                    switch (accessResult.AccessType)
                    {
                        case "FREE":
                            library.TotalFreeGames++;
                            break;
                        case "PURCHASED":
                            library.TotalPurchasedGames++;
                            break;
                        case "SUBSCRIPTION":
                            library.TotalSubscriptionGames++;
                            break;
                    }
                }
            }

            return library;
        }

        public async Task<IEnumerable<GameDTO>> GetSubscriptionGamesAsync(int subscriptionPlanId)
        {
            var games = await _context.GameSubscriptions
                .Where(gs => gs.SubscriptionId == subscriptionPlanId)
                .Include(gs => gs.Game)
                    .ThenInclude(g => g.GameCategories)
                        .ThenInclude(gc => gc.Category)
                .Select(gs => gs.Game)
                .ToListAsync();

            return games.Select(g => new GameDTO
            {
                GameId = g.GameId,
                GameName = g.GameName,
                PublishedBy = g.PublishedBy,
                ReleaseDate = g.ReleaseDate,
                FreeToPlay = g.FreeToPlay,
                Price = g.BasePrice ?? 0m,
                IsMultiplayer = g.IsMultiplayer,
                Categories = g.GameCategories.Select(gc => gc.Category.CategoryName).ToList()
            }).ToList();
        }

        public async Task<bool> HasAnyEntitlementsAsync(int userId)
        {
            // Check if user has any purchases
            var hasPurchases = await _context.UserPurchaseGames
                .AnyAsync(upg => upg.UserId == userId);

            if (hasPurchases)
                return true;

            // Check if user has active subscription
            var hasActiveSubscription = await _subscriptionRepository.HasActiveSubscriptionAsync(userId);

            return hasActiveSubscription;
        }
    }
}
