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
        private readonly ISubscriptionPlanRepository _planRepository;

        public EntitlementService(
            AppDbContext context,
            IGameRepository gameRepository,
            IUserPurchaseGameRepository purchaseRepository,
            IUserSubscriptionPlanRepository subscriptionRepository,
            ISubscriptionPlanRepository planRepository)
        {
            _context = context;
            _gameRepository = gameRepository;
            _purchaseRepository = purchaseRepository;
            _subscriptionRepository = subscriptionRepository;
            _planRepository = planRepository;
        }

        public async Task<GameAccessResultDTO> CanUserAccessGameAsync(Guid userId, Guid gameId)
        {
            // Same implementation as before, but ensure it's correct
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

            var activeSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (activeSubscription != null)
            {
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

            return new GameAccessResultDTO
            {
                GameId = game.GameId,
                GameName = game.GameName,
                CanAccess = false,
                AccessType = "NO_ACCESS",
                Message = "You need to purchase this game or subscribe to a plan that includes it"
            };
        }

        public async Task<UserLibraryDTO> GetUserLibraryAsync(Guid userId)
        {
            var library = new UserLibraryDTO
            {
                UserId = userId,
                AccessibleGames = new List<GameAccessResultDTO>()
            };

            // 1. Get Purchased Game IDs (1 Query)
            var purchasedGameIds = (await _purchaseRepository.GetPurchasedGameIdsAsync(userId)).ToHashSet();

            // 2. Get Free Game IDs (1 Query)
            var freeGameIds = (await _gameRepository.GetFreeGameIdsAsync()).ToHashSet();

            // 3. Get Subscription Game IDs (1-2 Queries)
            var subscriptionGameIds = new HashSet<Guid>();
            var activeSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(userId);
            
            if (activeSubscription != null)
            {
                var plan = await _planRepository.GetPlanWithGamesAsync(activeSubscription.SubscriptionPlanCountry.SubscriptionId);
                if (plan?.GameSubscriptions != null)
                {
                    foreach (var gs in plan.GameSubscriptions)
                    {
                        subscriptionGameIds.Add(gs.GameId);
                    }
                }
            }

            // 4. Combine all unique IDs
            var allAccessibleIds = purchasedGameIds
                .Union(freeGameIds)
                .Union(subscriptionGameIds)
                .ToList();

            if (!allAccessibleIds.Any())
                return library;

            // 5. Batch Fetch Game Details (1 Query)
            var games = await _gameRepository.GetGamesByIdsAsync(allAccessibleIds);

            // 6. Build Result in Memory
            foreach (var game in games)
            {
                var accessResult = new GameAccessResultDTO
                {
                    GameId = game.GameId,
                    GameName = game.GameName,
                    CanAccess = true
                };

                // Determine primary access type (Priority: Purchased > Subscription > Free)
                // Actually Priority usually: Purchased (Permanent) > Subscription (Timed) > Free (Generic)
                
                if (purchasedGameIds.Contains(game.GameId))
                {
                    accessResult.AccessType = "PURCHASED";
                    accessResult.Message = "You own this game permanently";
                    library.TotalPurchasedGames++;
                }
                else if (subscriptionGameIds.Contains(game.GameId))
                {
                    accessResult.AccessType = "SUBSCRIPTION";
                    accessResult.Message = $"Included in your {activeSubscription?.SubscriptionPlanCountry.SubscriptionPlan.SubscriptionType} subscription";
                    accessResult.SubscriptionPlan = activeSubscription?.SubscriptionPlanCountry.SubscriptionPlan.SubscriptionType;
                    accessResult.SubscriptionExpiresOn = activeSubscription?.PlanEndDate;
                    library.TotalSubscriptionGames++;
                }
                else if (freeGameIds.Contains(game.GameId))
                {
                    accessResult.AccessType = "FREE";
                    accessResult.Message = "This is a free-to-play game";
                    library.TotalFreeGames++;
                }

                library.AccessibleGames.Add(accessResult);
            }

            // Optional: Get purchase dates for purchased games (Requires one more query or improved previous query)
            // Current implementation of 'CanUserAccess' fetches date.
            // For batch view, we might skip the specific purchase date or fetch it in the first step if needed.
            // The DTO has 'PurchasedOn'.
            // To be perfect, we should fetch UserPurchases with dates in step 1.
            // Step 1 was: GetPurchasedGameIdsAsync.
            // Better: GetUserPurchasesAsync (which returns list of UserPurchaseGame objects with dates).
            // Let's optimize Step 1 slightly in memory if we have the objects.
            
            // Re-optimizing Step 1 inline:
            var purchases = await _purchaseRepository.GetUserPurchasesAsync(userId);
            var purchaseMap = purchases.ToDictionary(p => p.GameId, p => p.PurchaseDate);
            
            // Update the loop to use purchaseMap
            foreach (var result in library.AccessibleGames)
            {
                if (result.AccessType == "PURCHASED" && purchaseMap.TryGetValue(result.GameId, out var date))
                {
                    result.PurchasedOn = date;
                }
            }

            return library;
        }

        public async Task<IEnumerable<GameDTO>> GetSubscriptionGamesAsync(Guid subscriptionPlanId)
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

        public async Task<bool> HasAnyEntitlementsAsync(Guid userId)
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
