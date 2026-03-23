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
        var task = new TaskItem
        {
            Title = "Test Task",
            Description = "Test Description",
            UserId = Guid.NewGuid()
        };

        // Act
        await _repository.AddAsync(task);
        var tasks = await _repository.GetAllAsync();

        // Assert
        tasks.Should().HaveCount(1);
        tasks[0].Title.Should().Be("Test Task");
        tasks[0].Description.Should().Be("Test Description");
        tasks[0].Id.Should().NotBe(Guid.Empty);
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
        var user1Tasks = await _repository.GetByUserIdAsync(userId1);

        // Assert
        user1Tasks.Should().HaveCount(2);
        user1Tasks.All(t => t.UserId == userId1).Should().BeTrue();
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

        var tasks = await _repository.GetAllAsync();

        // Assert
        tasks.Should().HaveCount(1);
        tasks[0].Title.Should().Be("Updated Title");
        tasks[0].Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveTask()
    {
        // Arrange
        var task = new TaskItem
        {
            Title = "Task to Delete",
            UserId = Guid.NewGuid()
        };
        await _repository.AddAsync(task);

        // Act
        await _repository.DeleteAsync(task.Id);
        var tasks = await _repository.GetAllAsync();

        // Assert
        tasks.Should().BeEmpty();
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
    public async Task GetAllAsync_ShouldReturnAllTasks()
    {
        // Arrange
        await _repository.AddAsync(new TaskItem { Title = "Task 1", UserId = Guid.NewGuid() });
        await _repository.AddAsync(new TaskItem { Title = "Task 2", UserId = Guid.NewGuid() });
        await _repository.AddAsync(new TaskItem { Title = "Task 3", UserId = Guid.NewGuid() });

        // Act
        var tasks = await _repository.GetAllAsync();

        // Assert
        tasks.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoTasks()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var tasks = await _repository.GetByUserIdAsync(userId);

        // Assert
        tasks.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}