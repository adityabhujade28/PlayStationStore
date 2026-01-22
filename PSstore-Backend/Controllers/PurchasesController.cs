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

        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseResponseDTO>> PurchaseGame([FromBody] CreatePurchaseDTO purchaseDTO, [FromQuery] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _purchaseService.PurchaseGameAsync(userId, purchaseDTO);
            
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetPurchaseDetails), new { id = result.PurchaseId }, result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PurchaseHistoryDTO>>> GetUserPurchaseHistory(Guid userId)
        {
            var purchases = await _purchaseService.GetUserPurchaseHistoryAsync(userId);
            return Ok(purchases);
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
            var pagedPurchases = await _purchaseService.GetPagedUserPurchaseHistoryAsync(userId, pageNumber, pageSize);
            return Ok(pagedPurchases);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseResponseDTO>> GetPurchaseDetails(Guid id)
        {
            var purchase = await _purchaseService.GetPurchaseDetailsAsync(id);
            if (purchase == null)
                return NotFound(new { message = "Purchase not found." });

            return Ok(purchase);
        }

        [HttpGet("check")]
        public async Task<ActionResult<bool>> HasUserPurchasedGame([FromQuery] Guid userId, [FromQuery] Guid gameId)
        {
            var result = await _purchaseService.HasUserPurchasedGameAsync(userId, gameId);
            return Ok(new { purchased = result });
        }
    }
}
