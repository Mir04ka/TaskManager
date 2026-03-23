using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManager.API.Services;
using Xunit;

namespace TaskManager.Tests.API.Services;

public class JwtServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Jwt:Key"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configurationMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        _jwtService = new JwtService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be("TestIssuer");
        jwtToken.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateToken_ShouldIncludeUserIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateToken_ShouldIncludeUsernameClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var usernameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
        usernameClaim.Should().NotBeNull();
        usernameClaim!.Value.Should().Be(username);
    }

    [Fact]
    public void GenerateToken_ShouldSetExpirationTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddHours(2));
    }

    [Fact]
    public void GenerateToken_ShouldGenerateDifferentTokensForDifferentUsers()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        // Act
        var token1 = _jwtService.GenerateToken(user1Id, "user1");
        var token2 = _jwtService.GenerateToken(user2Id, "user2");

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeJtiClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var username = "testuser";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        jtiClaim.Should().NotBeNull();
        Guid.TryParse(jtiClaim!.Value, out _).Should().BeTrue();
    }
}