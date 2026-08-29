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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Post টা আছে কিনা check করা হচ্ছে
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
        // কাজ: একটা Post এর সব Comment দেখানো, পুরাতন থেকে নতুন ক্রমে
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)   // পুরাতন থেকে নতুন (OrderByDescending না, সাধারণ OrderBy)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.UserId,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }






    }
}