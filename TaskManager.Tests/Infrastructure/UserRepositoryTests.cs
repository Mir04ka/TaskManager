using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Tests.Infrastructure;

public class UserRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            PasswordHash = "hashedpassword123"
        };

        // Act
        await _repository.AddAsync(user);
        var foundUser = await _repository.GetByUsernameAsync("testuser");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Username.Should().Be("testuser");
        foundUser.PasswordHash.Should().Be("hashedpassword123");
        foundUser.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var user = await _repository.GetByUsernameAsync("nonexistent");

        // Assert
        user.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var user = new User
        {
            Username = "existinguser",
            PasswordHash = "hash123"
        };
        await _repository.AddAsync(user);

        // Act
        var foundUser = await _repository.GetByUsernameAsync("existinguser");

        // Assert
        foundUser.Should().NotBeNull();
        foundUser!.Username.Should().Be("existinguser");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task GetByUsernameAsync_ShouldHandleInvalidInput(string? username)
    {
        // Act & Assert
        var result = await _repository.GetByUsernameAsync(username!);
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}