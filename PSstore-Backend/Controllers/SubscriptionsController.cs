
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly ILogger<SubscriptionsController> _logger;

        public SubscriptionsController(ISubscriptionService subscriptionService, ILogger<SubscriptionsController> logger)
        {
            _subscriptionService = subscriptionService;
            _logger = logger;
        }

        [HttpGet("plans")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDTO>>> GetAllPlans()
        {
            _logger.LogInformation("Fetching available subscription plans.");
            var plans = await _subscriptionService.GetAllSubscriptionPlansAsync();
            return Ok(plans);
        }


        [HttpPost]
        public async Task<ActionResult<SubscriptionResponseDTO>> Subscribe([FromBody] CreateSubscriptionDTO subscriptionDTO, [FromQuery] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Subscription attempt for user ID: {UserId}, Plan ID: {PlanId}", userId, subscriptionDTO.SubscriptionPlanCountryId);
            var result = await _subscriptionService.SubscribeAsync(userId, subscriptionDTO);

            if (!result.Success)
            {
                _logger.LogWarning("Subscription failed for user ID: {UserId}. Reason: {Reason}", userId, result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("Subscription successful for user ID: {UserId}, Subscription ID: {SubscriptionId}", userId, result.UserSubscriptionId);
            return Ok(result);
        }

        [HttpGet("user/{userId}/active")]
        public async Task<ActionResult<UserSubscriptionDTO>> GetActiveSubscription(Guid userId)
        {
            var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId);
            if (subscription == null)
                return NotFound(new { message = "No active subscription found." });

            return Ok(subscription);
        }

        [HttpGet("user/{userId}/history")]
        public async Task<ActionResult<IEnumerable<UserSubscriptionDTO>>> GetUserSubscriptionHistory(Guid userId)
        {
            var subscriptions = await _subscriptionService.GetUserSubscriptionHistoryAsync(userId);
            return Ok(subscriptions);
        }

        [HttpDelete("user/{userId}")]
        public async Task<ActionResult> CancelSubscription(Guid userId)
        {
            _logger.LogInformation("Subscription cancellation requested for user ID: {UserId}", userId);
            var result = await _subscriptionService.CancelSubscriptionAsync(userId);
            if (!result)
            {
                _logger.LogWarning("Cancel failed for user ID: {UserId}. No active subscription found.", userId);
                return NotFound(new { message = "No active subscription to cancel." });
            }

            _logger.LogInformation("Subscription cancelled successfully for user ID: {UserId}", userId);
            return Ok(new { message = "Subscription cancelled successfully." });
        }

        [HttpPost("plans")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionPlanDTO>> CreatePlan([FromBody] CreatePlanDTO createPlanDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var plan = await _subscriptionService.CreateSubscriptionPlanAsync(createPlanDTO);
            return CreatedAtAction(nameof(GetAllPlans), new { }, plan);
        }

        [HttpPut("plans/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionPlanDTO>> UpdatePlan(Guid id, [FromBody] UpdatePlanDTO updatePlanDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var plan = await _subscriptionService.UpdateSubscriptionPlanAsync(id, updatePlanDTO);
            if (plan == null)
                return NotFound(new { message = "Subscription plan not found." });

            return Ok(plan);
        }

        [HttpDelete("plans/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeletePlan(Guid id)
        {
            try 
            {
                var result = await _subscriptionService.DeleteSubscriptionPlanAsync(id);
                if (!result)
                    return NotFound(new { message = "Subscription plan not found." });

                return NoContent();
            }
            catch (Exception)
            {
                // Likely FK constraint
                return BadRequest(new { message = "Cannot delete plan as it is in use by users or has linked data." });
            }
        }
        [HttpGet("pricing/{subscriptionId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<SubscriptionPricingDTO>>> GetPricing(Guid subscriptionId)
        {
            var pricing = await _subscriptionService.GetSubscriptionPricingAsync(subscriptionId);
            return Ok(pricing);
        }

        [HttpPut("pricing/{planCountryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionPricingDTO>> UpdatePricing(Guid planCountryId, [FromBody] decimal newPrice)
        {
            try
            {
                var result = await _subscriptionService.UpdateSubscriptionPriceAsync(planCountryId, newPrice);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("pricing")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<SubscriptionPricingDTO>> AddPricing([FromBody] CreatePlanPricingDTO pricingDTO)
        {
            try
            {
                var result = await _subscriptionService.AddSubscriptionPriceAsync(pricingDTO);
                return CreatedAtAction(nameof(GetPricing), new { subscriptionId = pricingDTO.SubscriptionId }, result);
            }
            catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
            catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { message = ex.Message, stack = ex.StackTrace }); }
        }

        [HttpGet("{subscriptionId}/games")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<GameDTO>>> GetIncludedGames(Guid subscriptionId)
        {
            var games = await _subscriptionService.GetIncludedGamesAsync(subscriptionId);
            return Ok(games);
        }

        [HttpPost("{subscriptionId}/games/{gameId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> AddGameToPlan(Guid subscriptionId, Guid gameId)
        {
            var success = await _subscriptionService.AddGameToSubscriptionAsync(subscriptionId, gameId);
            if (!success) return Conflict(new { message = "Game already in plan or invalid." });
            return Ok(new { message = "Game added to plan." });
        }

        [HttpDelete("{subscriptionId}/games/{gameId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RemoveGameFromPlan(Guid subscriptionId, Guid gameId)
        {
            var success = await _subscriptionService.RemoveGameFromSubscriptionAsync(subscriptionId, gameId);
            if (!success) return NotFound(new { message = "Game not in plan." });
            return Ok(new { message = "Game removed from plan." });
        }
    }
}
