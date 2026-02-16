namespace API.Contracts.WorkItems.Comments;

public class GetWorkItemCommentsRequest
{
    public int PageSize { get; set; } = 10;
    public DateTime? Cursor { get; set; }
}