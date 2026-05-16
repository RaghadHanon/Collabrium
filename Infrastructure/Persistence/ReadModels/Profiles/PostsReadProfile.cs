using Application.Features.Posts.DTOs;
using Application.Features.Tags.DTOs;
using Application.Features.Users.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles;

public sealed class PostsReadProfile : Profile
{
    public PostsReadProfile()
    {
        CreateMap<TagEntity, TagDto>();

        CreateMap<ApplicationUserEntity, UserDto>()
            .ForMember(d => d.ProfileImageUrl,
                o => o.MapFrom(s =>
                    s.ProfileImage != null
                        ? s.ProfileImage.Url
                        : null));

        CreateMap<PostCommentEntity, PostCommentDto>()
            .ForMember(d => d.CreatedByUserName,
                o => o.MapFrom(s => s.CreatedByUser.UserName))
            .ForMember(d => d.CreatedByUserProfileImageUrl,
                o => o.MapFrom(s =>
                    s.CreatedByUser.ProfileImage != null
                        ? s.CreatedByUser.ProfileImage.Url
                        : null));

        CreateMap<StoredImageOwned, PostImageDto>();

        CreateMap<PostEntity, PostDto>()
            .ForMember(d => d.CreatedByUser,
                o => o.MapFrom(s => s.CreatedByUser))

            .ForMember(d => d.PinnedByUserName,
                o => o.MapFrom(s =>
                    s.PinnedByUser != null
                        ? s.PinnedByUser.FullName ?? s.PinnedByUser.UserName
                        : null))

            .ForMember(d => d.Tags,
                o => o.MapFrom(s => s.Tags))

            .ForMember(d => d.Links,
                o => o.MapFrom(s => s.Links.Select(x => x.Url)))

            .ForMember(d => d.Images,
                o => o.MapFrom(s => s.Images))

            .ForMember(d => d.LikesCount,
                o => o.MapFrom(s => s.Likes.Count))

            .ForMember(d => d.CommentsCount,
                o => o.MapFrom(s => s.Comments.Count));
    }
}