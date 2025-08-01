using FluentAssertions;
using Moq;
using Helsi.TaskManagement.Application.Services.Implementations;
using Helsi.TaskManagement.Application.DTOs.Requests;
using Helsi.TaskManagement.Application.Common.Exceptions;
using Helsi.TaskManagement.Domain.Entities;
using Helsi.TaskManagement.Domain.Interfaces;
using UnauthorizedAccessException = Helsi.TaskManagement.Application.Common.Exceptions.UnauthorizedAccessException;

namespace Helsi.TaskManagement.Tests.Application.Services;

public class TaskListServiceTests
{
    private readonly Mock<ITaskListRepository> _repositoryMock;
    private readonly TaskListService _service;
    private const string ValidUserId = "user123";
    private const string ValidOwnerId = "owner456";
    private const string ValidTaskListId = "tasklist789";
    private const string ValidName = "Test Task List";

    public TaskListServiceTests()
    {
        _repositoryMock = new Mock<ITaskListRepository>();
        _service = new TaskListService(_repositoryMock.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new TaskListService(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("repository");
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateAndReturnTaskList()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = ValidName };
        var createdTaskList = new TaskList(ValidName, ValidUserId);
        
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<TaskList>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTaskList);

        // Act
        var result = await _service.CreateAsync(request, ValidUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(ValidName);
        result.OwnerId.Should().Be(ValidUserId);
        result.ConnectedUserIds.Should().BeEmpty();
        
        _repositoryMock.Verify(r => r.CreateAsync(It.Is<TaskList>(tl => 
            tl.Name == ValidName && tl.OwnerId == ValidUserId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = ValidName };

        // Act & Assert
        var act = async () => await _service.CreateAsync(request, invalidUserId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("UserId cannot be empty*")
            .Where(ex => ex.ParamName == "userId");
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithValidIdAndAuthorizedUser_ShouldReturnTaskList()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        taskList.AddConnection(ValidUserId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act
        var result = await _service.GetByIdAsync(ValidTaskListId, ValidUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(ValidName);
        result.OwnerId.Should().Be(ValidOwnerId);
        result.ConnectedUserIds.Should().Contain(ValidUserId);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldThrowTaskListNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskList?)null);

        // Act & Assert
        var act = async () => await _service.GetByIdAsync(ValidTaskListId, ValidUserId);
        await act.Should().ThrowAsync<TaskListNotFoundException>()
            .WithMessage($"Task list with ID '{ValidTaskListId}' was not found.");
    }

    [Fact]
    public async Task GetByIdAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act & Assert
        var act = async () => await _service.GetByIdAsync(ValidTaskListId, ValidUserId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"User '{ValidUserId}' is not authorized to perform 'access task list' operation.");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithValidRequestAndAuthorizedUser_ShouldUpdateAndReturnTaskList()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = "Updated Name" };
        var taskList = new TaskList(ValidName, ValidOwnerId);
        taskList.AddConnection(ValidUserId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskList>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskList tl, CancellationToken _) => tl);

        // Act
        var result = await _service.UpdateAsync(ValidTaskListId, request, ValidUserId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        
        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<TaskList>(tl => tl.Name == "Updated Name"), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new CreateAndUpdateTaskListRequest { Name = "Updated Name" };
        var taskList = new TaskList(ValidName, ValidOwnerId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act & Assert
        var act = async () => await _service.UpdateAsync(ValidTaskListId, request, ValidUserId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithValidIdAndOwner_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidUserId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);
        _repositoryMock.Setup(r => r.DeleteAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(ValidTaskListId, ValidUserId);

        // Assert
        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(ValidTaskListId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithNonOwner_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act & Assert
        var act = async () => await _service.DeleteAsync(ValidTaskListId, ValidUserId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage($"User '{ValidUserId}' is not authorized to perform 'delete task list' operation.");
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentId_ShouldThrowTaskListNotFoundException()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskList?)null);

        // Act & Assert
        var act = async () => await _service.DeleteAsync(ValidTaskListId, ValidUserId);
        await act.Should().ThrowAsync<TaskListNotFoundException>();
    }

    #endregion

    #region GetPaginatedAsync Tests

    [Fact]
    public async Task GetPaginatedAsync_WithValidParameters_ShouldReturnPaginatedResult()
    {
        // Arrange
        var taskLists = new List<TaskList>
        {
            new("Task List 1", ValidUserId),
            new("Task List 2", ValidUserId)
        };
        
        _repositoryMock.Setup(r => r.GetByUserAccessAsync(ValidUserId, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskLists);
        _repositoryMock.Setup(r => r.GetCountByUserAccessAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _service.GetPaginatedAsync(ValidUserId, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetPaginatedAsync_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
    {
        // Act & Assert
        var act = async () => await _service.GetPaginatedAsync(invalidUserId);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("UserId cannot be empty*")
            .Where(ex => ex.ParamName == "userId");
    }

    [Theory]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(1, 0, 1, 10)]
    [InlineData(1, -1, 1, 10)]
    [InlineData(1, 101, 1, 10)]
    public async Task GetPaginatedAsync_WithInvalidPagination_ShouldUseDefaults(int page, int pageSize, int expectedPage, int expectedPageSize)
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByUserAccessAsync(ValidUserId, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TaskList>());
        _repositoryMock.Setup(r => r.GetCountByUserAccessAsync(ValidUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _service.GetPaginatedAsync(ValidUserId, page, pageSize);

        // Assert
        result.Page.Should().Be(expectedPage);
        result.PageSize.Should().Be(expectedPageSize);
        _repositoryMock.Verify(r => r.GetByUserAccessAsync(ValidUserId, expectedPage, expectedPageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region AddConnectionAsync Tests

    [Fact]
    public async Task AddConnectionAsync_WithValidRequest_ShouldAddConnection()
    {
        // Arrange
        var request = new AddConnectionRequest { UserId = "newuser123" };
        var taskList = new TaskList(ValidName, ValidUserId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskList>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskList tl, CancellationToken _) => tl);

        // Act
        await _service.AddConnectionAsync(ValidTaskListId, request, ValidUserId);

        // Assert
        taskList.ConnectedUserIds.Should().Contain("newuser123");
        _repositoryMock.Verify(r => r.UpdateAsync(taskList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddConnectionAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var request = new AddConnectionRequest { UserId = "newuser123" };
        var taskList = new TaskList(ValidName, ValidOwnerId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act & Assert
        var act = async () => await _service.AddConnectionAsync(ValidTaskListId, request, ValidUserId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region GetConnectionsAsync Tests

    [Fact]
    public async Task GetConnectionsAsync_WithAuthorizedUser_ShouldReturnConnections()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        taskList.AddConnection(ValidUserId);
        taskList.AddConnection("user2");
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act
        var result = await _service.GetConnectionsAsync(ValidTaskListId, ValidUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(ValidUserId);
        result.Should().Contain("user2");
    }

    [Fact]
    public async Task GetConnectionsAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act & Assert
        var act = async () => await _service.GetConnectionsAsync(ValidTaskListId, ValidUserId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion

    #region RemoveConnectionAsync Tests

    [Fact]
    public async Task RemoveConnectionAsync_WithValidRequest_ShouldRemoveConnection()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidUserId);
        taskList.AddConnection("userToRemove");
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);
        _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<TaskList>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskList tl, CancellationToken _) => tl);

        // Act
        await _service.RemoveConnectionAsync(ValidTaskListId, "userToRemove", ValidUserId);

        // Assert
        taskList.ConnectedUserIds.Should().NotContain("userToRemove");
        _repositoryMock.Verify(r => r.UpdateAsync(taskList, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveConnectionAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        
        _repositoryMock.Setup(r => r.GetByIdAsync(ValidTaskListId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        // Act & Assert
        var act = async () => await _service.RemoveConnectionAsync(ValidTaskListId, "userToRemove", ValidUserId);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    #endregion
}