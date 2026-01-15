using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly IEntitlementService _entitlementService;

        public GameService(IGameRepository gameRepository, IEntitlementService entitlementService)
        {
            _gameRepository = gameRepository;
            _entitlementService = entitlementService;
        }

        public async Task<GameDTO?> GetGameByIdAsync(int gameId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            return game != null ? MapToGameDTO(game) : null;
        }

        public async Task<IEnumerable<GameDTO>> GetAllGamesAsync(bool includeDeleted = false)
        {
            var games = includeDeleted 
                ? await _gameRepository.GetAllIncludingDeletedAsync()
                : await _gameRepository.GetAllAsync();
            
            return games.Select(MapToGameDTO);
        }

        public async Task<IEnumerable<GameDTO>> SearchGamesAsync(string searchTerm)
        {
            var games = await _gameRepository.SearchGamesAsync(searchTerm);
            return games.Select(MapToGameDTO);
        }

        public async Task<IEnumerable<GameDTO>> GetGamesByCategoryAsync(int categoryId)
        {
            var games = await _gameRepository.GetGamesByCategoryAsync(categoryId);
            return games.Select(MapToGameDTO);
        }

        public async Task<IEnumerable<GameDTO>> GetFreeToPlayGamesAsync()
        {
            var games = await _gameRepository.GetFreeGamesAsync();
            return games.Select(MapToGameDTO);
        }

        public async Task<GameWithAccessDTO?> GetGameWithAccessAsync(int gameId, int userId)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) return null;

            var accessResult = await _entitlementService.CanUserAccessGameAsync(userId, gameId);

            return new GameWithAccessDTO
            {
                GameId = game.GameId,
                GameName = game.GameName,
                PublishedBy = game.PublishedBy,
                ReleaseDate = game.ReleaseDate,
                FreeToPlay = game.FreeToPlay,
                Price = game.Price,
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
                Price = createGameDTO.Price,
                IsMultiplayer = createGameDTO.IsMultiplayer,
                IsDeleted = false
            };

            await _gameRepository.AddAsync(game);
            await _gameRepository.SaveChangesAsync();

            return MapToGameDTO(game);
        }

        public async Task<GameDTO?> UpdateGameAsync(int gameId, UpdateGameDTO updateGameDTO)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null) return null;

            if (updateGameDTO.GameName != null) game.GameName = updateGameDTO.GameName;
            if (updateGameDTO.PublishedBy != null) game.PublishedBy = updateGameDTO.PublishedBy;
            if (updateGameDTO.ReleaseDate.HasValue) game.ReleaseDate = updateGameDTO.ReleaseDate;
            if (updateGameDTO.FreeToPlay.HasValue) game.FreeToPlay = updateGameDTO.FreeToPlay.Value;
            if (updateGameDTO.Price.HasValue) game.Price = updateGameDTO.Price.Value;
            if (updateGameDTO.IsMultiplayer.HasValue) game.IsMultiplayer = updateGameDTO.IsMultiplayer.Value;

            _gameRepository.Update(game);
            await _gameRepository.SaveChangesAsync();

            return MapToGameDTO(game);
        }

        public async Task<bool> SoftDeleteGameAsync(int gameId)
        {
            await _gameRepository.SoftDeleteAsync(gameId);
            return true;
        }

        public async Task<bool> RestoreGameAsync(int gameId)
        {
            return await _gameRepository.RestoreAsync(gameId);
        }

        private static GameDTO MapToGameDTO(Game game)
        {
            return new GameDTO
            {
                GameId = game.GameId,
                GameName = game.GameName,
                PublishedBy = game.PublishedBy,
                ReleaseDate = game.ReleaseDate,
                FreeToPlay = game.FreeToPlay,
                Price = game.Price,
                IsMultiplayer = game.IsMultiplayer
            };
        }
    }
}
