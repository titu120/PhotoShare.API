using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.DTOs;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // URL: POST api/Comments/{postId}
        // কাজ: একটা Post এ নতুন Comment যোগ করা, শুধু login করা user-ই পারবে
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentDto dto)
        {
            // Validation: Comment খালি হতে পারবে না
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Comment খালি হতে পারবে না" });

            // Validation: Comment ৫০০ character এর বেশি হতে পারবে না
            if (dto.Content.Length > 500)
                return BadRequest(new { message = "Comment ৫০০ character এর বেশি হতে পারবে না" });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.PostId,
                comment.UserId,
                comment.CreatedAt
            });
        }

        // URL: GET api/Comments/{postId}
        // কাজ: একটা Post এর সব Comment দেখানো, commenter এর username/profile picture সহ
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Join(_context.Users,                          // Users টেবিলের সাথে জোড়া লাগানো
                      comment => comment.UserId,                 // Comment এর UserId
                      user => user.Id,                            // Users এর Id
                      (comment, user) => new                      // দুটো মিলিয়ে নতুন shape বানানো
                      {
                          comment.Id,
                          comment.Content,
                          comment.CreatedAt,
                          UserId = user.Id,
                          Username = user.UserName,
                          ProfilePictureUrl = user.ProfilePictureUrl
                      })
                .ToListAsync();

            return Ok(comments);
        }

        // URL: DELETE api/Comments/{id}
        // কাজ: Comment মুছে ফেলা, শুধু যে লিখেছে সে-ই পারবে
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound(new { message = "Comment পাওয়া যায়নি" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Validation: শুধু নিজের Comment-ই delete করা যাবে
            if (comment.UserId != currentUserId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment সফলভাবে মুছে ফেলা হয়েছে" });
        }

        // URL: GET api/Comments/user/{userId}
        // কাজ: একজন নির্দিষ্ট user এর করা সব Comment এর history দেখানো
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCommentHistory(string userId)
        {
            var comments = await _context.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)   // নতুন থেকে পুরাতন (history দেখানোর স্বাভাবিক নিয়ম)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.PostId,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }






    }
}