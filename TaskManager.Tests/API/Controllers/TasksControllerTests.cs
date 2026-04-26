using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaskManager.API.Controllers;
using TaskManager.API.DTOs;
using TaskManager.Application.Common;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Tests.API.Controllers;

public class TasksControllerTests
{
    private readonly Mock<ITaskService> _taskServiceMock;
    private readonly Mock<ILogger<TasksController>> _loggerMock;
    private readonly TasksController _controller;
    private readonly Guid _testUserId;

    public TasksControllerTests()
    {
        _taskServiceMock = new Mock<ITaskService>();
        _loggerMock = new Mock<ILogger<TasksController>>();
        _controller = new TasksController(_taskServiceMock.Object, _loggerMock.Object);
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

        _taskServiceMock
            .Setup(x => x.GetCurrentUserTasksAsync(1, 10))
            .ReturnsAsync(new PagedResult<TaskItem>
            {
                Items = tasks,
                TotalCount = 2,
                PageNumber = 1,
                PageSize = 10
            });

        // Act
        var result = await _controller.GetTasks(1, 10);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var okResult = result.Result as OkObjectResult;

        var returned = okResult!.Value as PagedResult<TaskItemDto>;

        returned.Should().NotBeNull();
        returned!.Items.Should().HaveCount(2);
        returned.Items[0].Title.Should().Be("Task 1");
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

        _taskServiceMock.Verify(x => x.AddAsync(It.Is<TaskItem>(
            t => t.Title == "New Task"
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

        _taskServiceMock.Verify(x => x.UpdateAsync(It.Is<TaskItem>(
            t => t.Id == taskId && t.Title == "Updated Task"
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
        _taskServiceMock.Verify(x => x.DeleteAsync(taskId), Times.Once);
    }
}
