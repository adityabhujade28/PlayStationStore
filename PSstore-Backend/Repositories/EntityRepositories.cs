using Microsoft.EntityFrameworkCore;
using PSstore.Data;
using PSstore.Interfaces;
using PSstore.Models;

namespace PSstore.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.UserEmail == email);
        }

        public async Task<User?> GetUserWithRegionAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.Country)
                    .ThenInclude(c => c.Region)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithPurchasesAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.UserPurchasedGames)
                    .ThenInclude(upg => upg.Game)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithSubscriptionsAsync(int userId)
        {
            return await _dbSet
                .Include(u => u.UserSubscriptionPlans)
                    .ThenInclude(usp => usp.SubscriptionPlanCountry)
                        .ThenInclude(spc => spc.SubscriptionPlan)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(u => u.UserEmail == email);
        }

        public async Task SoftDeleteAsync(int userId)
        {
            var user = await _dbSet.FindAsync(userId);
            if (user != null)
            {
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int userId)
        {
            var user = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserId == userId);
            if (user != null && user.IsDeleted)
            {
                user.IsDeleted = false;
                user.DeletedAt = null;
                await SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

    public class GameRepository : BaseRepository<Game>, IGameRepository
    {
        public GameRepository(AppDbContext context) : base(context) { }

        public async Task<Game?> GetGameWithCategoriesAsync(int gameId)
        {
            return await _dbSet
                .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                .FirstOrDefaultAsync(g => g.GameId == gameId);
        }

        public async Task<Game?> GetGameWithSubscriptionsAsync(int gameId)
        {
            return await _dbSet
                .Include(g => g.GameSubscriptions)
                    .ThenInclude(gs => gs.SubscriptionPlan)
                .FirstOrDefaultAsync(g => g.GameId == gameId);
        }

        public async Task<IEnumerable<Game>> GetGamesByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(g => g.GameCategories)
                .Where(g => g.GameCategories.Any(gc => gc.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetFreeGamesAsync()
        {
            return await _dbSet.Where(g => g.FreeToPlay).ToListAsync();
        }

        public async Task<IEnumerable<Game>> SearchGamesAsync(string searchTerm)
        {
            return await _dbSet
                .Where(g => g.GameName.Contains(searchTerm) || 
                           (g.PublishedBy != null && g.PublishedBy.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task SoftDeleteAsync(int gameId)
        {
            var game = await _dbSet.FindAsync(gameId);
            if (game != null)
            {
                game.IsDeleted = true;
                game.DeletedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int gameId)
        {
            var game = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(g => g.GameId == gameId);
            if (game != null && game.IsDeleted)
            {
                game.IsDeleted = false;
                game.DeletedAt = null;
                await SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<IEnumerable<Game>> GetAllIncludingDeletedAsync()
        {
            return await _dbSet.IgnoreQueryFilters().ToListAsync();
        }
    }

    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context) { }

        public async Task<Category?> GetCategoryWithGamesAsync(int categoryId)
        {
            return await _dbSet
                .Include(c => c.GameCategories)
                    .ThenInclude(gc => gc.Game)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<bool> CategoryNameExistsAsync(string categoryName)
        {
            return await _dbSet.AnyAsync(c => c.CategoryName == categoryName);
        }

        public async Task SoftDeleteAsync(int categoryId)
        {
            var category = await _dbSet.FindAsync(categoryId);
            if (category != null)
            {
                category.IsDeleted = true;
                category.DeletedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int categoryId)
        {
            var category = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category != null && category.IsDeleted)
            {
                category.IsDeleted = false;
                category.DeletedAt = null;
                await SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

    public class RegionRepository : BaseRepository<Region>, IRegionRepository
    {
        public RegionRepository(AppDbContext context) : base(context) { }

        public async Task<Region?> GetByCodeAsync(string regionCode)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.RegionCode == regionCode);
        }
    }

    public class UserPurchaseGameRepository : BaseRepository<UserPurchaseGame>, IUserPurchaseGameRepository
    {
        public UserPurchaseGameRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UserPurchaseGame>> GetUserPurchasesAsync(int userId)
        {
            return await _dbSet
                .Include(upg => upg.Game)
                .Where(upg => upg.UserId == userId)
                .OrderByDescending(upg => upg.PurchaseDate)
                .ToListAsync();
        }

        public async Task<UserPurchaseGame?> GetPurchaseDetailsAsync(int purchaseId)
        {
            return await _dbSet
                .Include(upg => upg.Game)
                .Include(upg => upg.User)
                .FirstOrDefaultAsync(upg => upg.PurchaseId == purchaseId);
        }

        public async Task<bool> HasUserPurchasedGameAsync(int userId, int gameId)
        {
            return await _dbSet.AnyAsync(upg => upg.UserId == userId && upg.GameId == gameId);
        }
    }

    public class SubscriptionPlanRepository : BaseRepository<SubscriptionPlan>, ISubscriptionPlanRepository
    {
        public SubscriptionPlanRepository(AppDbContext context) : base(context) { }

        public async Task<SubscriptionPlan?> GetPlanWithRegionsAsync(int planId)
        {
            return await _dbSet
                .Include(sp => sp.SubscriptionPlanCountries)
                    .ThenInclude(spc => spc.Country)
                .FirstOrDefaultAsync(sp => sp.SubscriptionId == planId);
        }

        public async Task<SubscriptionPlan?> GetPlanWithGamesAsync(int planId)
        {
            return await _dbSet
                .Include(sp => sp.GameSubscriptions)
                    .ThenInclude(gs => gs.Game)
                .FirstOrDefaultAsync(sp => sp.SubscriptionId == planId);
        }

        public async Task<IEnumerable<SubscriptionPlan>> GetAllPlansWithDetailsAsync()
        {
            return await _dbSet
                .Include(sp => sp.SubscriptionPlanCountries)
                    .ThenInclude(spc => spc.Country)
                .Include(sp => sp.GameSubscriptions)
                    .ThenInclude(gs => gs.Game)
                .ToListAsync();
        }
    }

public class SubscriptionPlanCountryRepository : BaseRepository<SubscriptionPlanCountry>, ISubscriptionPlanCountryRepository
{
    public SubscriptionPlanCountryRepository(AppDbContext context) : base(context) { }

    public async Task<SubscriptionPlanCountry?> GetPlanCountryDetailsAsync(int planCountryId)
    {
        return await _dbSet
            .Include(spc => spc.SubscriptionPlan)
            .Include(spc => spc.Country)
            .FirstOrDefaultAsync(spc => spc.SubscriptionPlanCountryId == planCountryId);
    }

    public async Task<IEnumerable<SubscriptionPlanCountry>> GetPlansByCountryAsync(int countryId)
    {
        return await _dbSet
            .Include(spc => spc.SubscriptionPlan)
            .Where(spc => spc.CountryId == countryId)
                .ToListAsync();
        }
    }

    public class UserSubscriptionPlanRepository : BaseRepository<UserSubscriptionPlan>, IUserSubscriptionPlanRepository
    {
        public UserSubscriptionPlanRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<UserSubscriptionPlan>> GetUserSubscriptionsAsync(int userId)
        {
            return await _dbSet
                .Include(usp => usp.SubscriptionPlanCountry)
                    .ThenInclude(spc => spc.SubscriptionPlan)
                .Include(usp => usp.SubscriptionPlanCountry)
                    .ThenInclude(spc => spc.Country)
                .Where(usp => usp.UserId == userId)
                .OrderByDescending(usp => usp.PlanStartDate)
                .ToListAsync();
        }

        public async Task<UserSubscriptionPlan?> GetActiveSubscriptionAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Include(usp => usp.SubscriptionPlanCountry)
                    .ThenInclude(spc => spc.SubscriptionPlan)
                .FirstOrDefaultAsync(usp => usp.UserId == userId && 
                                           usp.PlanStartDate <= now && 
                                           usp.PlanEndDate >= now);
        }

        public async Task<bool> HasActiveSubscriptionAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet.AnyAsync(usp => usp.UserId == userId && 
                                               usp.PlanStartDate <= now && 
                                               usp.PlanEndDate >= now);
        }

        public async Task<IEnumerable<UserSubscriptionPlan>> GetExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(usp => usp.PlanEndDate < now)
                .ToListAsync();
        }
    }

    public class CartRepository : BaseRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context) { }

        public async Task<Cart?> GetUserCartAsync(int userId)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetCartWithItemsAsync(int cartId)
        {
            return await _dbSet
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Game)
                .FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task ClearCartAsync(int cartId)
        {
            var cart = await GetCartWithItemsAsync(cartId);
            if (cart != null)
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                await SaveChangesAsync();
            }
        }
    }

    public class CartItemRepository : BaseRepository<CartItem>, ICartItemRepository
    {
        public CartItemRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<CartItem>> GetCartItemsAsync(int cartId)
        {
            return await _dbSet
                .Include(ci => ci.Game)
                .Where(ci => ci.CartId == cartId)
                .ToListAsync();
        }

        public async Task<CartItem?> GetCartItemWithGameAsync(int cartItemId)
        {
            return await _dbSet
                .Include(ci => ci.Game)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<bool> IsGameInCartAsync(int cartId, int gameId)
        {
            return await _dbSet.AnyAsync(ci => ci.CartId == cartId && ci.GameId == gameId);
        }
    }

    public class AdminRepository : BaseRepository<Admin>, IAdminRepository
    {
        public AdminRepository(AppDbContext context) : base(context) { }

        public async Task<Admin?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(a => a.AdminEmail == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _dbSet.AnyAsync(a => a.AdminEmail == email);
        }

        public async Task SoftDeleteAsync(int adminId)
        {
            var admin = await _dbSet.FindAsync(adminId);
            if (admin != null)
            {
                admin.IsDeleted = true;
                admin.DeletedAt = DateTime.UtcNow;
                await SaveChangesAsync();
            }
        }

        public async Task<bool> RestoreAsync(int adminId)
        {
            var admin = await _dbSet.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.AdminId == adminId);
            if (admin != null && admin.IsDeleted)
            {
                admin.IsDeleted = false;
                admin.DeletedAt = null;
                await SaveChangesAsync();
                return true;
            }
            return false;
        }
    }

    // Country Repository
    public class CountryRepository : BaseRepository<Country>, ICountryRepository
    {
        public CountryRepository(AppDbContext context) : base(context) { }

        public async Task<Country?> GetByCodeAsync(string countryCode)
        {
            return await _dbSet
                .Include(c => c.Region)
                .FirstOrDefaultAsync(c => c.CountryCode == countryCode);
        }

        public async Task<IEnumerable<Country>> GetCountriesByRegionAsync(int regionId)
        {
            return await _dbSet
                .Where(c => c.RegionId == regionId)
                .ToListAsync();
        }
    }

    // GameCountry Repository
    public class GameCountryRepository : BaseRepository<GameCountry>, IGameCountryRepository
    {
        public GameCountryRepository(AppDbContext context) : base(context) { }

        public async Task<GameCountry?> GetGamePricingAsync(int gameId, int countryId)
        {
            return await _dbSet
                .Include(gc => gc.Game)
                .Include(gc => gc.Country)
                .FirstOrDefaultAsync(gc => gc.GameId == gameId && gc.CountryId == countryId);
        }

        public async Task<IEnumerable<GameCountry>> GetGamePricesByCountryAsync(int countryId)
        {
            return await _dbSet
                .Include(gc => gc.Game)
                .Where(gc => gc.CountryId == countryId)
                .ToListAsync();
        }
    }
}
