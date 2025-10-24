namespace Application.Common.DTOs;

public abstract class QueryParametersDto<TCursor>
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;
    public TCursor? Cursor { get; set; }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}