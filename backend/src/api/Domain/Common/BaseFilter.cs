namespace Domain.Common;

public readonly record struct BaseFilter
{
    public int PageSize { get; init; }
    public int PageNumber { get; init; }

    public BaseFilter()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    public BaseFilter(int pageSize, int pageNumber)
    {
        PageSize = pageSize <= 0 ? 10 : pageSize;
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
    }
}