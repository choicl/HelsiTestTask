using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Helsi.TaskManagement.Api.Controllers;
using Helsi.TaskManagement.Application.Services.Interfaces;
using Helsi.TaskManagement.Application.DTOs.Requests;
using Helsi.TaskManagement.Application.DTOs.Responses;
using Helsi.TaskManagement.Application.Common.Models;
using Helsi.TaskManagement.Application.Common.Exceptions;
using UnauthorizedAccessException = Helsi.TaskManagement.Application.Common.Exceptions.UnauthorizedAccessException;

namespace Helsi.TaskManagement.Tests.Api.Controllers;

public class TaskListsControllerTests
{
    private readonly Mock<ITaskListService> _taskListServiceMock;
    private readonly TaskListsController _controller;
    private const string ValidUserId = "user123";
    private const string ValidTaskListId = "tasklist456";

    public TaskListsControllerTests()
    {
        _taskListServiceMock = new Mock<ITaskListService>();
        _controller = new TaskListsController(_taskListServiceMock.Object);
        
        // Setup HttpContext with UserId
        var httpContext = new DefaultHttpContext
        {
            Items =
            {
                ["UserId"] = ValidUserId
            }
        };
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public void Constructor_WithNullService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new TaskListsController(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("taskListService");
    }

    #region CreateTaskList Tests

    [Fact]
    public async Task CreateTaskList_WithValidRequest_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = "Test Task List" };
        var response = new TaskListResponse
        {
            Id = ValidTaskListId,
            Name = "Test Task List",
            OwnerId = ValidUserId,
            ConnectedUserIds = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskListServiceMock.Setup(s => s.CreateAsync(request, ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateTaskList(request);

        // Assert
        result.Should().NotBeNull();
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Value.Should().BeEquivalentTo(response);
        createdResult.ActionName.Should().Be(nameof(_controller.GetTaskList));
        createdResult.RouteValues!["id"].Should().Be(ValidTaskListId);
    }

    [Fact]
    public async Task CreateTaskList_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = "" };
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateTaskList(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetTaskList Tests

    [Fact]
    public async Task GetTaskList_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var response = new TaskListResponse
        {
            Id = ValidTaskListId,
            Name = "Test Task List",
            OwnerId = ValidUserId,
            ConnectedUserIds = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskListServiceMock.Setup(s => s.GetByIdAsync(ValidTaskListId, ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetTaskList(ValidTaskListId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetTaskList_WithNonExistentId_ShouldThrowTaskListNotFoundException()
    {
        // Arrange
        _taskListServiceMock.Setup(s => s.GetByIdAsync(ValidTaskListId, ValidUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskListNotFoundException(ValidTaskListId));

        // Act & Assert
        var act = async () => await _controller.GetTaskList(ValidTaskListId);
        await act.Should().ThrowAsync<TaskListNotFoundException>();
    }

    [Fact]
    public async Task GetTaskList_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _taskListServiceMock.Setup(s => s.GetByIdAsync(ValidTaskListId, ValidUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException(ValidUserId, "access task list"));

        // Act & Assert
        var act = async () => await _controller.GetTaskList(ValidTaskListId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region UpdateTaskList Tests

    [Fact]
    public async Task UpdateTaskList_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = "Updated Task List" };
        var response = new TaskListResponse
        {
            Id = ValidTaskListId,
            Name = "Updated Task List",
            OwnerId = ValidUserId,
            ConnectedUserIds = new List<string>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _taskListServiceMock.Setup(s => s.UpdateAsync(ValidTaskListId, request, ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.UpdateTaskList(ValidTaskListId, request);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task UpdateTaskList_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = "" };
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateTaskList(ValidTaskListId, request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region DeleteTaskList Tests

    [Fact]
    public async Task DeleteTaskList_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        _taskListServiceMock.Setup(s => s.DeleteAsync(ValidTaskListId, ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTaskList(ValidTaskListId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task DeleteTaskList_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        _taskListServiceMock.Setup(s => s.DeleteAsync(ValidTaskListId, ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTaskList(ValidTaskListId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        notFoundResult.Value.Should().Be($"Task list with ID '{ValidTaskListId}' not found");
    }

    #endregion

    #region GetTaskLists Tests

    [Fact]
    public async Task GetTaskLists_WithDefaultParameters_ShouldReturnOkResult()
    {
        // Arrange
        var taskLists = new List<TaskListSummaryResponse>
        {
            new TaskListSummaryResponse { Id = "1", Name = "Task List 1", OwnerId = ValidUserId, CreatedAt = DateTime.UtcNow },
            new TaskListSummaryResponse { Id = "2", Name = "Task List 2", OwnerId = ValidUserId, CreatedAt = DateTime.UtcNow }
        };
        var paginatedResult = new PaginatedResult<TaskListSummaryResponse>(taskLists, 2, 1, 10);

        _taskListServiceMock.Setup(s => s.GetPaginatedAsync(ValidUserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetTaskLists();

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(paginatedResult);
    }

    [Fact]
    public async Task GetTaskLists_WithCustomParameters_ShouldPassParametersToService()
    {
        // Arrange
        var page = 2;
        var pageSize = 5;
        var paginatedResult = new PaginatedResult<TaskListSummaryResponse>(new List<TaskListSummaryResponse>(), 0, page, pageSize);

        _taskListServiceMock.Setup(s => s.GetPaginatedAsync(ValidUserId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedResult);

        // Act
        var result = await _controller.GetTaskLists(page, pageSize);

        // Assert
        result.Should().NotBeNull();
        _taskListServiceMock.Verify(s => s.GetPaginatedAsync(ValidUserId, page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region AddConnection Tests

    [Fact]
    public async Task AddConnection_WithValidRequest_ShouldReturnNoContent()
    {
        // Arrange
        var request = new AddConnectionRequest { UserId = "newuser123" };

        _taskListServiceMock.Setup(s => s.AddConnectionAsync(ValidTaskListId, request, ValidUserId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddConnection(ValidTaskListId, request);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task AddConnection_WithInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new AddConnectionRequest { UserId = "" };
        _controller.ModelState.AddModelError("UserId", "UserId is required");

        // Act
        var result = await _controller.AddConnection(ValidTaskListId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region GetConnections Tests

    [Fact]
    public async Task GetConnections_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var connections = new List<string> { "user1", "user2", "user3" };

        _taskListServiceMock.Setup(s => s.GetConnectionsAsync(ValidTaskListId, ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(connections);

        // Act
        var result = await _controller.GetConnections(ValidTaskListId);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().BeEquivalentTo(connections);
    }

    #endregion

    #region RemoveConnection Tests

    [Fact]
    public async Task RemoveConnection_WithValidParameters_ShouldReturnNoContent()
    {
        // Arrange
        var userIdToRemove = "userToRemove";

        _taskListServiceMock.Setup(s => s.RemoveConnectionAsync(ValidTaskListId, userIdToRemove, ValidUserId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveConnection(ValidTaskListId, userIdToRemove);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    #endregion
}