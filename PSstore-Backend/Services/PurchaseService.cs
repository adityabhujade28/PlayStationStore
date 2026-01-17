using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IUserPurchaseGameRepository _purchaseRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEntitlementService _entitlementService;

        public PurchaseService(
            IUserPurchaseGameRepository purchaseRepository,
            IGameRepository gameRepository,
            IUserRepository userRepository,
            IEntitlementService entitlementService)
        {
            _purchaseRepository = purchaseRepository;
            _gameRepository = gameRepository;
            _userRepository = userRepository;
            _entitlementService = entitlementService;
        }

        public async Task<PurchaseResponseDTO> PurchaseGameAsync(Guid userId, CreatePurchaseDTO purchaseDTO)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new PurchaseResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Validate game exists and is not free
            var game = await _gameRepository.GetByIdAsync(purchaseDTO.GameId);
            if (game == null)
            {
                return new PurchaseResponseDTO
                {
                    Success = false,
                    Message = "Game not found."
                };
            }

            if (game.FreeToPlay)
            {
                return new PurchaseResponseDTO
                {
                    Success = false,
                    Message = "This game is free to play. No purchase required."
                };
            }

            // Check if user already owns the game
            var alreadyPurchased = await _purchaseRepository.HasUserPurchasedGameAsync(userId, purchaseDTO.GameId);
            if (alreadyPurchased)
            {
                return new PurchaseResponseDTO
                {
                    Success = false,
                    Message = "You already own this game."
                };
            }

            // Check if game is accessible via active subscription
            var gameAccess = await _entitlementService.CanUserAccessGameAsync(userId, purchaseDTO.GameId);
            if (gameAccess.CanAccess && gameAccess.AccessType == "SUBSCRIPTION")
            {
                return new PurchaseResponseDTO
                {
                    Success = false,
                    Message = "This game is already accessible through your subscription. No purchase needed."
                };
            }

            // Create purchase record
            var purchase = new UserPurchaseGame
            {
                UserId = userId,
                GameId = purchaseDTO.GameId,
                PurchasePrice = game.BasePrice ?? 0m,
                PurchaseDate = DateTime.UtcNow
            };

            await _purchaseRepository.AddAsync(purchase);
            await _purchaseRepository.SaveChangesAsync();

            return new PurchaseResponseDTO
            {
                Success = true,
                Message = "Game purchased successfully!",
                PurchaseId = purchase.PurchaseId,
                GameName = game.GameName,
                PurchasePrice = purchase.PurchasePrice,
                PurchaseDate = purchase.PurchaseDate
            };
        }

        public async Task<IEnumerable<PurchaseHistoryDTO>> GetUserPurchaseHistoryAsync(Guid userId)
        {
            var purchases = await _purchaseRepository.GetUserPurchasesAsync(userId);
            
            return purchases.Select(p => new PurchaseHistoryDTO
            {
                PurchaseId = p.PurchaseId,
                GameId = p.GameId,
                GameName = p.Game?.GameName ?? "Unknown",
                PurchasePrice = p.PurchasePrice,
                PurchaseDate = p.PurchaseDate
            });
        }

        public async Task<bool> HasUserPurchasedGameAsync(Guid userId, Guid gameId)
        {
            return await _purchaseRepository.HasUserPurchasedGameAsync(userId, gameId);
        }

        public async Task<PurchaseResponseDTO?> GetPurchaseDetailsAsync(Guid purchaseId)
        {
            var purchase = await _purchaseRepository.GetPurchaseDetailsAsync(purchaseId);
            if (purchase == null) return null;

            return new PurchaseResponseDTO
            {
                Success = true,
                PurchaseId = purchase.PurchaseId,
                GameName = purchase.Game?.GameName ?? "Unknown",
                PurchasePrice = purchase.PurchasePrice,
                PurchaseDate = purchase.PurchaseDate,
                Message = "Purchase found."
            };
        }
    }
}
