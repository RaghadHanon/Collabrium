using API.Contracts.Articles;
using API.SetupProgram;
using Application.Common;
using Application.Features.Articles.Commands;
using Application.Features.Articles.DTOs;
using Application.Features.Articles.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/articles")]
    [ApiController]
    [Authorize(Policy = Policies.PasswordChanged)]
    public sealed class ArticlesController : ApiBaseController
    {
        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet]
        public async Task<ActionResult<PagedList<ArticleDto, DateTime?>>> GetArticles(
            [FromQuery] GetArticlesRequest request)
        {
            var query = Mapper.Map<GetArticles.Query>(request);
            query.IsPublished = true;

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("{articleId}")]
        public async Task<ActionResult<ArticleDetailsDto>> GetArticleDetails(string articleId)
        {
            var query = new GetArticleDetails.Query
            {
                ArticleId = articleId
            };

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("{articleId}/comments")]
        public async Task<ActionResult<PagedList<ArticleCommentDto, DateTime?>>> GetArticleComments(
            string articleId,
            [FromQuery] GetArticleCommentsRequest request)
        {
            var query = Mapper.Map<GetArticleComments.Query>(request);
            query.ArticleId = articleId;

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpGet("{articleId}/likes")]
        public async Task<ActionResult<PagedList<ArticleLikeUserDto, DateTime?>>> GetArticleLikes(
            string articleId,
            [FromQuery] GetArticleLikesRequest request)
        {
            var query = Mapper.Map<GetArticleLikes.Query>(request);
            query.ArticleId = articleId;

            return HandleResult(await Mediator.Send(query));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("article")]
        public async Task<ActionResult<ArticleDetailsDto>> CreateArticle(CreateArticleRequest request)
        {
            var command = Mapper.Map<CreateArticle.Command>(request);

            return HandleResult(await Mediator.Send(command), nameof(GetArticleDetails), "Articles");
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPatch("article/{articleId}")]
        public async Task<ActionResult<ArticleDetailsDto>> UpdateArticle(
            string articleId,
            UpdateArticleRequest request)
        {
            var command = Mapper.Map<UpdateArticle.Command>(request);
            command.ArticleId = articleId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpDelete("article/{articleId}")]
        public async Task<ActionResult<string>> DeleteArticle(string articleId)
        {
            var command = new DeleteArticle.Command
            {
                ArticleId = articleId
            };

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPut("article/{articleId}/tags")]
        public async Task<ActionResult<ArticleDetailsDto>> SetArticleTags(
            string articleId,
            SetArticleTagsRequest request)
        {
            var command = Mapper.Map<SetArticleTags.Command>(request);
            command.ArticleId = articleId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("article/{articleId}/tags")]
        public async Task<ActionResult<ArticleDetailsDto>> AddArticleTags(
            string articleId,
            AddArticleTagsRequest request)
        {
            var command = Mapper.Map<AddArticleTags.Command>(request);
            command.ArticleId = articleId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpDelete("article/{articleId}/tags")]
        public async Task<ActionResult<ArticleDetailsDto>> RemoveArticleTags(
            string articleId,
            RemoveArticleTagsRequest request)
        {
            var command = Mapper.Map<RemoveArticleTags.Command>(request);
            command.ArticleId = articleId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpPost("article/{articleId}/like")]
        public async Task<ActionResult<ArticleDto>> LikeArticle(string articleId)
        {
            var command = new LikeArticle.Command
            {
                ArticleId = articleId
            };

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpDelete("article/{articleId}/like")]
        public async Task<ActionResult<ArticleDto>> UnlikeArticle(string articleId)
        {
            var command = new UnlikeArticle.Command
            {
                ArticleId = articleId
            };

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpPost("article/{articleId}/comments")]
        public async Task<ActionResult<ArticleCommentDto>> AddArticleComment(
            string articleId,
            AddArticleCommentRequest request)
        {
            var command = Mapper.Map<AddArticleComment.Command>(request);
            command.ArticleId = articleId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpPatch("article/{articleId}/comments/{commentId}")]
        public async Task<ActionResult<ArticleCommentDto>> UpdateArticleComment(
            string articleId,
            string commentId,
            UpdateArticleCommentRequest request)
        {
            var command = Mapper.Map<UpdateArticleComment.Command>(request);
            command.ArticleId = articleId;
            command.CommentId = commentId;

            return HandleResult(await Mediator.Send(command));
        }

        [Authorize(Policy = Policies.Roadmaps_Read)]
        [HttpDelete("article/{articleId}/comments/{commentId}")]
        public async Task<ActionResult<string>> DeleteArticleComment(
            string articleId,
            string commentId)
        {
            var command = new DeleteArticleComment.Command
            {
                ArticleId = articleId,
                CommentId = commentId
            };

            return HandleResult(await Mediator.Send(command));
        }
        /// <summary>
        /// Uploads or replaces the cover image of an article.
        /// </summary>
        /// <param name="articleId">The article id.</param>
        /// <param name="file">The cover image file.</param>
        /// <returns>No content if uploaded successfully.</returns>
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpPost("{articleId}/cover-image")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult> UploadCoverImage(
            [FromRoute] string articleId,
            IFormFile file)
        {
            var command = new UploadArticleCoverImage.Command
            {
                ArticleId = articleId,
                File = file
            };

            return HandleResult(await Mediator.Send(command));
        }
        [Authorize(Policy = Policies.Roadmaps_Manage)]
        [HttpGet("manage")]
        public async Task<ActionResult<PagedList<ArticleDto, DateTime?>>> GetManageArticles(
         [FromQuery] GetArticlesRequest request)
        {
            var query = Mapper.Map<GetArticles.Query>(request);

            return HandleResult(await Mediator.Send(query));
        }
    }
}
