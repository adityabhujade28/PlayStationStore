using Microsoft.AspNetCore.Mvc;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetAllGames([FromQuery] bool includeDeleted = false)
        {
            var games = await _gameService.GetAllGamesAsync(includeDeleted);
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameDTO>> GetGameById(int id)
        {
            var game = await _gameService.GetGameByIdAsync(id);
            if (game == null)
                return NotFound(new { message = "Game not found." });

            return Ok(game);
        }

        [HttpGet("{id}/access/{userId}")]
        public async Task<ActionResult<GameWithAccessDTO>> GetGameWithAccess(int id, int userId)
        {
            var game = await _gameService.GetGameWithAccessAsync(id, userId);
            if (game == null)
                return NotFound(new { message = "Game not found." });

            return Ok(game);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GameDTO>>> SearchGames([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query is required." });

            var games = await _gameService.SearchGamesAsync(query);
            return Ok(games);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetGamesByCategory(int categoryId)
        {
            var games = await _gameService.GetGamesByCategoryAsync(categoryId);
            return Ok(games);
        }

        [HttpGet("free-to-play")]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetFreeToPlayGames()
        {
            var games = await _gameService.GetFreeToPlayGamesAsync();
            return Ok(games);
        }

        [HttpPost]
        public async Task<ActionResult<GameDTO>> CreateGame([FromBody] CreateGameDTO createGameDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var game = await _gameService.CreateGameAsync(createGameDTO);
            return CreatedAtAction(nameof(GetGameById), new { id = game.GameId }, game);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GameDTO>> UpdateGame(int id, [FromBody] UpdateGameDTO updateGameDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var game = await _gameService.UpdateGameAsync(id, updateGameDTO);
            if (game == null)
                return NotFound(new { message = "Game not found." });

            return Ok(game);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> SoftDeleteGame(int id)
        {
            var result = await _gameService.SoftDeleteGameAsync(id);
            if (!result)
                return NotFound(new { message = "Game not found." });

            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<ActionResult> RestoreGame(int id)
        {
            var result = await _gameService.RestoreGameAsync(id);
            if (!result)
                return NotFound(new { message = "Game not found." });

            return Ok(new { message = "Game restored successfully." });
        }
    }
}
