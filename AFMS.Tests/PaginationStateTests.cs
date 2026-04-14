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
}
