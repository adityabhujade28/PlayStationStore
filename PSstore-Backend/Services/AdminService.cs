using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using BCrypt.Net;

namespace PSstore.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IUserSubscriptionPlanRepository _subscriptionRepository;

        public AdminService(
            IAdminRepository adminRepository, 
            IJwtService jwtService, 
            IConfiguration configuration,
            IUserRepository userRepository,
            IGameRepository gameRepository,
            IUserSubscriptionPlanRepository subscriptionRepository)
        {
            _adminRepository = adminRepository;
            _jwtService = jwtService;
            _configuration = configuration;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDTO)
        {
            var admin = await _adminRepository.GetByEmailAsync(loginDTO.UserEmail);

            // Verify admin exists and password matches using BCrypt
            if (admin == null || !BCrypt.Net.BCrypt.Verify(loginDTO.UserPassword, admin.AdminPassword))
            {
                return null;
            }

            // Generate JWT token with "Admin" role
            var token = _jwtService.GenerateToken(admin.AdminId, "Admin");
            var expirationMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"]);

            return new LoginResponseDTO
            {
                UserId = admin.AdminId,
                UserName = admin.AdminEmail, // Using Email as Name
                UserEmail = admin.AdminEmail,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }

        public async Task<AdminDTO?> GetAdminByIdAsync(Guid adminId)
        {
            var admin = await _adminRepository.GetByIdAsync(adminId);
            if (admin == null) return null;

            return MapToAdminDTO(admin);
        }

        public async Task<AdminDTO?> GetAdminByEmailAsync(string email)
        {
            var admin = await _adminRepository.GetByEmailAsync(email);
            if (admin == null) return null;

            return MapToAdminDTO(admin);
        }

        private static AdminDTO MapToAdminDTO(Admin admin)
        {
            return new AdminDTO
            {
                AdminId = admin.AdminId,
                AdminName = admin.AdminEmail, // Using Email as Name
                AdminEmail = admin.AdminEmail,
                CreatedAt = admin.CreatedAt
            };
        }

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            return new DashboardStatsDTO
            {
                TotalUsers = await _userRepository.CountAsync(u => !u.IsDeleted), 
                TotalGames = await _gameRepository.CountAsync(g => !g.IsDeleted),
                ActiveSubscriptions = await _subscriptionRepository.CountActiveSubscriptionsAsync()
            };
        }
    }
}
