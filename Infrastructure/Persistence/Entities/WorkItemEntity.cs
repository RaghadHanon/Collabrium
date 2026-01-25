using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class WorkItemEntity : DbEntity
{
    public string Title { get; set; } = null!;
    public string? AcceptanceCriteria { get; set; }
    public string? Description { get; set; }
    public int EffortPoints { get; set; }

    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string CreatedByUserId { get; set; } = null!;

    public string ProjectId { get; set; } = null!;
    public string? SprintId { get; set; }

    public ApplicationUserEntity? AssignedToUser { get; set; }
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;
    public ProjectEntity Project { get; set; } = null!;
    public SprintEntity? Sprint { get; set; }

    public ICollection<CommentEntity> Comments { get; set; } = new List<CommentEntity>();
    public ICollection<WorkItemRelationEntity> OutgoingRelations { get; set; } = new List<WorkItemRelationEntity>();
    public ICollection<WorkItemRelationEntity> IncomingRelations { get; set; } = new List<WorkItemRelationEntity>();
}
