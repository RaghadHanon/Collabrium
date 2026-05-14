using Application.Common;
using Domain.Enums;
using MediatR;
using System.Net;
using System.Text.RegularExpressions;

namespace Application.Features.Metadata.Queries;

public class GetEnumMetaData
{
    public class Query : IRequest<Result<string[]>>
    {
        public MetadataType MetadataType { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<string[]>>
    {
        public Task<Result<string[]>> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = request.MetadataType switch
            {
                MetadataType.UserRole => Result<string[]>.Success(
                    Enum.GetNames(typeof(UserRole)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.Department => Result<string[]>.Success(
                    Enum.GetNames(typeof(Department)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.TestEnvironment => Result<string[]>.Success(
                    Enum.GetNames(typeof(TestEnvironment)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.AttachmentType => Result<string[]>.Success(
                    Enum.GetNames(typeof(AttachmentType)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.SprintStatus => Result<string[]>.Success(
                    Enum.GetNames(typeof(SprintStatus)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.ProjectStatus => Result<string[]>.Success(
                    Enum.GetNames(typeof(ProjectStatus)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.CodeLinkType => Result<string[]>.Success(
                    Enum.GetNames(typeof(CodeLinkType)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.TestCaseResult => Result<string[]>.Success(
                    Enum.GetNames(typeof(TestCaseResult)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.WorkItemStatus => Result<string[]>.Success(
                    Enum.GetNames(typeof(WorkItemStatus)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.WorkItemPriority => Result<string[]>.Success(
                    Enum.GetNames(typeof(WorkItemPriority)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.WorkItemRelationType => Result<string[]>.Success(
                    Enum.GetNames(typeof(WorkItemRelationType)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.WorkItemType => Result<string[]>.Success(
                    Enum.GetNames(typeof(WorkItemType)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.RoadmapPath => Result<string[]>.Success(
                    Enum.GetNames(typeof(RoadmapPath)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.RoadmapLevel => Result<string[]>.Success(
                    Enum.GetNames(typeof(RoadmapLevel)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.RoadmapResourceType => Result<string[]>.Success(
                    Enum.GetNames(typeof(RoadmapResourceType)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.CourseLevel => Result<string[]>.Success(
                    Enum.GetNames(typeof(CourseLevel)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.CourseLanguage => Result<string[]>.Success(
                    Enum.GetNames(typeof(CourseLanguage)).Select(AddSpace).ToArray(),
                    HttpStatusCode.OK),

                MetadataType.UserCourseStatus => Result<string[]>.Success(
               Enum.GetNames(typeof(UserCourseStatus)).Select(AddSpace).ToArray(),
               HttpStatusCode.OK),
                MetadataType.PostType => Result<string[]>.Success(
              Enum.GetNames(typeof(PostType)).Select(AddSpace).ToArray(),
              HttpStatusCode.OK),
                _ => throw new NotImplementedException()
            };

            return Task.FromResult(result);
        }

        private static string AddSpace(string value)
        {
            var spaced = Regex.Replace(value, "(\\B[A-Z])", " $1");
            return value.Length > 3 ? spaced : value;
        }
    }
}