using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly IUserSubscriptionPlanRepository _userSubscriptionRepository;
        private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
        private readonly ISubscriptionPlanCountryRepository _planCountryRepository;
        private readonly IUserRepository _userRepository;

        public SubscriptionService(
            IUserSubscriptionPlanRepository userSubscriptionRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository,
            ISubscriptionPlanCountryRepository planCountryRepository,
            IUserRepository userRepository)
        {
            _userSubscriptionRepository = userSubscriptionRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _planCountryRepository = planCountryRepository;
            _userRepository = userRepository;
        }

        public async Task<SubscriptionResponseDTO> SubscribeAsync(Guid userId, CreateSubscriptionDTO subscriptionDTO)
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

            // Validate subscription plan country exists
            var planCountry = await _planCountryRepository.GetByIdAsync(subscriptionDTO.SubscriptionPlanCountryId);
            if (planCountry == null)
            {
                return new SubscriptionResponseDTO
                {
                    Success = false,
                    Message = "Subscription plan not found."
                };
            }

            // Create subscription record
            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths(planCountry.DurationMonths);

            var userSubscription = new UserSubscriptionPlan
            {
                UserId = userId,
                SubscriptionPlanCountryId = subscriptionDTO.SubscriptionPlanCountryId,
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
                DurationMonths = planCountry.DurationMonths,
                Price = planCountry.Price
            };
        }

        public async Task<UserSubscriptionDTO?> GetActiveSubscriptionAsync(Guid userId)
        {
            var subscription = await _userSubscriptionRepository.GetActiveSubscriptionAsync(userId);
            if (subscription == null) return null;

            return new UserSubscriptionDTO
            {
                UserSubscriptionId = subscription.UserSubscriptionId,
                UserId = subscription.UserId,
                SubscriptionPlanCountryId = subscription.SubscriptionPlanCountryId,
                PlanStartDate = subscription.PlanStartDate,
                PlanEndDate = subscription.PlanEndDate,
                IsActive = subscription.PlanEndDate >= DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<UserSubscriptionDTO>> GetUserSubscriptionHistoryAsync(Guid userId)
        {
            var subscriptions = await _userSubscriptionRepository.GetUserSubscriptionsAsync(userId);
            
            return subscriptions.Select(s => new UserSubscriptionDTO
            {
                UserSubscriptionId = s.UserSubscriptionId,
                UserId = s.UserId,
                SubscriptionPlanCountryId = s.SubscriptionPlanCountryId,
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
                SubscriptionName = p.SubscriptionType,
                IncludedGames = p.GameSubscriptions?.Select(gs => gs.GameId.ToString()).ToList() ?? new List<string>()
            });
        }

        public async Task<IEnumerable<SubscriptionPlanCountryDTO>> GetSubscriptionPlanOptionsAsync(Guid subscriptionId, Guid countryId)
        {
            var options = await _planCountryRepository.GetPlansByCountryAsync(countryId);
            var filtered = options.Where(o => o.SubscriptionId == subscriptionId);
            
            return filtered.Select(o => new SubscriptionPlanCountryDTO
            {
                SubscriptionPlanCountryId = o.SubscriptionPlanCountryId,
                SubscriptionId = o.SubscriptionId,
                CountryId = o.CountryId,
                DurationMonths = o.DurationMonths,
                Price = o.Price
            });
        }

        public async Task<bool> CancelSubscriptionAsync(Guid userId)
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
