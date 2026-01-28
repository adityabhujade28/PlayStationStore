using Microsoft.AspNetCore.Mvc;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ILogger<PurchasesController> _logger;

        public PurchasesController(IPurchaseService purchaseService, ILogger<PurchasesController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseResponseDTO>> PurchaseGame([FromBody] CreatePurchaseDTO purchaseDTO, [FromQuery] Guid userId)
        {
            _logger.LogInformation("Purchase attempt initiated for user ID: {UserId}, Game ID: {GameId}", userId, purchaseDTO.GameId);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Purchase failed for user ID: {UserId}, Game ID: {GameId}. Reason: {Reason}", userId, purchaseDTO.GameId, result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("Purchase successful for user ID: {UserId}, Purchase ID: {PurchaseId}", userId, result.PurchaseId);
                return CreatedAtAction(nameof(GetPurchaseDetails), new { id = result.PurchaseId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during purchase for user ID: {UserId}, Game ID: {GameId}", userId, purchaseDTO.GameId);
                throw;
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PurchaseHistoryDTO>>> GetUserPurchaseHistory(Guid userId)
        {
            _logger.LogInformation("Fetching purchase history for user ID: {UserId}", userId);
            try
            {
                var purchases = await _purchaseService.GetUserPurchaseHistoryAsync(userId);
                _logger.LogInformation("Purchase history retrieved successfully for user ID: {UserId}", userId);
                return Ok(purchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase history for user ID: {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Get paginated purchase history for a user
        /// Recommended endpoint for user library with pagination support
        /// </summary>
        [HttpGet("user/{userId}/paged")]
        public async Task<ActionResult<PagedResponse<PurchaseHistoryDTO>>> GetUserPurchaseHistoryPaged(
            Guid userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            _logger.LogInformation("Fetching paginated purchase history. User ID: {UserId}, Page: {PageNumber}, Size: {PageSize}", userId, pageNumber, pageSize);
            try
            {
                var pagedPurchases = await _purchaseService.GetPagedUserPurchaseHistoryAsync(userId, pageNumber, pageSize);
                _logger.LogInformation("Paginated purchase history retrieved successfully for user ID: {UserId}", userId);
                return Ok(pagedPurchases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paginated purchase history for user ID: {UserId}", userId);
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseResponseDTO>> GetPurchaseDetails(Guid id)
        {
            _logger.LogInformation("Fetching purchase details. Purchase ID: {PurchaseId}", id);
            try
            {
                var purchase = await _purchaseService.GetPurchaseDetailsAsync(id);
                if (purchase == null)
                {
                    _logger.LogWarning("Purchase not found. ID: {PurchaseId}", id);
                    return NotFound(new { message = "Purchase not found." });
                }

                _logger.LogInformation("Purchase details retrieved successfully. ID: {PurchaseId}", id);
                return Ok(purchase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase details. ID: {PurchaseId}", id);
                throw;
            }
        }

        [HttpGet("check")]
        public async Task<ActionResult<bool>> HasUserPurchasedGame([FromQuery] Guid userId, [FromQuery] Guid gameId)
        {
            _logger.LogInformation("Checking if user has purchased game. User ID: {UserId}, Game ID: {GameId}", userId, gameId);
            try
            {
                var result = await _purchaseService.HasUserPurchasedGameAsync(userId, gameId);
                _logger.LogInformation("Purchase check completed. User ID: {UserId}, Game ID: {GameId}, Purchased: {Purchased}", userId, gameId, result);
                return Ok(new { purchased = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking purchase. User ID: {UserId}, Game ID: {GameId}", userId, gameId);
                throw;
            }
        }
    }
}
