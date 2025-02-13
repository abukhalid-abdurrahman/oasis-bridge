namespace Application.Extensions.Responses.PagedResponse;

public record PagedResponse<T> : BaseFilter
{
    public int TotalPages { get; init; }
    public int TotalRecords { get; init; }
    public T? Data { get; init; }

    private PagedResponse(int pageSize, int pageNumber, int totalRecords, T? data)
        : base(pageNumber, pageSize)
    {
        Data = data;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
    }

    public static PagedResponse<T>
        Create(int pageSize, int pageNumber, int totalRecords, T? data)
        => new(pageSize, pageNumber, totalRecords, data);
}