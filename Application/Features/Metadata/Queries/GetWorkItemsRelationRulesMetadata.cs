using Application.Common;
using Application.Features.Metadata.DTOs;
using Application.Interfaces;
using Domain.Enums;
using MediatR;

namespace Application.Features.Metadata.Queries;
public class GetWorkItemsRelationRulesMetadata

{
    public class Query : IRequest<Result<List<WorkItemsRelationRulesMetadataDto>>>
    {
    }

    public class Handler : IRequestHandler<Query, Result<List<WorkItemsRelationRulesMetadataDto>>>
    {
        public Task<Result<List<WorkItemsRelationRulesMetadataDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var rules = new List<WorkItemsRelationRulesMetadataDto>
            {
                // Parent-Child relations
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.Epic, RelationType = WorkItemRelationType.Parent },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TaskItem, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.Parent },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.Parent },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TestCase, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.Parent },

                // Child relations
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Epic, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.Child },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.TaskItem, RelationType = WorkItemRelationType.Child },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.Bug, RelationType = WorkItemRelationType.Child },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.Child },

                // FoundByTestCase relations
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.FoundByTestCase },

                // RelatesTo relations (assuming all types can relate to each other)
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Epic, To = WorkItemType.Epic, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Epic, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Epic, To = WorkItemType.TaskItem, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Epic, To = WorkItemType.Bug, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Epic, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.RelatesTo },

                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.Epic, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.TaskItem, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.Bug, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.UserStory, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.RelatesTo },

                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TaskItem, To = WorkItemType.Epic, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TaskItem, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TaskItem, To = WorkItemType.TaskItem, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TaskItem, To = WorkItemType.Bug, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TaskItem, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.RelatesTo },

                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.Epic, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.TaskItem, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.Bug, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.Bug, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.RelatesTo },

                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TestCase, To = WorkItemType.Epic, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TestCase, To = WorkItemType.UserStory, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TestCase, To = WorkItemType.TaskItem, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TestCase, To = WorkItemType.Bug, RelationType = WorkItemRelationType.RelatesTo },
                new WorkItemsRelationRulesMetadataDto { From = WorkItemType.TestCase, To = WorkItemType.TestCase, RelationType = WorkItemRelationType.RelatesTo },
            };
            return Task.FromResult(Result<List<WorkItemsRelationRulesMetadataDto>>.Success(rules, System.Net.HttpStatusCode.OK));
        }
    }
}