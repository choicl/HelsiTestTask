using FluentAssertions;
using Helsi.TaskManagement.Application.Common.Models;

namespace Helsi.TaskManagement.Tests.Application.Common.Models;

public class PaginatedResultTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldInitializeProperties()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var totalCount = 10;
        var page = 2;
        var pageSize = 5;

        // Act
        var result = new PaginatedResult<string>(items, totalCount, page, pageSize);

        // Assert
        result.Items.Should().BeEquivalentTo(items);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
    }

    [Theory]
    [InlineData(10, 5, 2)]
    [InlineData(11, 5, 3)]
    [InlineData(9, 5, 2)]
    [InlineData(5, 5, 1)]
    [InlineData(0, 5, 0)]
    public void TotalPages_ShouldCalculateCorrectly(int totalCount, int pageSize, int expectedTotalPages)
    {
        // Arrange
        var items = new List<string>();
        var result = new PaginatedResult<string>(items, totalCount, 1, pageSize);

        // Act & Assert
        result.TotalPages.Should().Be(expectedTotalPages);
    }

    [Theory]
    [InlineData(1, 15, 5, true)]  // page 1 of 3
    [InlineData(2, 15, 5, true)]  // page 2 of 3
    [InlineData(3, 15, 5, false)] // page 3 of 3
    [InlineData(1, 10, 5, true)]  // page 1 of 2
    [InlineData(2, 10, 5, false)] // page 2 of 2
    public void HasNextPage_ShouldReturnCorrectValue(int currentPage, int totalCount, int pageSize, bool expectedHasNext)
    {
        // Arrange
        var items = new List<string>();
        var result = new PaginatedResult<string>(items, totalCount, currentPage, pageSize);

        // Act & Assert
        result.HasNextPage.Should().Be(expectedHasNext);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(3, true)]
    public void HasPreviousPage_ShouldReturnCorrectValue(int currentPage, bool expectedHasPrevious)
    {
        // Arrange
        var items = new List<string>();
        var result = new PaginatedResult<string>(items, 15, currentPage, 5);

        // Act & Assert
        result.HasPreviousPage.Should().Be(expectedHasPrevious);
    }

    [Fact]
    public void Constructor_WithEmptyItems_ShouldWork()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = new PaginatedResult<string>(items, 0, 1, 10);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNullItems_ShouldWork()
    {
        // Act
        var result = new PaginatedResult<string>(null!, 0, 1, 10);

        // Assert
        result.Items.Should().BeNull();
        result.TotalCount.Should().Be(0);
    }
}