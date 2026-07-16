using EduApoyos.Domain.Common;

namespace EduApoyos.Application.Common;

public class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }

    public PagedResponse(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize, int totalPages)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = totalPages;
    }

    public static PagedResponse<T> FromDomain<TSource>(PagedResult<TSource> source, Func<TSource, T> map)
    {
        var items = source.Items.Select(map).ToList();
        return new PagedResponse<T>(items, source.TotalCount, source.PageNumber, source.PageSize, source.TotalPages);
    }
}
