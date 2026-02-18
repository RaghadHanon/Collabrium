using Application.Common;
using Application.Features.Metadata.DTOs;
using Application.Interfaces.ReadModels;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Metadata.Queries;

public class GetWorkItemsWorkFlowMetadata

{
    public class Query : IRequest<Result<List<WorkItemWorkflowDto>>>
    {
    }

    public class Handler(
        IWorkItemsReadModel workItemsReadModel
      ) : IRequestHandler<Query, Result<List<WorkItemWorkflowDto>>>
    {
        public async Task<Result<List<WorkItemWorkflowDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var workflows = new List<WorkItemWorkflowDto>
            {
                new WorkItemWorkflowDto
                {
                    Type = WorkItemType.Epic,
                    Workflow = workItemsReadModel.GetWorkItemWorkflowByType(WorkItemType.Epic)
                },

                new WorkItemWorkflowDto
                {
                    Type = WorkItemType.UserStory,
                    Workflow = workItemsReadModel.GetWorkItemWorkflowByType(WorkItemType.UserStory)
                },

                new WorkItemWorkflowDto
                {
                    Type = WorkItemType.TestCase,
                    Workflow = workItemsReadModel.GetWorkItemWorkflowByType(WorkItemType.TestCase)
                },

                new WorkItemWorkflowDto
                {
                    Type = WorkItemType.TaskItem,
                    Workflow = workItemsReadModel.GetWorkItemWorkflowByType(WorkItemType.TaskItem)
                },

                new WorkItemWorkflowDto
                {
                    Type = WorkItemType.Bug,
                    Workflow = workItemsReadModel.GetWorkItemWorkflowByType(WorkItemType.Bug)
                }
            };

            return Result<List<WorkItemWorkflowDto>>.Success(workflows, HttpStatusCode.OK);
        }
    }
}