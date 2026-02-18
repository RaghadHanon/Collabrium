using Application.Common;
using Domain.Enums;
using MediatR;
using System.Net;

namespace Application.Features.Metadata.Queries;
public class GetEnumMetaData
{
    public class Query : IRequest<Result<string[]>>
    {
        public MetadataType MetadataType { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<string[]>>
    {
        public async Task<Result<string[]>> Handle(Query request, CancellationToken cancellationToken)
        {
            return (request.MetadataType) switch
            {
                MetadataType.UserRole => Result<string[]>.Success(Enum.GetNames(typeof(UserRole)), HttpStatusCode.OK),
                MetadataType.Department => Result<string[]>.Success(Enum.GetNames(typeof(Department)), HttpStatusCode.OK),
                MetadataType.TestEnvironment => Result<string[]>.Success(Enum.GetNames(typeof(TestEnvironment)), HttpStatusCode.OK),
                MetadataType.AttachmentType => Result<string[]>.Success(Enum.GetNames(typeof(AttachmentType)), HttpStatusCode.OK),
                MetadataType.SprintStatus => Result<string[]>.Success(Enum.GetNames(typeof(SprintStatus)), HttpStatusCode.OK),
                MetadataType.ProjectStatus => Result<string[]>.Success(Enum.GetNames(typeof(ProjectStatus)), HttpStatusCode.OK),
                MetadataType.CodeLinkType => Result<string[]>.Success(Enum.GetNames(typeof(CodeLinkType)), HttpStatusCode.OK),
                MetadataType.TestCaseResult => Result<string[]>.Success(Enum.GetNames(typeof(TestCaseResult)), HttpStatusCode.OK),
                MetadataType.WorkItemStatus => Result<string[]>.Success(Enum.GetNames(typeof(WorkItemStatus)), HttpStatusCode.OK),
                MetadataType.WorkItemPriority => Result<string[]>.Success(Enum.GetNames(typeof(WorkItemPriority)), HttpStatusCode.OK),
                MetadataType.WorkItemRelationType => Result<string[]>.Success(Enum.GetNames(typeof(WorkItemRelationType)), HttpStatusCode.OK),
                MetadataType.WorkItemType => Result<string[]>.Success(Enum.GetNames(typeof(WorkItemType)), HttpStatusCode.OK),
                _ => throw new NotImplementedException()
            };
        }
    }
}
