using FluentAssertions;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_ShouldGenerateGuidId_OnCreation()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void User_ShouldHaveDefaultEmptyValues()
    {
        // Act
        var user = new User();

        // Assert
        user.Username.Should().Be(string.Empty);
        user.PasswordHash.Should().Be(string.Empty);
    }

    [Fact]
    public void User_ShouldAllowSettingProperties()
    {
        // Arrange
        var user = new User();

        // Act
        user.Username = "testuser";
        user.PasswordHash = "hashed";

        // Assert
        user.Username.Should().Be("testuser");
        user.PasswordHash.Should().Be("hashed");
    }
}