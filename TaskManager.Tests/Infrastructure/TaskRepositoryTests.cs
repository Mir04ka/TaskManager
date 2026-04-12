using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Repositories;
using Xunit;

namespace TaskManager.Tests.Infrastructure;

public class TaskRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly TaskRepository _repository;

    public TaskRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new TaskRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddTaskToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var task = new TaskItem
        {
            Title = "Test Task",
            Description = "Test Description",
            UserId = userId
        };

        // Act
        await _repository.AddAsync(task);

        var result = await _repository.GetByUserIdAsync(userId, 1, 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Test Task");
        result.Items[0].Description.Should().Be("Test Description");
        result.Items[0].Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnOnlyUserTasks()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        await _repository.AddAsync(new TaskItem { Title = "User1 Task1", UserId = userId1 });
        await _repository.AddAsync(new TaskItem { Title = "User1 Task2", UserId = userId1 });
        await _repository.AddAsync(new TaskItem { Title = "User2 Task1", UserId = userId2 });

        // Act
        var user1Tasks = await _repository.GetByUserIdAsync(userId1, 1, 10);

        // Assert
        user1Tasks.Items.Should().HaveCount(2);
        user1Tasks.Items.All(t => t.UserId == userId1).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateTaskProperties()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Original Title",
            Description = "Original Description",
            UserId = Guid.NewGuid()
        };

        await _repository.AddAsync(task);

        // Act
        task.Title = "Updated Title";
        task.Description = "Updated Description";

        await _repository.UpdateAsync(task);

        var result = await _repository.GetByUserIdAsync(
            task.UserId,
            pageNumber: 1,
            pageSize: 10);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Updated Title");
        result.Items[0].Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var task = new TaskItem
        {
            Title = "Task to Delete",
            UserId = userId
        };

        await _repository.AddAsync(task);

        // Act
        await _repository.DeleteAsync(task.Id);

        var result = await _repository.GetByUserIdAsync(userId, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDoNothing_WhenTaskDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act & Assert - should not throw
        await _repository.DeleteAsync(nonExistentId);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnPagedTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();

        await _repository.AddAsync(new TaskItem { Title = "Task 1", UserId = userId });
        await _repository.AddAsync(new TaskItem { Title = "Task 2", UserId = userId });
        await _repository.AddAsync(new TaskItem { Title = "Task 3", UserId = userId });

        // Act
        var result = await _repository.GetByUserIdAsync(userId, 1, 2);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var tasks = await _repository.GetByUserIdAsync(userId, 1, 10);

        // Assert
        tasks.Items.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}