using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PSstore.DTOs;
using PSstore.Interfaces;
using PSstore.Models;
using PSstore.Services;

namespace PSstore_Backend.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IUserSubscriptionPlanRepository> _mockSubscriptionRepository;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockJwtService = new Mock<IJwtService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockSubscriptionRepository = new Mock<IUserSubscriptionPlanRepository>();

            _mockConfiguration.Setup(c => c["JwtSettings:ExpirationMinutes"]).Returns("60");

            _userService = new UserService(
                _mockUserRepository.Object,
                _mockJwtService.Object,
                _mockConfiguration.Object,
                _mockSubscriptionRepository.Object
            );
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = "TestUser",
                UserEmail = "test@example.com",
                Age = 25,
                CountryId = Guid.Parse("20000000-0000-0000-0000-000000000001")
            };

            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync(user);
            _mockSubscriptionRepository.Setup(repo => repo.HasActiveSubscriptionAsync(userId))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(userId);
            result.UserName.Should().Be(user.UserName);
            result.SubscriptionStatus.Should().BeTrue();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _mockUserRepository.Setup(repo => repo.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUser_WhenUserExists()
        {
            var email = "test@example.com";
            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserEmail = email,
                UserName = "TestUser"
            };

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mockSubscriptionRepository.Setup(r => r.HasActiveSubscriptionAsync(user.UserId))
                .ReturnsAsync(false);

            var result = await _userService.GetUserByEmailAsync(email);

            result.Should().NotBeNull();
            result!.UserEmail.Should().Be(email);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldNotReturnUser_WhenUserDoesNotExists()
        {
            var email = "test@example.com";

            _mockUserRepository.Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);


            var result = await _userService.GetUserByEmailAsync(email);

            result.Should().BeNull();
        }


        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO
            {
                UserName = "NewUser",
                UserEmail = "newuser@example.com",
                UserPassword = "Password123!",
                Age = 30,
                CountryId = Guid.Parse("20000000-0000-0000-0000-000000000001")
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(createUserDTO.UserEmail))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.CreateUserAsync(createUserDTO);

            // Assert
            result.Should().NotBeNull();
            result.UserEmail.Should().Be(createUserDTO.UserEmail);
            
            // Verify repository add was called
            _mockUserRepository.Verify(repo => repo.AddAsync(It.Is<User>(u => 
                u.UserEmail == createUserDTO.UserEmail && 
                u.UserName == createUserDTO.UserName &&
                u.UserPassword != createUserDTO.UserPassword // Password should be hashed
            )), Times.Once);

            _mockUserRepository.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var createUserDTO = new CreateUserDTO { UserEmail = "existing@example.com", UserPassword = "password" };
            var existingUser = new User { UserEmail = "existing@example.com" };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(createUserDTO.UserEmail))
                .ReturnsAsync(existingUser);

            // Act
            var act = async () => await _userService.CreateUserAsync(createUserDTO);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already registered.");
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "SecurePassword123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4); // Low work factor for speed in tests
            var loginDTO = new LoginDTO { UserEmail = "valid@example.com", UserPassword = password };
            
            var user = new User 
            { 
                UserId = Guid.NewGuid(), 
                UserEmail = loginDTO.UserEmail, 
                UserPassword = hashedPassword,
                UserName = "ValidUser"
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDTO.UserEmail))
                .ReturnsAsync(user);

            _mockJwtService.Setup(jwt => jwt.GenerateToken(user.UserId, "User"))
                .Returns("generated_jwt_token");

            // Act
            var result = await _userService.LoginAsync(loginDTO);

            // Assert
            result.Should().NotBeNull();
            result!.Token.Should().Be("generated_jwt_token");
            result.UserId.Should().Be(user.UserId);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenPasswordIsInvalid()
        {
            // Arrange
            var password = "CorrectPassword";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
            var loginDTO = new LoginDTO { UserEmail = "valid@example.com", UserPassword = "WrongPassword" };

            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserEmail = loginDTO.UserEmail,
                UserPassword = hashedPassword
            };

            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDTO.UserEmail))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.LoginAsync(loginDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var loginDTO = new LoginDTO { UserEmail = "unknown@example.com", UserPassword = "password" };
            _mockUserRepository.Setup(repo => repo.GetByEmailAsync(loginDTO.UserEmail))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _userService.LoginAsync(loginDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenUserNotFound()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserDTO { UserName = "NewName" };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync((User?)null);
            //Act
            var result = await _userService.UpdateUserAsync(userId, dto);
            //Assert
            result.Should().BeNull();
        }


        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenValid()
        {
            //Arrange
            var userId = Guid.NewGuid();
            var user = new User { UserId = userId, UserName = "OldName", Age = 20 };

            _mockUserRepository.Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mockSubscriptionRepository.Setup(r => r.HasActiveSubscriptionAsync(userId))
                .ReturnsAsync(true);
            //Act
            var dto = new UpdateUserDTO { UserName = "NewName", Age = 25 };

            var result = await _userService.UpdateUserAsync(userId, dto);

            _mockUserRepository.Verify(r => r.Update(user), Times.Once);
            _mockUserRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
            //Assert
            result!.UserName.Should().Be("NewName");
            result.Age.Should().Be(25);
        }

        [Fact]
        public async Task SoftDeleteUserAsync_ShouldCallRepository()
        {
            var userId = Guid.NewGuid();

            var result = await _userService.SoftDeleteUserAsync(userId);

            _mockUserRepository.Verify(r => r.SoftDeleteAsync(userId), Times.Once);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreUserAsync_ShouldReturnRepositoryResult()
        {
            var userId = Guid.NewGuid();

            _mockUserRepository.Setup(r => r.RestoreAsync(userId))
                .ReturnsAsync(true);

            var result = await _userService.RestoreUserAsync(userId);

            result.Should().BeTrue();
        }



    }
}
