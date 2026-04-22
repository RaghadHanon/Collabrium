using Domain.Enums;

namespace Infrastructure.Persistence.Entities;

public class WorkItemEntity : DbEntity
{
    public string Title { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? AcceptanceCriteria { get; set; }
    public string? Description { get; set; }
    public int EffortPoints { get; set; }

    public WorkItemType Type { get; set; }
    public WorkItemStatus Status { get; set; }
    public WorkItemPriority Priority { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public string? AssignedToUserId { get; set; }
    public ApplicationUserEntity? AssignedToUser { get; set; }

    public string CreatedByUserId { get; set; } = null!;
    public ApplicationUserEntity CreatedByUser { get; set; } = null!;

    public string ProjectId { get; set; } = null!;
    public ProjectEntity Project { get; set; } = null!;

    public string? SprintId { get; set; }
    public SprintEntity? Sprint { get; set; }

    public string? ParentId { get; set; }
    public WorkItemEntity? Parent { get; set; }
    public ICollection<WorkItemCommentEntity> Comments { get; set; } = new List<WorkItemCommentEntity>();
    public ICollection<WorkItemRelationEntity> Relations { get; set; } = new List<WorkItemRelationEntity>();
    public ICollection<CodeLinkEntity> CodeLinks { get; set; } = new List<CodeLinkEntity>();
    public ICollection<AttachmentEntity> Attachments { get; set; } = new List<AttachmentEntity>();
}