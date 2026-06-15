using API.Contracts.Posts;
using API.SetupProgram;
using Application.Common;
using Application.Features.Posts.Commands;
using Application.Features.Posts.DTOs;
using Application.Features.Posts.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;


[Route("api/posts")]
[Authorize(Policy = Policies.PasswordChanged)]
public sealed class PostsController : ApiBaseController
{
    /// <summary>
    /// Retrieves posts.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/posts?pageSize=10
    ///
    /// </remarks>
    /// <response code="200">Posts retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpGet]
    public async Task<ActionResult<PagedList<PostDto, PostsCursorDto?>>> GetPosts(
    [FromQuery] GetPostsRequest request)
    {
        var query = Mapper.Map<GetPosts.Query>(request);

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves a post by id.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/posts/{postId}
    ///
    /// </remarks>
    /// <response code="200">Post retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpGet("{postId}")]
    public async Task<ActionResult<PostDto>> GetPostById(string postId)
    {
        var query = new GetPostById.Query
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Retrieves post details including comments.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/posts/{postId}/details
    ///
    /// </remarks>
    /// <response code="200">Post details retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpGet("{postId}/details")]
    public async Task<ActionResult<PostDetailsDto>> GetPostDetails(string postId)
    {
        var query = new GetPostDetails.Query
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Creates a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/posts
    ///     {
    ///       "title": "Useful ASP.NET Core resource",
    ///       "content": "This helped me understand clean architecture.",
    ///       "type": "Resource",
    ///       "tagIds": ["tag-id-1", "tag-id-2"],
    ///       "links": ["https://learn.microsoft.com"]
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Post created successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPost]
    public async Task<ActionResult<PostDto>> CreatePost(CreatePostRequest request)
    {
        var command = Mapper.Map<CreatePost.Command>(request);

        return HandleResult(
            await Mediator.Send(command),
            nameof(GetPostById),
            "Posts");
    }

    /// <summary>
    /// Updates a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/posts/{postId}
    ///     {
    ///       "title": "Updated title",
    ///       "content": "Updated content",
    ///       "type": "Discussion",
    ///       "links": []
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Post updated successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to update the post.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPatch("{postId}")]
    public async Task<ActionResult<PostDto>> UpdatePost(
        string postId,
        UpdatePostRequest request)
    {
        var command = Mapper.Map<UpdatePost.Command>(request);
        command.PostId = postId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/posts/{postId}
    ///
    /// </remarks>
    /// <response code="200">Post deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to delete the post.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpDelete("{postId}")]
    public async Task<ActionResult> DeletePost(string postId)
    {
        var command = new DeletePost.Command
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Uploads images to a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/posts/{postId}/images
    ///     Content-Type: multipart/form-data
    ///
    /// </remarks>
    /// <response code="200">Images uploaded successfully.</response>
    /// <response code="400">If no images were uploaded or max images exceeded.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to modify the post.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPost("{postId}/images")]
    public async Task<ActionResult<PostDto>> UploadPostImages(
        string postId,
        [FromForm] List<IFormFile> images)
    {
        var command = new UploadPostImages.Command
        {
            PostId = postId,
            Images = images
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Removes an image from a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/posts/{postId}/images?storagePath=Collabrium/PostImages/example
    ///
    /// </remarks>
    /// <response code="200">Image removed successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to modify the post.</response>
    /// <response code="404">If tenant, post, or image not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpDelete("{postId}/images")]
    public async Task<ActionResult<PostDto>> RemovePostImage(
        string postId,
        [FromQuery] string storagePath)
    {
        var command = new RemovePostImage.Command
        {
            PostId = postId,
            StoragePath = storagePath
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Pins a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/posts/{postId}/pin
    ///
    /// </remarks>
    /// <response code="200">Post pinned successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to pin posts.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Manage)]
    [HttpPost("{postId}/pin")]
    public async Task<ActionResult<PostDto>> PinPost(string postId)
    {
        var command = new PinPost.Command
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Unpins a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/posts/{postId}/pin
    ///
    /// </remarks>
    /// <response code="200">Post unpinned successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to unpin posts.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Manage)]
    [HttpDelete("{postId}/pin")]
    public async Task<ActionResult<PostDto>> UnpinPost(string postId)
    {
        var command = new UnpinPost.Command
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Likes a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/posts/{postId}/like
    ///
    /// </remarks>
    /// <response code="200">Post liked successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPost("{postId}/like")]
    public async Task<ActionResult<PostDto>> LikePost(string postId)
    {
        var command = new LikePost.Command
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Removes the current user's like from a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/posts/{postId}/like
    ///
    /// </remarks>
    /// <response code="200">Post unliked successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpDelete("{postId}/like")]
    public async Task<ActionResult<PostDto>> UnlikePost(string postId)
    {
        var command = new UnlikePost.Command
        {
            PostId = postId
        };

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Adds tags to a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/posts/{postId}/tags
    ///     {
    ///       "tagIds": ["tag-id-1", "tag-id-2"]
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Tags added successfully.</response>
    /// <response code="400">If max tags exceeded.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to modify the post.</response>
    /// <response code="404">If tenant, post, or tags not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPost("{postId}/tags")]
    public async Task<ActionResult<PostDto>> AddPostTags(
        string postId,
        PostTagsRequest request)
    {
        var command = Mapper.Map<AddPostTags.Command>(request);
        command.PostId = postId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Replaces all tags of a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/posts/{postId}/tags
    ///     {
    ///       "tagIds": ["tag-id-1", "tag-id-2"]
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Tags updated successfully.</response>
    /// <response code="400">If max tags exceeded.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to modify the post.</response>
    /// <response code="404">If tenant, post, or tags not found.</response>
    [Authorize(Policy = Policies.Posts_Manage)]
    [HttpPut("{postId}/tags")]
    public async Task<ActionResult<PostDto>> SetPostTags(
        string postId,
        PostTagsRequest request)
    {
        var command = Mapper.Map<SetPostTags.Command>(request);
        command.PostId = postId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Removes tags from a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/posts/{postId}/tags
    ///     {
    ///       "tagIds": ["tag-id-1", "tag-id-2"]
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Tags removed successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to modify the post.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpDelete("{postId}/tags")]
    public async Task<ActionResult<PostDto>> RemovePostTags(
        string postId,
        PostTagsRequest request)
    {
        var command = Mapper.Map<RemovePostTags.Command>(request);
        command.PostId = postId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Retrieves post comments with pagination.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/posts/{postId}/comments?pageSize=10
    ///
    /// </remarks>
    /// <response code="200">Comments retrieved successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpGet("{postId}/comments")]
    public async Task<ActionResult<PagedList<PostCommentDto, DateTime?>>> GetPostComments(
        string postId,
        [FromQuery] GetPostCommentsRequest request)
    {
        var query = Mapper.Map<GetPostComments.Query>(request);
        query.PostId = postId;

        return HandleResult(await Mediator.Send(query));
    }

    /// <summary>
    /// Adds a comment to a post.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/posts/{postId}/comments
    ///     {
    ///       "content": "Great post!"
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Comment added successfully.</response>
    /// <response code="400">If request data is invalid.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="404">If tenant or post not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPost("{postId}/comments")]
    public async Task<ActionResult<PostCommentDto>> AddPostComment(
        string postId,
        AddPostCommentRequest request)
    {
        var command = Mapper.Map<AddPostComment.Command>(request);
        command.PostId = postId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Updates a post comment.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     PATCH /api/posts/{postId}/comments/{commentId}
    ///     {
    ///       "content": "Updated comment"
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Comment updated successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to update the comment.</response>
    /// <response code="404">If tenant, post, or comment not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpPatch("{postId}/comments/{commentId}")]
    public async Task<ActionResult<PostCommentDto>> UpdatePostComment(
        string postId,
        string commentId,
        UpdatePostCommentRequest request)
    {
        var command = Mapper.Map<UpdatePostComment.Command>(request);
        command.PostId = postId;
        command.CommentId = commentId;

        return HandleResult(await Mediator.Send(command));
    }

    /// <summary>
    /// Deletes a post comment.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     DELETE /api/posts/{postId}/comments/{commentId}
    ///
    /// </remarks>
    /// <response code="200">Comment deleted successfully.</response>
    /// <response code="401">If the user is not authenticated.</response>
    /// <response code="403">If the user is not allowed to delete the comment.</response>
    /// <response code="404">If tenant, post, or comment not found.</response>
    [Authorize(Policy = Policies.Posts_Read)]
    [HttpDelete("{postId}/comments/{commentId}")]
    public async Task<ActionResult> DeletePostComment(
        string postId,
        string commentId)
    {
        var command = new DeletePostComment.Command
        {
            PostId = postId,
            CommentId = commentId
        };

        return HandleResult(await Mediator.Send(command));
    }
}