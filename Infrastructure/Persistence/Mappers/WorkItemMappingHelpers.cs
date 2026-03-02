using Domain.Models;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.Mappers;

internal static class WorkItemMappingHelpers
{
    internal static void HydrateChildrenIfRequested(WorkItemEntity entity, WorkItem domain, bool includeChildren)
    {
        if (!includeChildren) return;

        if (entity.Comments is not null)
        {
            foreach (var c in entity.Comments)
                domain.RehydrateAddComment(CommentMapper.ToDomain(c));
        }

        if (entity.Relations is not null)
        {
            foreach (var r in entity.Relations)
                domain.RehydrateAddRelation(WorkItemRelationMapper.ToDomain(r));
        }

        if (entity.CodeLinks is not null)
        {
            foreach (var linkEntity in entity.CodeLinks)
                domain.RehydrateAddCodeLink(CodeLinkMapper.ToDomain(linkEntity));
        }

        if (entity.Attachments is not null)
        {
            foreach (var attEntity in entity.Attachments)
                domain.RehydrateAddAttachment(AttachmentMapper.ToDomain(attEntity));
        }
    }

    internal static void FillEntityBaseFields(WorkItem domain, WorkItemEntity entity)
    {
        entity.Id = domain.Id;
        entity.CreatedAt = domain.CreatedAt;
        entity.UpdatedAt = domain.UpdatedAt;

        entity.Title = domain.Title;
        entity.Code = domain.Code;
        entity.Description = domain.Description;
        entity.AcceptanceCriteria = domain.AcceptanceCriteria;
        entity.EffortPoints = domain.EffortPoints;

        entity.Type = domain.Type;
        entity.Status = domain.Status;
        entity.Priority = domain.Priority;

        entity.IsBlocked = domain.IsBlocked;
        entity.StartDate = domain.StartDate;
        entity.EndDate = domain.EndDate;

        entity.AssignedToUserId = domain.AssignedToUserId;

        entity.CreatedByUserId = domain.CreatedByUserId;
        entity.ProjectId = domain.ProjectId;
        entity.SprintId = domain.SprintId;
    }
}