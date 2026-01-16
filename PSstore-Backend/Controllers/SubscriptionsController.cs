
using Microsoft.AspNetCore.Mvc;
using PSstore.DTOs;
using PSstore.Interfaces;

namespace PSstore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet("plans")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDTO>>> GetAllPlans()
        {
            var plans = await _subscriptionService.GetAllSubscriptionPlansAsync();
            return Ok(plans);
        }

        [HttpGet("plans/{subscriptionId}/options")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanCountryDTO>>> GetPlanOptions(Guid subscriptionId, [FromQuery] Guid countryId)
        {
            var options = await _subscriptionService.GetSubscriptionPlanOptionsAsync(subscriptionId, countryId);
            return Ok(options);
        }

        [HttpPost]
        public async Task<ActionResult<SubscriptionResponseDTO>> Subscribe([FromBody] CreateSubscriptionDTO subscriptionDTO, [FromQuery] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _subscriptionService.SubscribeAsync(userId, subscriptionDTO);

            if (!result.Success)
                return BadRequest(result);

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
            var result = await _subscriptionService.CancelSubscriptionAsync(userId);
            if (!result)
                return NotFound(new { message = "No active subscription to cancel." });

            return Ok(new { message = "Subscription cancelled successfully." });
        }
    }
}
