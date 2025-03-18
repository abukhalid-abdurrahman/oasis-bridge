namespace Domain.Common;

public abstract record BaseFilter
{
    public int PageSize { get; init; }
    public int PageNumber { get; init; }

    protected BaseFilter()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    protected BaseFilter(int pageSize, int pageNumber)
    {
        PageSize = pageSize <= 0 ? 10 : pageSize;
        PageNumber = pageNumber <= 0 ? 1 : pageNumber;
    }
}