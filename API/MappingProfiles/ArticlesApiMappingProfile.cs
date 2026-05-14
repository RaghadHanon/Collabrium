using API.Contracts.Articles;
using Application.Features.Articles.Commands;
using Application.Features.Articles.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public sealed class ArticlesApiMappingProfile : Profile
    {
        public ArticlesApiMappingProfile()
        {
            CreateMap<CreateArticleRequest, CreateArticle.Command>();

            CreateMap<UpdateArticleRequest, UpdateArticle.Command>();

            CreateMap<AddArticleCommentRequest, AddArticleComment.Command>();

            CreateMap<UpdateArticleCommentRequest, UpdateArticleComment.Command>();

            CreateMap<SetArticleTagsRequest, SetArticleTags.Command>();

            CreateMap<AddArticleTagsRequest, AddArticleTags.Command>();

            CreateMap<RemoveArticleTagsRequest, RemoveArticleTags.Command>();

            CreateMap<GetArticlesRequest, GetArticles.Query>();
            CreateMap<GetArticleCommentsRequest, GetArticleComments.Query>();
            CreateMap<GetArticleLikesRequest, GetArticleLikes.Query>();
        }
    }
}

