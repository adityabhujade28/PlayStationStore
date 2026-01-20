using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using PSstore.Services;
using System.IdentityModel.Tokens.Jwt;

namespace PSstore_Backend.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            _mockConfiguration = new Mock<IConfiguration>();

            
            _mockConfiguration.Setup(c => c["JwtSettings:SecretKey"]).Returns("ThisIsASecretKeyForTestingPurposesOnly123!"); 
            _mockConfiguration.Setup(c => c["JwtSettings:Issuer"]).Returns("TestIssuer");
            _mockConfiguration.Setup(c => c["JwtSettings:Audience"]).Returns("TestAudience");
            _mockConfiguration.Setup(c => c["JwtSettings:ExpirationMinutes"]).Returns("60");

            _jwtService = new JwtService(_mockConfiguration.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidToken_WhenInputsAreValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = "Admin";

            // Act
            var token = _jwtService.GenerateToken(userId, role);

            // Assert
            token.Should().NotBeNullOrEmpty();
            
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Issuer.Should().Be("TestIssuer");
            jwtToken.Audiences.Should().Contain("TestAudience");
        }

        [Fact]
        public void ValidateToken_ShouldReturnUserId_WhenTokenIsValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var role = "Admin";
            var token = _jwtService.GenerateToken(userId, role);

            // Act
            var result = _jwtService.ValidateToken(token);

            // Assert
            result.Should().Be(userId);
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsExpired()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["JwtSettings:ExpirationMinutes"])
                              .Returns("-1");

            var jwtService = new JwtService(_mockConfiguration.Object);
            var token = jwtService.GenerateToken(Guid.NewGuid(), "User");

            // Act
            var result = jwtService.ValidateToken(token);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsExpiredforAdmin()
        {
            // Arrange
            _mockConfiguration.Setup(c => c["JwtSettings:ExpirationMinutes"])
                              .Returns("-1");

            var jwtService = new JwtService(_mockConfiguration.Object);
            var token = jwtService.GenerateToken(Guid.NewGuid(), "Admin");

            // Act
            var result = jwtService.ValidateToken(token);

            // Assert
            result.Should().BeNull();
        }


        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
        {
            // Arrange
            var invalidToken = "invalid.token.string%^%^&";

            // Act
            var result = _jwtService.ValidateToken(invalidToken);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ShouldReturnNull_WhenTokenIsEmpty()
        {
            // Act
            var result = _jwtService.ValidateToken("");

            // Assert
            result.Should().BeNull();
        }
    }
}
