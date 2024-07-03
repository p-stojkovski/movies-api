namespace Movies.Contracts.Responses;

public class PagedResponse<TResponse>
{
    public required IEnumerable<TResponse> Items { get; init; } = Enumerable.Empty<TResponse>();
    public required int PageSize { get; set; }
    public required int Page { get; set; }
    public required int TotalCount { get; set; }
    public int TotalPages
    {
        get
        {
            if (PageSize == 0) return 0;
            return (int)Math.Ceiling((double)TotalCount / PageSize);
        }
    }

    public bool HasNextPage => TotalCount > (Page * PageSize);
}
