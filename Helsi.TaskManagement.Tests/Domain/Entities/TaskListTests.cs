using FluentAssertions;
using Helsi.TaskManagement.Domain.Entities;

namespace Helsi.TaskManagement.Tests.Domain.Entities;

public class TaskListTests
{
    private const string ValidName = "Test Task List";
    private const string ValidOwnerId = "owner123";
    private const string ValidUserId = "user456";

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateTaskList()
    {
        // Act
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Assert
        taskList.Name.Should().Be(ValidName);
        taskList.OwnerId.Should().Be(ValidOwnerId);
        taskList.ConnectedUserIds.Should().BeEmpty();
        taskList.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        taskList.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        var act = () => new TaskList(invalidName, ValidOwnerId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task list name cannot be empty*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var longName = new string('a', 256);

        // Act & Assert
        var act = () => new TaskList(longName, ValidOwnerId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task list name cannot exceed 255 characters*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void Constructor_WithNullOwnerId_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new TaskList(ValidName, null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("ownerId");
    }

    [Fact]
    public void Constructor_WithNameContainingWhitespace_ShouldTrimName()
    {
        // Arrange
        var nameWithWhitespace = "  Test Task List  ";

        // Act
        var taskList = new TaskList(nameWithWhitespace, ValidOwnerId);

        // Assert
        taskList.Name.Should().Be("Test Task List");
    }

    [Fact]
    public void SetName_WithValidName_ShouldUpdateNameAndTimestamp()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        var originalUpdatedAt = taskList.UpdatedAt;
        var newName = "Updated Task List";
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        taskList.SetName(newName);

        // Assert
        taskList.Name.Should().Be(newName);
        taskList.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetName_WithInvalidName_ShouldThrowArgumentException(string invalidName)
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act & Assert
        var act = () => taskList.SetName(invalidName);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task list name cannot be empty*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void SetName_WithNameTooLong_ShouldThrowArgumentException()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        var longName = new string('a', 256);

        // Act & Assert
        var act = () => taskList.SetName(longName);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Task list name cannot exceed 255 characters*")
            .And.ParamName.Should().Be("name");
    }

    [Fact]
    public void AddConnection_WithValidUserId_ShouldAddUserAndUpdateTimestamp()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        var originalUpdatedAt = taskList.UpdatedAt;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        taskList.AddConnection(ValidUserId);

        // Assert
        taskList.ConnectedUserIds.Should().Contain(ValidUserId);
        taskList.ConnectedUserIds.Should().HaveCount(1);
        taskList.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddConnection_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act & Assert
        var act = () => taskList.AddConnection(invalidUserId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*")
            .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void AddConnection_WithOwnerUserId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act & Assert
        var act = () => taskList.AddConnection(ValidOwnerId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Owner cannot be added as a connection");
    }

    [Fact]
    public void AddConnection_WithDuplicateUserId_ShouldNotAddDuplicateAndNotUpdateTimestamp()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        taskList.AddConnection(ValidUserId);
        var originalUpdatedAt = taskList.UpdatedAt;
        
        // Wait a small amount to ensure we can detect if timestamp changes
        Thread.Sleep(10);

        // Act
        taskList.AddConnection(ValidUserId);

        // Assert
        taskList.ConnectedUserIds.Should().Contain(ValidUserId);
        taskList.ConnectedUserIds.Should().HaveCount(1);
        taskList.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Fact]
    public void RemoveConnection_WithExistingUserId_ShouldRemoveUserAndUpdateTimestamp()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        taskList.AddConnection(ValidUserId);
        var originalUpdatedAt = taskList.UpdatedAt;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        taskList.RemoveConnection(ValidUserId);

        // Assert
        taskList.ConnectedUserIds.Should().NotContain(ValidUserId);
        taskList.ConnectedUserIds.Should().BeEmpty();
        taskList.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void RemoveConnection_WithNonExistingUserId_ShouldNotUpdateTimestamp()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        var originalUpdatedAt = taskList.UpdatedAt;
        
        // Wait a small amount to ensure we can detect if timestamp changes
        Thread.Sleep(10);

        // Act
        taskList.RemoveConnection("nonexistent");

        // Assert
        taskList.ConnectedUserIds.Should().BeEmpty();
        taskList.UpdatedAt.Should().Be(originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RemoveConnection_WithInvalidUserId_ShouldThrowArgumentException(string invalidUserId)
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act & Assert
        var act = () => taskList.RemoveConnection(invalidUserId);
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID cannot be empty*")
            .And.ParamName.Should().Be("userId");
    }

    [Fact]
    public void HasAccess_WithOwnerUserId_ShouldReturnTrue()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act
        var result = taskList.HasAccess(ValidOwnerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAccess_WithConnectedUserId_ShouldReturnTrue()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);
        taskList.AddConnection(ValidUserId);

        // Act
        var result = taskList.HasAccess(ValidUserId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasAccess_WithUnauthorizedUserId_ShouldReturnFalse()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act
        var result = taskList.HasAccess("unauthorized");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void HasAccess_WithInvalidUserId_ShouldReturnFalse(string invalidUserId)
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act
        var result = taskList.HasAccess(invalidUserId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOwner_WithOwnerUserId_ShouldReturnTrue()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act
        var result = taskList.IsOwner(ValidOwnerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsOwner_WithNonOwnerUserId_ShouldReturnFalse()
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act
        var result = taskList.IsOwner(ValidUserId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsOwner_WithInvalidUserId_ShouldReturnFalse(string invalidUserId)
    {
        // Arrange
        var taskList = new TaskList(ValidName, ValidOwnerId);

        // Act
        var result = taskList.IsOwner(invalidUserId);

        // Assert
        result.Should().BeFalse();
    }
}