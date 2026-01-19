using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IEntitlementService _entitlementService;
        private readonly IGameCountryRepository _gameCountryRepository;
        private readonly IUserRepository _userRepository;

        public GameService(IGameRepository gameRepository, IEntitlementService entitlementService, IGameCountryRepository gameCountryRepository, IUserRepository userRepository)
        {
            _gameRepository = gameRepository;
            _entitlementService = entitlementService;
            _gameCountryRepository = gameCountryRepository;
            _userRepository = userRepository;
        }

        public async Task<GameDTO?> GetGameByIdAsync(Guid gameId, Guid? userId = null)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) return null;
            
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }
            
            return await MapToGameDTOAsync(game, countryId);
        }

        public async Task<IEnumerable<GameDTO>> GetAllGamesAsync(bool includeDeleted = false, Guid? userId = null)
        {
            var games = includeDeleted 
                ? await _gameRepository.GetAllIncludingDeletedAsync()
                : await _gameRepository.GetAllAsync();
            
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }
            
            var gameDtos = new List<GameDTO>();
            foreach (var game in games)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }
            return gameDtos;
        }

        public async Task<IEnumerable<GameDTO>> SearchGamesAsync(string searchTerm, Guid? userId = null)
        {
            var games = await _gameRepository.SearchGamesAsync(searchTerm);
            
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }
            
            var gameDtos = new List<GameDTO>();
            foreach (var game in games)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }
            return gameDtos;
        }

        public async Task<IEnumerable<GameDTO>> GetGamesByCategoryAsync(Guid categoryId, Guid? userId = null)
        {
            var games = await _gameRepository.GetGamesByCategoryAsync(categoryId);
            
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }
            
            var gameDtos = new List<GameDTO>();
            foreach (var game in games)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }
            return gameDtos;
        }

        public async Task<IEnumerable<GameDTO>> GetFreeToPlayGamesAsync(Guid? userId = null)
        {
            var games = await _gameRepository.GetFreeGamesAsync();
            
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }
            
            var gameDtos = new List<GameDTO>();
            foreach (var game in games)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }
            return gameDtos;
        }

        public async Task<GameWithAccessDTO?> GetGameWithAccessAsync(Guid gameId, Guid userId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) return null;

            var accessResult = await _entitlementService.CanUserAccessGameAsync(userId, gameId);
            
            // Get user's country for pricing
            var user = await _userRepository.GetByIdAsync(userId);
            decimal price = game.BasePrice ?? 0m;
            
            if (user?.CountryId != null && !game.FreeToPlay)
            {
                var gameCountry = await _gameCountryRepository.GetGamePricingAsync(gameId, user.CountryId.Value);
                if (gameCountry != null)
                {
                    price = gameCountry.Price;
                }
            }

            return new GameWithAccessDTO
            {
                GameId = game.GameId,
                GameName = game.GameName,
                PublishedBy = game.PublishedBy,
                ReleaseDate = game.ReleaseDate,
                FreeToPlay = game.FreeToPlay,
                Price = price,
                IsMultiplayer = game.IsMultiplayer,
                CanAccess = accessResult.CanAccess,
                AccessType = accessResult.AccessType
            };
        }

        public async Task<GameDTO> CreateGameAsync(CreateGameDTO createGameDTO)
        {
            var game = new Game
            {
                GameName = createGameDTO.GameName,
                PublishedBy = createGameDTO.PublishedBy,
                ReleaseDate = createGameDTO.ReleaseDate,
                FreeToPlay = createGameDTO.FreeToPlay,
                BasePrice = createGameDTO.Price,
                IsMultiplayer = createGameDTO.IsMultiplayer,
                IsDeleted = false
            };

            await _gameRepository.AddAsync(game);
            await _gameRepository.SaveChangesAsync();

            return await MapToGameDTOAsync(game);
        }

        public async Task<GameDTO?> UpdateGameAsync(Guid gameId, UpdateGameDTO updateGameDTO)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) return null;

            if (updateGameDTO.GameName != null) game.GameName = updateGameDTO.GameName;
            if (updateGameDTO.PublishedBy != null) game.PublishedBy = updateGameDTO.PublishedBy;
            if (updateGameDTO.ReleaseDate.HasValue) game.ReleaseDate = updateGameDTO.ReleaseDate;
            if (updateGameDTO.FreeToPlay.HasValue) game.FreeToPlay = updateGameDTO.FreeToPlay.Value;
            if (updateGameDTO.Price.HasValue) game.BasePrice = updateGameDTO.Price.Value;
            if (updateGameDTO.IsMultiplayer.HasValue) game.IsMultiplayer = updateGameDTO.IsMultiplayer.Value;

            _gameRepository.Update(game);
            await _gameRepository.SaveChangesAsync();

            return await MapToGameDTOAsync(game);
        }

        public async Task<bool> SoftDeleteGameAsync(Guid gameId)
        {
            await _gameRepository.SoftDeleteAsync(gameId);
            return true;
        }

        public async Task<bool> RestoreGameAsync(Guid gameId)
        {
            return await _gameRepository.RestoreAsync(gameId);
        }

        public async Task<IEnumerable<GamePricingDTO>> GetGamePricingAsync(Guid gameId)
        {
            var prices = await _gameCountryRepository.GetPricesByGameIdAsync(gameId);
            return prices.Select(gc => new GamePricingDTO
            {
                GameCountryId = gc.GameCountryId,
                GameId = gc.GameId,
                CountryId = gc.CountryId,
                CountryName = gc.Country.CountryName,
                Currency = gc.Country.Currency,
                Price = gc.Price
            });
        }

        public async Task<GamePricingDTO> UpdateGamePriceAsync(Guid gameCountryId, decimal newPrice)
        {
            var gc = await _gameCountryRepository.GetByIdAsync(gameCountryId);
            if (gc == null) throw new KeyNotFoundException("Pricing not found.");
            
            // We need to reload Country for DTO return if not loaded
            // But GetByIdAsync usually doesn't include. 
            // Better to use GetGamePricingAsync or verify.
            // Let's assume we can fetch it again or just update.
            gc.Price = newPrice;
            _gameCountryRepository.Update(gc);
            await _gameCountryRepository.SaveChangesAsync();

            // Fetch fully for return
            var updated = (await _gameCountryRepository.GetPricesByGameIdAsync(gc.GameId))
                           .FirstOrDefault(x => x.GameCountryId == gameCountryId);
            
            return new GamePricingDTO
            {
                GameCountryId = updated!.GameCountryId,
                GameId = updated.GameId,
                CountryId = updated.CountryId,
                CountryName = updated.Country.CountryName,
                Currency = updated.Country.Currency,
                Price = updated.Price
            };
        }

        public async Task<GamePricingDTO> AddGamePriceAsync(CreateGamePricingDTO pricingDTO)
        {
             // Check existing
             var existing = await _gameCountryRepository.GetGamePricingAsync(pricingDTO.GameId, pricingDTO.CountryId);
             if (existing != null) throw new InvalidOperationException("Price for this country already exists.");

             var newGc = new GameCountry
             {
                 GameCountryId = Guid.NewGuid(),
                 GameId = pricingDTO.GameId,
                 CountryId = pricingDTO.CountryId,
                 Price = pricingDTO.Price
             };

             await _gameCountryRepository.AddAsync(newGc);
             await _gameCountryRepository.SaveChangesAsync();

             // Fetch for return
             var created = (await _gameCountryRepository.GetPricesByGameIdAsync(pricingDTO.GameId))
                           .FirstOrDefault(x => x.GameCountryId == newGc.GameCountryId);

             return new GamePricingDTO
             {
                GameCountryId = created!.GameCountryId,
                GameId = created.GameId,
                CountryId = created.CountryId,
                CountryName = created.Country.CountryName,
                Currency = created.Country.Currency,
                Price = created.Price
             };
        }

        private async Task<GameDTO> MapToGameDTOAsync(Game game, Guid? countryId = null)
        {
            decimal price = game.BasePrice ?? 0m;
            
            // Get country-specific price if countryId is provided
            if (countryId.HasValue && !game.FreeToPlay)
            {
                var gameCountry = await _gameCountryRepository.GetGamePricingAsync(game.GameId, countryId.Value);
                if (gameCountry != null)
                {
                    price = gameCountry.Price;
                }
            }
            
            return new GameDTO
            {
                GameId = game.GameId,
                GameName = game.GameName,
                PublishedBy = game.PublishedBy,
                ReleaseDate = game.ReleaseDate,
                FreeToPlay = game.FreeToPlay,
                Price = price,
                IsMultiplayer = game.IsMultiplayer
            };
        }
    }
}
