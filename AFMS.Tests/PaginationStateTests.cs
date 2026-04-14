using AFMS.Models;

namespace AFMS.Tests;

public class PaginationStateTests
{
    [Fact]
    public void TotalPages_UsesDefaultPageSizeWhenPageSizeIsZero()
    {
        var pagination = new PaginationState
        {
            TotalCount = 50,
            PageSize = 0
        };

        Assert.Equal(2, pagination.TotalPages);
    }

    [Fact]
    public void PageEnd_UsesDefaultPageSizeWhenPageSizeIsNegative()
    {
        var pagination = new PaginationState
        {
            Page = 2,
            TotalCount = 60,
            PageSize = -5
        };

        Assert.Equal(50, pagination.PageEnd);
    }

    [Fact]
    public void PageStart_UsesFirstPageWhenRequestedPageIsZero()
    {
        var pagination = new PaginationState
        {
            Page = 0,
            PageSize = 25,
            TotalCount = 90
        };

        Assert.Equal(1, pagination.PageStart);
    }

    [Fact]
    public void HasNextPage_IsFalseWhenRequestedPageExceedsTotalPages()
    {
        var pagination = new PaginationState
        {
            Page = 99,
            PageSize = 25,
            TotalCount = 30
        };

        Assert.False(pagination.HasNextPage);
    }
}
