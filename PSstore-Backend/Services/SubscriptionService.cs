using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUserSubscriptionPlanRepository _userSubscriptionRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly ISubscriptionPlanRegionRepository _planRegionRepository;
        private readonly IUserRepository _userRepository;

        public SubscriptionService(
            IUserSubscriptionPlanRepository userSubscriptionRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository,
            ISubscriptionPlanRegionRepository planRegionRepository,
            IUserRepository userRepository)
        {
            _userSubscriptionRepository = userSubscriptionRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _planRegionRepository = planRegionRepository;
            _userRepository = userRepository;
        }

        public async Task<SubscriptionResponseDTO> SubscribeAsync(int userId, CreateSubscriptionDTO subscriptionDTO)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new SubscriptionResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            // Check if user already has an active subscription
            var activeSubscription = await _userSubscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (activeSubscription != null)
            {
                return new SubscriptionResponseDTO
                {
                    Success = false,
                    Message = $"You already have an active subscription that expires on {activeSubscription.PlanEndDate:yyyy-MM-dd}."
                };
            }

            // Validate subscription plan region exists
            var planRegion = await _planRegionRepository.GetByIdAsync(subscriptionDTO.SubscriptionPlanRegionId);
            if (planRegion == null)
            {
                return new SubscriptionResponseDTO
                {
                    Success = false,
                    Message = "Subscription plan not found."
                };
            }

            // Create subscription record
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths(planRegion.DurationMonths);

            var userSubscription = new UserSubscriptionPlan
            {
                UserId = userId,
                SubscriptionPlanRegionId = subscriptionDTO.SubscriptionPlanRegionId,
                PlanStartDate = startDate,
                PlanEndDate = endDate
            };

            await _userSubscriptionRepository.AddAsync(userSubscription);

            // Update user subscription status
            user.SubscriptionStatus = "Active";
            _userRepository.Update(user);

            await _userSubscriptionRepository.SaveChangesAsync();

            return new SubscriptionResponseDTO
            {
                Success = true,
                Message = "Subscription activated successfully!",
                UserSubscriptionId = userSubscription.UserSubscriptionId,
                PlanStartDate = userSubscription.PlanStartDate,
                PlanEndDate = userSubscription.PlanEndDate,
                DurationMonths = planRegion.DurationMonths,
                Price = planRegion.Price
            };
        }

        public async Task<UserSubscriptionDTO?> GetActiveSubscriptionAsync(int userId)
        {
            var subscription = await _userSubscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (subscription == null) return null;

            return new UserSubscriptionDTO
            {
                UserSubscriptionId = subscription.UserSubscriptionId,
                UserId = subscription.UserId,
                SubscriptionPlanRegionId = subscription.SubscriptionPlanRegionId,
                PlanStartDate = subscription.PlanStartDate,
                PlanEndDate = subscription.PlanEndDate,
                IsActive = subscription.PlanEndDate >= DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<UserSubscriptionDTO>> GetUserSubscriptionHistoryAsync(int userId)
        {
            var subscriptions = await _userSubscriptionRepository.GetUserSubscriptionsAsync(userId);
            
            return subscriptions.Select(s => new UserSubscriptionDTO
            {
                UserSubscriptionId = s.UserSubscriptionId,
                UserId = s.UserId,
                SubscriptionPlanRegionId = s.SubscriptionPlanRegionId,
                PlanStartDate = s.PlanStartDate,
                PlanEndDate = s.PlanEndDate,
                IsActive = s.PlanEndDate >= DateTime.UtcNow
            });
        }

        public async Task<IEnumerable<SubscriptionPlanDTO>> GetAllSubscriptionPlansAsync()
        {
            var plans = await _subscriptionPlanRepository.GetAllAsync();
            
            return plans.Select(p => new SubscriptionPlanDTO
            {
                SubscriptionId = p.SubscriptionId,
                SubscriptionName = p.SubscriptionType
            });
        }

        public async Task<IEnumerable<SubscriptionPlanRegionDTO>> GetSubscriptionPlanOptionsAsync(int subscriptionId, int regionId)
        {
            var options = await _planRegionRepository.GetPlansByRegionAsync(regionId);
            var filtered = options.Where(o => o.SubscriptionId == subscriptionId);
            
            return filtered.Select(o => new SubscriptionPlanRegionDTO
            {
                SubscriptionPlanRegionId = o.SubscriptionPlanRegionId,
                SubscriptionId = o.SubscriptionId,
                RegionId = o.RegionId,
                DurationMonths = o.DurationMonths,
                Price = o.Price,
                Currency = o.Currency
            });
        }

        public async Task<bool> CancelSubscriptionAsync(int userId)
        {
            var activeSubscription = await _userSubscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (activeSubscription == null) return false;

            // Set end date to now (effectively canceling it)
            activeSubscription.PlanEndDate = DateTime.UtcNow;
            _userSubscriptionRepository.Update(activeSubscription);

            // Update user subscription status
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.SubscriptionStatus = null;
                _userRepository.Update(user);
            }

            await _userSubscriptionRepository.SaveChangesAsync();
            return true;
        }
    }
}
