using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    // এই Controller এর সব URL শুরু হবে api/Likes দিয়ে
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LikesController(AppDbContext context)
        {
            _context = context;
        }

        // URL: POST api/Likes/{postId}
        // কাজ: একটা Post এ Like দেওয়া, একই user দুইবার Like দিতে পারবে না
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            // Validation: আগে থেকেই Like দেওয়া আছে কিনা
            var alreadyLiked = await _context.Likes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);

            if (alreadyLiked)
                return BadRequest(new { message = "আপনি ইতিমধ্যে এই Post এ Like দিয়েছেন" });

            var like = new Like
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post এ Like দেওয়া হয়েছে" });
        }

        // URL: DELETE api/Likes/{postId}
        // কাজ: আগে দেওয়া Like তুলে নেওয়া (Unlike)
        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> UnlikePost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null)
                return NotFound(new { message = "আপনি এই Post এ Like দেননি" });

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Unlike করা হয়েছে" });
        }

        // URL: GET api/Likes/{postId}
        // কাজ: একটা Post এ কারা কারা Like দিয়েছে তাদের list দেখানো
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostLikes(Guid postId)
        {
            var userIds = await _context.Likes
                .Where(l => l.PostId == postId)
                .Select(l => l.UserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(users);
        }

        // URL: GET api/Likes/most-liked-user
        // কাজ: বর্তমান logged-in user সবচেয়ে বেশি কার Post এ Like দিয়েছে তা বের করা
        [Authorize]
        [HttpGet("most-liked-user")]
        public async Task<IActionResult> GetMostLikedUser()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // আমার দেওয়া সব Like থেকে, প্রতিটা Post এর মালিক (owner UserId) বের করে,
            // কাকে কতবার Like দিয়েছি তা গণনা করা হচ্ছে
            var result = await _context.Likes
                .Where(l => l.UserId == currentUserId)          // শুধু আমার দেওয়া Like
                .Join(_context.Posts,                            // Post টেবিলের সাথে জোড়া লাগানো (Join)
                      like => like.PostId,
                      post => post.Id,
                      (like, post) => post.UserId)                // শুধু Post এর মালিকের ID নেওয়া
                .GroupBy(ownerId => ownerId)                      // মালিক অনুযায়ী দলে ভাগ করা
                .Select(g => new
                {
                    UserId = g.Key,
                    LikeCount = g.Count()                          // প্রতিটা দলে কতগুলো Like আছে গোনা
                })
                .OrderByDescending(x => x.LikeCount)               // সবচেয়ে বেশি সংখ্যক আগে
                .FirstOrDefaultAsync();

            if (result == null)
                return Ok(new { message = "আপনি এখনো কাউকে Like দেননি" });

            return Ok(result);
        }





    }
}