using FluentAssertions;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void TaskItem_ShouldGenerateGuidId_OnCreation()
    {
        // Act
        var task = new TaskItem();

        // Assert
        task.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void TaskItem_ShouldHaveDefaultEmptyValues()
    {
        // Act
        var task = new TaskItem();

        // Assert
        task.Title.Should().Be(string.Empty);
        task.Description.Should().Be(string.Empty);
        task.ProcessId.Should().Be(Guid.Empty);
        task.UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public void TaskItem_ShouldAllowSettingProperties()
    {
        // Arrange
        var task = new TaskItem();
        var userId = Guid.NewGuid();

        // Act
        task.Title = "Test Task";
        task.Description = "Test Description";
        task.UserId = userId;

        // Assert
        task.Title.Should().Be("Test Task");
        task.Description.Should().Be("Test Description");
        task.UserId.Should().Be(userId);
    }

    [Fact]
    public void TaskItem_ShouldAcceptMaxLengthTitle()
    {
        // Arrange
        var task = new TaskItem();
        var longTitle = new string('A', 200);

        // Act
        task.Title = longTitle;

        // Assert
        task.Title.Should().HaveLength(200);
    }
}