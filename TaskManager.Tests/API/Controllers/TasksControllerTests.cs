using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaskManager.API.Controllers;
using TaskManager.API.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using Xunit;

namespace TaskManager.Tests.API.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<ILogger<TasksController>> _loggerMock;
    private readonly TasksController _controller;
    private readonly Guid _testUserId;

    public TasksControllerTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _loggerMock = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_taskRepositoryMock.Object, _loggerMock.Object);
        _testUserId = Guid.NewGuid();

        // Setup user claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task GetTasks_ShouldReturnUserTasks()
    {
        // Arrange
        var tasks = new List<TaskItem>
        {
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 1", Description = "Desc 1", UserId = _testUserId },
            new TaskItem { Id = Guid.NewGuid(), Title = "Task 2", Description = "Desc 2", UserId = _testUserId }
        };

        _taskRepositoryMock
            .Setup(x => x.GetByUserIdAsync(_testUserId))
            .ReturnsAsync(tasks);

        // Act
        var result = await _controller.GetTasks();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTasks = okResult!.Value as List<TaskItemDto>;
        returnedTasks.Should().HaveCount(2);
        returnedTasks![0].Title.Should().Be("Task 1");
    }

    [Fact]
    public async Task CreateTask_ShouldAddTaskAndReturnCreated()
    {
        // Arrange
        var request = new CreateTaskRequest
        {
            Title = "New Task",
            Description = "New Description"
        };

        // Act
        var result = await _controller.CreateTask(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var dto = createdResult!.Value as TaskItemDto;
        dto.Should().NotBeNull();
        dto!.Title.Should().Be("New Task");
        dto.Description.Should().Be("New Description");

        _taskRepositoryMock.Verify(x => x.AddAsync(It.Is<TaskItem>(
            t => t.Title == "New Task" && t.UserId == _testUserId
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateTask_ShouldUpdateTaskAndReturnNoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var request = new CreateTaskRequest
        {
            Title = "Updated Task",
            Description = "Updated Description"
        };

        // Act
        var result = await _controller.UpdateTask(taskId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();

        _taskRepositoryMock.Verify(x => x.UpdateAsync(It.Is<TaskItem>(
            t => t.Id == taskId && t.Title == "Updated Task" && t.UserId == _testUserId
        )), Times.Once);
    }

    [Fact]
    public async Task DeleteTask_ShouldDeleteTaskAndReturnNoContent()
    {
        // Arrange
        var taskId = Guid.NewGuid();

        // Act
        var result = await _controller.DeleteTask(taskId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _taskRepositoryMock.Verify(x => x.DeleteAsync(taskId), Times.Once);
    }
}