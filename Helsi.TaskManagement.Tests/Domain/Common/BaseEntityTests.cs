using FluentAssertions;
using Helsi.TaskManagement.Domain.Common;

namespace Helsi.TaskManagement.Tests.Domain.Common;

public class BaseEntityTests
{
    private class TestEntity : BaseEntity
    {
    }

    [Fact]
    public void Constructor_ShouldInitializeTimestamps()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        entity.CreatedAt.Should().Be(entity.UpdatedAt);
    }

    [Fact]
    public void UpdateTimestamp_ShouldUpdateOnlyUpdatedAt()
    {
        // Arrange
        var entity = new TestEntity();
        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;
        
        // Wait a small amount to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        entity.UpdateTimestamp();

        // Assert
        entity.CreatedAt.Should().Be(originalCreatedAt);
        entity.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        entity.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Id_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var newId = "custom-id-123";

        // Act
        entity.Id = newId;

        // Assert
        entity.Id.Should().Be(newId);
    }

    [Fact]
    public void CreatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var customDate = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        entity.CreatedAt = customDate;

        // Assert
        entity.CreatedAt.Should().Be(customDate);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSettable()
    {
        // Arrange
        var entity = new TestEntity();
        var customDate = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        entity.UpdatedAt = customDate;

        // Assert
        entity.UpdatedAt.Should().Be(customDate);
    }
}