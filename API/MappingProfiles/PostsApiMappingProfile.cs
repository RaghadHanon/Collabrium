using API.Contracts.Posts;
using Application.Features.Posts.Commands;
using Application.Features.Posts.Queries;
using AutoMapper;

namespace API.MappingProfiles
{
    public sealed class PostsApiMappingProfile : Profile
    {
        public PostsApiMappingProfile()
        {
            CreateMap<CreatePostRequest, CreatePost.Command>();
            CreateMap<UpdatePostRequest, UpdatePost.Command>();

            CreateMap<GetPostsRequest, GetPosts.Query>();
            CreateMap<GetPostCommentsRequest, GetPostComments.Query>();

            CreateMap<AddPostCommentRequest, AddPostComment.Command>();
            CreateMap<UpdatePostCommentRequest, UpdatePostComment.Command>();

            CreateMap<PostTagsRequest, AddPostTags.Command>();
            CreateMap<PostTagsRequest, RemovePostTags.Command>();
            CreateMap<PostTagsRequest, SetPostTags.Command>();
        }
    }
}
