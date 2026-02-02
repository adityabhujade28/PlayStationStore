using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PSstore.DTOs;
using PSstore.Models;
using PSstore.Services;
using PSstore_Backend.Tests.Helpers;

namespace PSstore_Backend.Tests.Services
{
    public class UserServiceTests : IntegrationTestBase
    {
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Create the service with real repositories and mocked external dependencies
            _userService = new UserService(
                UserRepository,
                MockJwtService.Object,
                MockConfiguration.Object,
                UserSubscriptionPlanRepository
            );
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenEmailIsUnique()
        {
            // Arrange
            var createUserDto = new CreateUserDTO
            {
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = Faker.Internet.Password(),
                Age = Faker.Random.Int(18, 65),
                CountryId = Guid.NewGuid()
            };

            // Act
            var result = await _userService.CreateUserAsync(createUserDto);

            // Assert
            result.Should().NotBeNull();
            result.UserEmail.Should().Be(createUserDto.UserEmail);
            result.UserName.Should().Be(createUserDto.UserName);
            result.Age.Should().Be(createUserDto.Age);
            result.IsDeleted.Should().BeFalse();
            result.SubscriptionStatus.Should().BeFalse();

            var savedUser = await Context.Users.FirstOrDefaultAsync(u => u.UserEmail == createUserDto.UserEmail);
            savedUser.Should().NotBeNull();
            savedUser!.UserPassword.Should().NotBe(createUserDto.UserPassword);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var email = Faker.Internet.Email();
            var existingUser = new User
            {
                UserId = Guid.NewGuid(),
                UserName = Faker.Internet.UserName(),
                UserEmail = email,
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password123"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            
            await Context.Users.AddAsync(existingUser);
            await Context.SaveChangesAsync();

            var createUserDto = new CreateUserDTO
            {
                UserName = Faker.Internet.UserName(),
                UserEmail = email, // Same email
                UserPassword = Faker.Internet.Password(),
                Age = 30,
                CountryId = Guid.NewGuid()
            };

            // Act
            var act = async () => await _userService.CreateUserAsync(createUserDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already registered.");
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password123"),
                Age = Faker.Random.Int(18, 65),
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };
            
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            var result = await _userService.GetUserByIdAsync(user.UserId);

            // Assert
            result.Should().NotBeNull();
            result!.UserId.Should().Be(user.UserId);
            result.UserName.Should().Be(user.UserName);
            result.UserEmail.Should().Be(user.UserEmail);
            result.Age.Should().Be(user.Age);
            result.SubscriptionStatus.Should().BeFalse();
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "SecurePassword123!";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserEmail = "valid@example.com",
                UserPassword = hashedPassword,
                UserName = "ValidUser",
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            MockJwtService.Setup(jwt => jwt.GenerateToken(user.UserId, "User"))
                .Returns("generated_jwt_token");

            var loginDTO = new LoginDTO { UserEmail = "valid@example.com", UserPassword = password };

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
            var user = new User
            {
                UserId = Guid.NewGuid(),
                UserEmail = "valid@example.com",
                UserPassword = hashedPassword,
                UserName = "ValidUser",
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            var loginDTO = new LoginDTO { UserEmail = "valid@example.com", UserPassword = "WrongPassword" };

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

            // Act
            var result = await _userService.LoginAsync(loginDTO);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateUser_WhenValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = "OldName",
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 20,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            var dto = new UpdateUserDTO { UserName = "NewName", Age = 25 };

            // Act
            var result = await _userService.UpdateUserAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result!.UserName.Should().Be("NewName");
            result.Age.Should().Be(25);

            var updated = await Context.Users.FindAsync(userId);
            updated!.UserName.Should().Be("NewName");
            updated.Age.Should().Be(25);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserDTO { UserName = "NewName" };

            // Act
            var result = await _userService.UpdateUserAsync(userId, dto);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteUserAsync_ShouldMarkAsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            var result = await _userService.SoftDeleteUserAsync(userId);

            // Assert
            result.Should().BeTrue();

            var deleted = await Context.Users.IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserId == userId);
            deleted.Should().NotBeNull();
            deleted!.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task RestoreUserAsync_ShouldRestoreDeletedUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                UserId = userId,
                UserName = Faker.Internet.UserName(),
                UserEmail = Faker.Internet.Email(),
                UserPassword = BCrypt.Net.BCrypt.HashPassword("password"),
                Age = 25,
                CountryId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                IsDeleted = true
            };

            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();

            // Act
            var result = await _userService.RestoreUserAsync(userId);

            // Assert
            result.Should().BeTrue();

            var restored = await Context.Users.FindAsync(userId);
            restored.Should().NotBeNull();
            restored!.IsDeleted.Should().BeFalse();
        }
    }
}
