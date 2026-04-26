using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using TaskManager.Application.Services;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Tests.Integration;

public class AuthenticationFlowTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly AuthController _controller;

    public AuthenticationFlowTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        var userRepository = new UserRepository(_context);
        var currentUserMock = new Mock<ICurrentUserService>();
        var authService = new AuthService(
            userRepository,
            currentUserMock.Object,
            NullLogger<AuthService>.Instance);

        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["Jwt:Key"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        configMock.Setup(c => c["Jwt:ExpiryMinutes"]).Returns("60");

        var jwtService = new JwtService(configMock.Object);
        var logger = NullLogger<AuthController>.Instance;

        _controller = new AuthController(
            authService,
            jwtService,
            NullLogger<AuthController>.Instance);
    }

    [Fact]
    public async Task CompleteAuthFlow_RegisterLoginFlow_ShouldSucceed()
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = "integrationuser",
            Password = "password123"
        };

        // Act - Register
        var registerResult = await _controller.Register(registerRequest);

        // Assert - Register succeeded
        registerResult.Result.Should().BeOfType<OkObjectResult>();
        var registerOkResult = registerResult.Result as OkObjectResult;
        var registerResponse = registerOkResult!.Value as AuthResponse;
        
        registerResponse.Should().NotBeNull();
        registerResponse!.Token.Should().NotBeNullOrEmpty();
        registerResponse.Username.Should().Be("integrationuser");

        // Act - Login with same credentials
        var loginRequest = new LoginRequest
        {
            Username = "integrationuser",
            Password = "password123"
        };

        var loginResult = await _controller.Login(loginRequest);

        // Assert - Login succeeded
        loginResult.Result.Should().BeOfType<OkObjectResult>();
        var loginOkResult = loginResult.Result as OkObjectResult;
        var loginResponse = loginOkResult!.Value as AuthResponse;
        
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
        loginResponse.Username.Should().Be("integrationuser");
    }

    [Fact]
    public async Task CannotRegisterSameUserTwice()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "duplicateuser",
            Password = "password123"
        };

        // Act
        await _controller.Register(request);
        var secondAttempt = await _controller.Register(request);

        // Assert
        secondAttempt.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}