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
        private readonly ICountryRepository _countryRepository;

        public GameService(IGameRepository gameRepository, IEntitlementService entitlementService, IGameCountryRepository gameCountryRepository, IUserRepository userRepository, ICountryRepository countryRepository)
        {
            _gameRepository = gameRepository;
            _entitlementService = entitlementService;
            _gameCountryRepository = gameCountryRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
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
                ImageUrl = game.ImageUrl,
                CanAccess = accessResult.CanAccess,
                AccessType = accessResult.AccessType
            };
        }

        /// <summary>
        /// Creates a new game with region-wise pricing.
        /// - Region-wise pricing is saved in GameCountries table
        /// - India (IN) price is used as BasePrice in Games table
        /// - If India price not available, first price from Pricing array is used
        /// - If no Pricing array, falls back to the Price field
        /// - ImageUrl is saved to Games table
        /// </summary>
        public async Task<GameDTO> CreateGameAsync(CreateGameDTO createGameDTO)
        {
            // Log incoming data for debugging
            Console.WriteLine($"[CreateGame] GameName: {createGameDTO.GameName}");
            Console.WriteLine($"[CreateGame] ImageUrl: {createGameDTO.ImageUrl ?? "NULL"}");
            Console.WriteLine($"[CreateGame] Pricing Count: {createGameDTO.Pricing?.Count ?? 0}");
            
            // Determine BasePrice (Use India price if available from Pricing array)
            decimal? basePrice = null;
            
            if (createGameDTO.Pricing != null && createGameDTO.Pricing.Any())
            {
                // Try to find India price first
                var indiaCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryCode == "IN");
                Console.WriteLine($"[CreateGame] India Country Found: {indiaCountry != null}");
                
                if (indiaCountry != null)
                {
                    Console.WriteLine($"[CreateGame] India CountryId: {indiaCountry.CountryId}");
                    var indiaPrice = createGameDTO.Pricing.FirstOrDefault(p => p.CountryId == indiaCountry.CountryId);
                    
                    if (indiaPrice != null)
                    {
                        basePrice = indiaPrice.Price;
                        Console.WriteLine($"[CreateGame] Using India Price as BasePrice: {basePrice}");
                    }
                    else
                    {
                        Console.WriteLine($"[CreateGame] India price not found in Pricing array");
                        // Log all country IDs in pricing for debugging
                        foreach (var p in createGameDTO.Pricing)
                        {
                            Console.WriteLine($"[CreateGame] Pricing CountryId: {p.CountryId}, Price: {p.Price}");
                        }
                    }
                }
                
                // If India price not found, use first available price as fallback
                if (!basePrice.HasValue && createGameDTO.Pricing.Any())
                {
                    basePrice = createGameDTO.Pricing.First().Price;
                    Console.WriteLine($"[CreateGame] Using first price as BasePrice fallback: {basePrice}");
                }
            }
            else if (createGameDTO.Price > 0)
            {
                // Fallback to the old Price field if no Pricing array provided
                basePrice = createGameDTO.Price;
                Console.WriteLine($"[CreateGame] Using legacy Price field as BasePrice: {basePrice}");
            }
            else
            {
                Console.WriteLine($"[CreateGame] No BasePrice set (free game or no pricing provided)");
            }

            var game = new Game
            {
                GameName = createGameDTO.GameName,
                PublishedBy = createGameDTO.PublishedBy,
                ReleaseDate = createGameDTO.ReleaseDate,
                FreeToPlay = createGameDTO.FreeToPlay,
                BasePrice = basePrice,
                IsMultiplayer = createGameDTO.IsMultiplayer,
                ImageUrl = createGameDTO.ImageUrl, // Save image URL to Games table
                IsDeleted = false
            };
            
            Console.WriteLine($"[CreateGame] Game entity created with ImageUrl: {game.ImageUrl ?? "NULL"}");

            await _gameRepository.AddAsync(game);
            await _gameRepository.SaveChangesAsync();
            
            Console.WriteLine($"[CreateGame] Game saved with ID: {game.GameId}");

            // Add per-country/region-wise pricing - save all to GameCountries table
            if (createGameDTO.Pricing != null && createGameDTO.Pricing.Any())
            {
                foreach (var pricingInput in createGameDTO.Pricing)
                {
                    var gameCountry = new GameCountry
                    {
                        GameCountryId = Guid.NewGuid(),
                        GameId = game.GameId,
                        CountryId = pricingInput.CountryId,
                        Price = pricingInput.Price
                    };
                    await _gameCountryRepository.AddAsync(gameCountry);
                }
                await _gameCountryRepository.SaveChangesAsync();
            }

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
            if (updateGameDTO.ImageUrl != null) game.ImageUrl = updateGameDTO.ImageUrl;

            game.UpdatedAt = DateTime.UtcNow;
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
                IsMultiplayer = game.IsMultiplayer,
                IsDeleted = game.IsDeleted,
                ImageUrl = game.ImageUrl
            };
        }

        /// <summary>
        /// Get paginated games with support for filtering, searching, and sorting
        /// Optimized to only load requested page of games and resolve country pricing
        /// </summary>
        public async Task<PagedResponse<GameDTO>> GetPagedGamesAsync(GamePaginationQuery query, Guid? userId = null)
        {
            // Get paginated games from repository
            var pagedGames = await _gameRepository.GetPagedGamesAsync(query);

            // Get user's country for pricing
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }

            // Convert to DTOs efficiently
            var gameDtos = new List<GameDTO>();
            foreach (var game in pagedGames.Items)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }

            return new PagedResponse<GameDTO>(gameDtos, pagedGames.TotalCount, pagedGames.PageNumber, pagedGames.PageSize);
        }

        /// <summary>
        /// Get paginated games by category
        /// Optimized for category browsing with pagination
        /// </summary>
        public async Task<PagedResponse<GameDTO>> GetPagedGamesByCategoryAsync(Guid categoryId, int pageNumber, int pageSize, Guid? userId = null)
        {
            // Get paginated games from repository
            var pagedGames = await _gameRepository.GetPagedGamesByCategoryAsync(categoryId, pageNumber, pageSize);

            // Get user's country for pricing
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }

            // Convert to DTOs efficiently
            var gameDtos = new List<GameDTO>();
            foreach (var game in pagedGames.Items)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }

            return new PagedResponse<GameDTO>(gameDtos, pagedGames.TotalCount, pagedGames.PageNumber, pagedGames.PageSize);
        }

        /// <summary>
        /// Get paginated search results
        /// Optimized for search with pagination
        /// </summary>
        public async Task<PagedResponse<GameDTO>> GetPagedSearchResultsAsync(string searchTerm, int pageNumber, int pageSize, Guid? userId = null)
        {
            // Get paginated search results from repository
            var pagedGames = await _gameRepository.GetPagedSearchResultsAsync(searchTerm, pageNumber, pageSize);

            // Get user's country for pricing
            Guid? countryId = null;
            if (userId.HasValue)
            {
                var user = await _userRepository.GetByIdAsync(userId.Value);
                countryId = user?.CountryId;
            }

            // Convert to DTOs efficiently
            var gameDtos = new List<GameDTO>();
            foreach (var game in pagedGames.Items)
            {
                gameDtos.Add(await MapToGameDTOAsync(game, countryId));
            }

            return new PagedResponse<GameDTO>(gameDtos, pagedGames.TotalCount, pagedGames.PageNumber, pagedGames.PageSize);
        }
    }
}
