using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.API.Controllers;
using TaskManager.API.DTOs;
using TaskManager.API.Services;
using TaskManager.Application.Common;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.API.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _jwtServiceMock  = new Mock<IJwtService>();
        _loggerMock      = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(
            _authServiceMock.Object,
            _jwtServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Register_WithNewUser_ShouldReturnOkWithToken()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var request = new RegisterRequest
        {
            Username = "newuser",
            Password = "password123"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request.Username, request.Password))
            .ReturnsAsync(AuthResult.Ok(userId, request.Username));

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Guid>(), request.Username))
            .Returns("fake-jwt-token");

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;
        response.Should().NotBeNull();
        response!.Token.Should().Be("fake-jwt-token");
        response.Username.Should().Be("newuser");
        response.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Register_WithExistingUser_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = "existinguser",
            Password = "password123"
        };

        _authServiceMock
            .Setup(x => x.RegisterAsync(request.Username, request.Password))
            .ReturnsAsync(AuthResult.Fail("User already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var userId  = Guid.NewGuid();
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request.Username, request.Password))
            .ReturnsAsync(AuthResult.Ok(userId, request.Username));

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Guid>(), request.Username))
            .Returns("fake-jwt-token");

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;
        response.Should().NotBeNull();
        response!.Token.Should().Be("fake-jwt-token");
        response.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "nonexistent",
            Password = "password123"
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request.Username, request.Password))
            .ReturnsAsync(AuthResult.Fail("Invalid credentials"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };

        _authServiceMock
            .Setup(x => x.LoginAsync(request.Username, request.Password))
            .ReturnsAsync(AuthResult.Fail("Invalid credentials"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        _jwtServiceMock.Verify(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Register_WithEmptyUsername_ShouldHandleGracefully(string? username)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Username = username!,
            Password = "password123"
        };

        // Act
        var result = await _controller.Register(request);

        // Assert - behavior depends on validation setup
        // For now just ensure it doesn't throw
        result.Should().NotBeNull();
    }
}