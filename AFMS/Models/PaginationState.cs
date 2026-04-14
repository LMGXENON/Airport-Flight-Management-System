namespace AFMS.Models;

public class PaginationState
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalCount { get; set; }

    private int EffectivePageSize => PageSize > 0 ? PageSize : 25;
    private int EffectivePage => Math.Clamp(Page, 1, TotalPages);

    public int TotalPages => TotalCount <= 0
        ? 1
        : (int)Math.Ceiling(TotalCount / (double)EffectivePageSize);

    public bool HasPreviousPage => EffectivePage > 1;
    public bool HasNextPage => EffectivePage < TotalPages;
    public int SkipCount => TotalCount == 0 ? 0 : (EffectivePage - 1) * EffectivePageSize;
    public int PageStart => TotalCount == 0 ? 0 : ((EffectivePage - 1) * EffectivePageSize) + 1;
    public int PageEnd => TotalCount == 0 ? 0 : Math.Min(EffectivePage * EffectivePageSize, TotalCount);

    public IEnumerable<int> VisiblePages(int maxButtons = 7)
    {
        if (maxButtons < 3)
        {
            maxButtons = 3;
        }

        var currentPage = EffectivePage;
        var startPage = Math.Max(1, currentPage - (maxButtons / 2));
        var endPage = Math.Min(TotalPages, startPage + maxButtons - 1);

        if (endPage - startPage < maxButtons - 1)
        {
            startPage = Math.Max(1, endPage - maxButtons + 1);
        }

        for (var pageNumber = startPage; pageNumber <= endPage; pageNumber++)
        {
            yield return pageNumber;
        }
    }
}