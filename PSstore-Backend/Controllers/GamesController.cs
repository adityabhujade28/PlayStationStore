using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly ILogger<GamesController> _logger;

        public GamesController(IGameService gameService, ILogger<GamesController> logger)
        {
            _gameService = gameService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetAllGames([FromQuery] bool includeDeleted = false, [FromQuery] Guid? userId = null)
        {
            _logger.LogInformation("Fetching all games. IncludeDeleted: {IncludeDeleted}, UserId: {UserId}", includeDeleted, userId);
            var games = await _gameService.GetAllGamesAsync(includeDeleted, userId);
            return Ok(games);
        }

        /// <summary>
        /// Get paginated games with optional filtering and sorting
        /// Recommended endpoint for browsing games with pagination support
        /// </summary>
        [HttpGet("paged")]
        [Authorize]
        public async Task<ActionResult<PagedResponse<GameDTO>>> GetPagedGames(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] bool? freeToPlayOnly = null,
            [FromQuery] bool includeDeleted = false,
            [FromQuery] string? sortBy = null,
            [FromQuery] string sortDirection = "asc",
            [FromQuery] Guid? userId = null)
        {
            _logger.LogInformation("Fetching paginated games. Page: {PageNumber}, Size: {PageSize}", pageNumber, pageSize);

            var query = new GamePaginationQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                FreeToPlayOnly = freeToPlayOnly,
                IncludeDeleted = includeDeleted,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var pagedGames = await _gameService.GetPagedGamesAsync(query, userId);
            return Ok(pagedGames);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<GameDTO>> GetGameById(Guid id, [FromQuery] Guid? userId = null)
        {
            _logger.LogInformation("Fetching game details for ID: {GameId}", id);
            var game = await _gameService.GetGameByIdAsync(id, userId);
            if (game == null)
            {
                _logger.LogWarning("Game with ID: {GameId} not found.", id);
                return NotFound(new { message = "Game not found." });
            }

            return Ok(game);
        }

        [HttpGet("{id}/access/{userId}")]
        [Authorize]
        public async Task<ActionResult<GameWithAccessDTO>> GetGameWithAccess(Guid id, Guid userId)
        {
            var game = await _gameService.GetGameWithAccessAsync(id, userId);
            if (game == null)
                return NotFound(new { message = "Game not found." });

            return Ok(game);
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GameDTO>>> SearchGames([FromQuery] string query, [FromQuery] Guid? userId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query is required." });

            var games = await _gameService.SearchGamesAsync(query, userId);
            return Ok(games);
        }

        /// <summary>
        /// Search games with pagination
        /// Recommended endpoint for paginated search results
        /// </summary>
        [HttpGet("search/paged")]
        [Authorize]
        public async Task<ActionResult<PagedResponse<GameDTO>>> SearchGamesPaged(
            [FromQuery] string query,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? userId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query is required." });

            _logger.LogInformation("Searching games with query: {Query}, Page: {PageNumber}, Size: {PageSize}", query, pageNumber, pageSize);
            
            var pagedResults = await _gameService.GetPagedSearchResultsAsync(query, pageNumber, pageSize, userId);
            return Ok(pagedResults);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetGamesByCategory(Guid categoryId, [FromQuery] Guid? userId = null)
        {
            var games = await _gameService.GetGamesByCategoryAsync(categoryId, userId);
            return Ok(games);
        }

        /// <summary>
        /// Get games by category with pagination
        /// Recommended endpoint for paginated category browsing
        /// </summary>
        [HttpGet("category/{categoryId}/paged")]
        public async Task<ActionResult<PagedResponse<GameDTO>>> GetGamesByCategoryPaged(
            Guid categoryId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] Guid? userId = null)
        {
            _logger.LogInformation("Fetching games by category {CategoryId} with pagination. Page: {PageNumber}, Size: {PageSize}", categoryId, pageNumber, pageSize);
            
            var pagedGames = await _gameService.GetPagedGamesByCategoryAsync(categoryId, pageNumber, pageSize, userId);
            return Ok(pagedGames);
        }

        [HttpGet("free-to-play")]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetFreeToPlayGames([FromQuery] Guid? userId = null)
        {
            var games = await _gameService.GetFreeToPlayGamesAsync(userId);
            return Ok(games);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GameDTO>> CreateGame([FromBody] CreateGameDTO createGameDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var game = await _gameService.CreateGameAsync(createGameDTO);
            return CreatedAtAction(nameof(GetGameById), new { id = game.GameId }, game);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GameDTO>> UpdateGame(Guid id, [FromBody] UpdateGameDTO updateGameDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var game = await _gameService.UpdateGameAsync(id, updateGameDTO);
            if (game == null)
                return NotFound(new { message = "Game not found." });

            return Ok(game);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] 
        public async Task<ActionResult> SoftDeleteGame(Guid id)
        {
            var result = await _gameService.SoftDeleteGameAsync(id);
            if (!result)
                return NotFound(new { message = "Game not found." });

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RestoreGame(Guid id)
        {
            var result = await _gameService.RestoreGameAsync(id);
            if (!result)
                return NotFound(new { message = "Game not found." });

            return Ok(new { message = "Game restored successfully." });
        }
        [HttpGet("{id}/pricing")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GamePricingDTO>>> GetGamePricing(Guid id)
        {
            var pricing = await _gameService.GetGamePricingAsync(id);
            return Ok(pricing);
        }

        [HttpPut("pricing/{gameCountryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GamePricingDTO>> UpdateGamePrice(Guid gameCountryId, [FromBody] decimal newPrice)
        {
            try
            {
                var result = await _gameService.UpdateGamePriceAsync(gameCountryId, newPrice);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("pricing")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GamePricingDTO>> AddGamePrice([FromBody] CreateGamePricingDTO pricingDTO)
        {
            try
            {
                var result = await _gameService.AddGamePriceAsync(pricingDTO);
                return CreatedAtAction(nameof(GetGamePricing), new { id = pricingDTO.GameId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
