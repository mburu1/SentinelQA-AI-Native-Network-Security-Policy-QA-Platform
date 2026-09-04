namespace SentinelQA.Application.Common;

public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record PageRequest(int Page = 1, int PageSize = 20)
{
    public int Offset => (Math.Max(1, Page) - 1) * Math.Clamp(PageSize, 1, 100);
    public int Limit => Math.Clamp(PageSize, 1, 100);
}