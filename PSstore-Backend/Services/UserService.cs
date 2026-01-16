using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using BCrypt.Net;

namespace PSstore.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IJwtService jwtService, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user != null ? MapToUserDTO(user) : null;
        }

        public async Task<UserDTO?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null ? MapToUserDTO(user) : null;
        }

        public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(createUserDTO.UserEmail);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already registered.");
            }

            // Hash password using BCrypt (12 rounds for security)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(createUserDTO.UserPassword, workFactor: 12);

            var user = new User
            {
                UserName = createUserDTO.UserName,
                UserEmail = createUserDTO.UserEmail,
                UserPassword = hashedPassword,
                Age = createUserDTO.Age,
                CountryId = createUserDTO.CountryId,
                SubscriptionStatus = null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return MapToUserDTO(user);
        }

        public async Task<UserDTO?> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDTO)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            user.UserName = updateUserDTO.UserName;
            user.Age = updateUserDTO.Age;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return MapToUserDTO(user);
        }

        public async Task<bool> SoftDeleteUserAsync(Guid userId)
        {
            await _userRepository.SoftDeleteAsync(userId);
            return true;
        }

        public async Task<bool> RestoreUserAsync(Guid userId)
        {
            return await _userRepository.RestoreAsync(userId);
        }

        public async Task<LoginResponseDTO?> LoginAsync(LoginDTO loginDTO)
        {
            var user = await _userRepository.GetByEmailAsync(loginDTO.UserEmail);
            
            // Verify user exists and password matches using BCrypt
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.UserPassword, user.UserPassword))
            {
                return null;
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.UserId, "User");
            var expirationMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpirationMinutes"]);

            return new LoginResponseDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserEmail = user.UserEmail,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes)
            };
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Verify old password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.UserPassword))
            {
                return false;
            }

            // Hash new password
            user.UserPassword = BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return true;
        }

        private static UserDTO MapToUserDTO(User user)
        {
            return new UserDTO
            {
                UserId = user.UserId,
                UserName = user.UserName,
                UserEmail = user.UserEmail,
                Age = user.Age,
                SubscriptionStatus = !string.IsNullOrEmpty(user.SubscriptionStatus),
                CreatedAt = user.CreatedAt,
                CountryId = user.CountryId
            };
        }
    }
}
