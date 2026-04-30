using Application.Features.Articles.DTOs;
using Application.Features.Tags.DTOs;
using AutoMapper;
using Infrastructure.Persistence.Entities;

namespace Infrastructure.Persistence.ReadModels.Profiles
{
    public sealed class ArticlesReadProfile : Profile
    {
        public ArticlesReadProfile()
        {
            CreateMap<TagEntity, TagDto>();

            CreateMap<ArticleEntity, ArticleDto>()
                .ForMember(d => d.CoverImageUrl,
                    o => o.MapFrom(s => s.CoverImage != null ? s.CoverImage.Url : null))
                .ForMember(d => d.CoverImagePublicId,
                    o => o.MapFrom(s => s.CoverImage != null ? s.CoverImage.PublicId : null))
                .ForMember(d => d.CreatedByUserName,
                    o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : ""))
                .ForMember(d => d.CreatedByUserProfileImageUrl,
                    o => o.MapFrom(s => s.CreatedByUser.ProfileImage != null
                        ? s.CreatedByUser.ProfileImage.Url
                        : null))
                .ForMember(d => d.Tags,
                    o => o.MapFrom(s => s.Tags))
                .ForMember(d => d.LikesCount,
                    o => o.MapFrom(s => s.Likes.Count))
                .ForMember(d => d.CommentsCount,
                    o => o.MapFrom(s => s.Comments.Count))
                .ForMember(d => d.IsLikedByCurrentUser,
                    o => o.Ignore());

            CreateMap<ArticleEntity, ArticleDetailsDto>()
                .ForMember(d => d.CoverImageUrl,
                    o => o.MapFrom(s => s.CoverImage != null ? s.CoverImage.Url : null))
                .ForMember(d => d.CoverImagePublicId,
                    o => o.MapFrom(s => s.CoverImage != null ? s.CoverImage.PublicId : null))
                .ForMember(d => d.CreatedByUserName,
                    o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : ""))
                .ForMember(d => d.CreatedByUserProfileImageUrl,
                    o => o.MapFrom(s => s.CreatedByUser.ProfileImage != null
                        ? s.CreatedByUser.ProfileImage.Url
                        : null))
                .ForMember(d => d.Tags,
                    o => o.MapFrom(s => s.Tags))
                .ForMember(d => d.LikesCount,
                    o => o.MapFrom(s => s.Likes.Count))
                .ForMember(d => d.CommentsCount,
                    o => o.MapFrom(s => s.Comments.Count))
                .ForMember(d => d.IsLikedByCurrentUser,
                    o => o.Ignore());

            CreateMap<ArticleCommentEntity, ArticleCommentDto>()
                .ForMember(d => d.CreatedByUserName,
                    o => o.MapFrom(s => s.CreatedByUser != null ? s.CreatedByUser.FullName : ""))
                .ForMember(d => d.CreatedByUserProfileImageUrl,
                    o => o.MapFrom(s => s.CreatedByUser.ProfileImage != null
                        ? s.CreatedByUser.ProfileImage.Url
                        : null));
        }
    }
}

