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
        private readonly ICountryRepository _countryRepository;
        private readonly IGameSubscriptionRepository _gameSubscriptionRepository;

        public SubscriptionService(
            IUserSubscriptionPlanRepository userSubscriptionRepository,
            ISubscriptionPlanRepository subscriptionPlanRepository,
            ISubscriptionPlanCountryRepository planCountryRepository,
            IUserRepository userRepository,
            ICountryRepository countryRepository,
            IGameSubscriptionRepository gameSubscriptionRepository)
        {
            _userSubscriptionRepository = userSubscriptionRepository;
            _subscriptionPlanRepository = subscriptionPlanRepository;
            _planCountryRepository = planCountryRepository;
            _userRepository = userRepository;
            _countryRepository = countryRepository;
            _gameSubscriptionRepository = gameSubscriptionRepository;
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



            // Update user subscription status - REMOVED (Calc dynamically)
            
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
                IsActive = subscription.PlanEndDate >= DateTime.UtcNow,
                SubscriptionName = subscription.SubscriptionPlanCountry?.SubscriptionPlan?.SubscriptionType ?? "Unknown"
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
            var plans = await _subscriptionPlanRepository.GetAllPlansWithDetailsAsync();
            
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

            // Update user subscription status - REMOVED (Calc dynamically)

            await _userSubscriptionRepository.SaveChangesAsync();
            return true;
        }
        // Admin Methods
        public async Task<SubscriptionPlanDTO> CreateSubscriptionPlanAsync(CreatePlanDTO createPlanDTO)
        {
            var plan = new SubscriptionPlan
            {
                SubscriptionId = Guid.NewGuid(),
                SubscriptionType = createPlanDTO.SubscriptionType,
                UpdatedAt = DateTime.UtcNow
            };

            await _subscriptionPlanRepository.AddAsync(plan);
            await _subscriptionPlanRepository.SaveChangesAsync();

            return new SubscriptionPlanDTO
            {
                SubscriptionId = plan.SubscriptionId,
                SubscriptionName = plan.SubscriptionType,
                IncludedGames = new List<string>()
            };
        }

        public async Task<SubscriptionPlanDTO?> UpdateSubscriptionPlanAsync(Guid subscriptionId, UpdatePlanDTO updatePlanDTO)
        {
            var plan = await _subscriptionPlanRepository.GetByIdAsync(subscriptionId);
            if (plan == null) return null;

            plan.SubscriptionType = updatePlanDTO.SubscriptionType;
            plan.UpdatedAt = DateTime.UtcNow;

            _subscriptionPlanRepository.Update(plan);
            await _subscriptionPlanRepository.SaveChangesAsync();

            return new SubscriptionPlanDTO
            {
                SubscriptionId = plan.SubscriptionId,
                SubscriptionName = plan.SubscriptionType,
                IncludedGames = await GetIncludedGamesForPlan(subscriptionId) // Helper or just empty? The original repo call includes logic.
            };
        }

        public async Task<bool> DeleteSubscriptionPlanAsync(Guid subscriptionId)
        {
            var plan = await _subscriptionPlanRepository.GetByIdAsync(subscriptionId);
            if (plan == null) return false;

            _subscriptionPlanRepository.Remove(plan);
            await _subscriptionPlanRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SubscriptionPricingDTO>> GetSubscriptionPricingAsync(Guid subscriptionId)
        {
            // Use the child repository directly to avoid issues with parent inclusion
            var planCountries = await _planCountryRepository.GetBySubscriptionIdAsync(subscriptionId);
            
            return planCountries.Select(spc => new SubscriptionPricingDTO
            {
                PlanCountryId = spc.SubscriptionPlanCountryId,
                CountryId = spc.CountryId,
                CountryName = spc.Country.CountryName,
                Currency = spc.Country.Currency,
                DurationMonths = spc.DurationMonths,
                Price = spc.Price
            });
        }

        public async Task<SubscriptionPricingDTO> UpdateSubscriptionPriceAsync(Guid planCountryId, decimal newPrice)
        {
            var spc = await _planCountryRepository.GetPlanCountryDetailsAsync(planCountryId);
            if (spc == null) throw new KeyNotFoundException("Pricing option not found.");

            spc.Price = newPrice;
            _planCountryRepository.Update(spc);
            await _planCountryRepository.SaveChangesAsync();

            return new SubscriptionPricingDTO
            {
                PlanCountryId = spc.SubscriptionPlanCountryId,
                CountryId = spc.CountryId,
                CountryName = spc.Country.CountryName,
                Currency = spc.Country.Currency,
                DurationMonths = spc.DurationMonths,
                Price = spc.Price
            };
        }

        public async Task<SubscriptionPricingDTO> AddSubscriptionPriceAsync(CreatePlanPricingDTO pricingDTO)
        {
            // Resolve CountryId if only Name is provided
            Guid targetCountryId;

            if (pricingDTO.CountryId.HasValue && pricingDTO.CountryId != Guid.Empty)
            {
                targetCountryId = pricingDTO.CountryId.Value;
            }
            else if (!string.IsNullOrWhiteSpace(pricingDTO.CountryName))
            {
                var country = await _countryRepository.GetByNameAsync(pricingDTO.CountryName);
                if (country == null) throw new KeyNotFoundException($"Country '{pricingDTO.CountryName}' not found.");
                targetCountryId = country.CountryId;
            }
            else
            {
                throw new ArgumentException("Either CountryId or CountryName must be provided.");
            }

            // Verify if exists
            var existing = (await _planCountryRepository.GetPlansByCountryAsync(targetCountryId))
                           .FirstOrDefault(p => p.SubscriptionId == pricingDTO.SubscriptionId && p.DurationMonths == pricingDTO.DurationMonths);

            if (existing != null) throw new InvalidOperationException("Price for this duration already exists in this region.");

            // Create new pricing
            var newSpc = new SubscriptionPlanCountry
            {
                SubscriptionPlanCountryId = Guid.NewGuid(),
                SubscriptionId = pricingDTO.SubscriptionId,
                CountryId = targetCountryId,
                DurationMonths = pricingDTO.DurationMonths,
                Price = pricingDTO.Price
            };

            await _planCountryRepository.AddAsync(newSpc);
            await _planCountryRepository.SaveChangesAsync();

            // Re-fetch to get country details for DTO
            var created = await _planCountryRepository.GetPlanCountryDetailsAsync(newSpc.SubscriptionPlanCountryId);
            
            return new SubscriptionPricingDTO
            {
                PlanCountryId = created!.SubscriptionPlanCountryId,
                CountryId = created.CountryId,
                CountryName = created.Country.CountryName,
                Currency = created.Country.Currency,
                DurationMonths = created.DurationMonths,
                Price = created.Price
            };
        }


        private async Task<List<string>> GetIncludedGamesForPlan(Guid subscriptionId)
        {
            var games = await _gameSubscriptionRepository.GetBySubscriptionIdAsync(subscriptionId);
            return games.Select(g => g.GameId.ToString()).ToList();
        }

        public async Task<bool> AddGameToSubscriptionAsync(Guid subscriptionId, Guid gameId)
        {
            if (await _gameSubscriptionRepository.ExistsAsync(subscriptionId, gameId))
                return false; // Already exists

            var link = new GameSubscription
            {
                SubscriptionId = subscriptionId,
                GameId = gameId
            };

            await _gameSubscriptionRepository.AddAsync(link);
            await _gameSubscriptionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveGameFromSubscriptionAsync(Guid subscriptionId, Guid gameId)
        {
            var link = await _gameSubscriptionRepository.GetAsync(subscriptionId, gameId);
            if (link == null) return false;

            _gameSubscriptionRepository.Remove(link);
            await _gameSubscriptionRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<GameDTO>> GetIncludedGamesAsync(Guid subscriptionId)
        {
            var links = await _gameSubscriptionRepository.GetBySubscriptionIdAsync(subscriptionId);
            return links.Select(l => new GameDTO 
            {
                GameId = l.Game.GameId,
                GameName = l.Game.GameName,
                PublishedBy = l.Game.PublishedBy,
                ReleaseDate = l.Game.ReleaseDate,
                FreeToPlay = l.Game.FreeToPlay,
                IsMultiplayer = l.Game.IsMultiplayer,
                Price = l.Game.BasePrice ?? 0,
                Categories = l.Game.GameCategories.Select(gc => gc.Category.CategoryName).ToList() ?? new List<string>(),
                AvailableInPlans = new List<string>() // Not filled here
            });
        }
    }
}
