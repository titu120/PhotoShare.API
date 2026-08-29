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
        // Database এর সাথে কথা বলার টুল
        private readonly AppDbContext _context;

        // In-memory list — সব Notification এখানে সাময়িকভাবে জমা থাকবে
        // App বন্ধ/restart হলে এই list খালি হয়ে যাবে (এখনো Database এ save হয় না)
        private static List<Notification> _notifications = new List<Notification>();

        public LikesController(AppDbContext context)
        {
            _context = context;
        }

        // URL: POST api/Likes/{postId}
        // কাজ: একটা Post এ Like দেওয়া, একই user দুইবার Like দিতে পারবে না
        // Like দেওয়ার সময় Post এর মালিকের জন্য একটা Notification (in-memory) তৈরি হবে
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Post টা খোঁজা হচ্ছে (পুরো object লাগবে, কারণ owner এর UserId দরকার)
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
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

            // Post এর মালিকের জন্য একটা Notification তৈরি করা হচ্ছে (in-memory, Database এ না)
            var notification = new Notification
            {
                Message = $"{userId} আপনার Post এ Like দিয়েছে",
                ToUserId = post.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _notifications.Add(notification);

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
            // এই Post এর সব Like থেকে UserId গুলো বের করা হচ্ছে
            var userIds = await _context.Likes
                .Where(l => l.PostId == postId)
                .Select(l => l.UserId)
                .ToListAsync();

            // সেই UserId গুলো দিয়ে আসল User এর তথ্য বের করা হচ্ছে
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

            // আমার দেওয়া সব Like থেকে Post এর মালিক বের করে, কাকে কতবার Like দিয়েছি গোনা হচ্ছে
            var result = await _context.Likes
                .Where(l => l.UserId == currentUserId)
                .Join(_context.Posts,
                      like => like.PostId,
                      post => post.Id,
                      (like, post) => post.UserId)
                .GroupBy(ownerId => ownerId)
                .Select(g => new
                {
                    UserId = g.Key,
                    LikeCount = g.Count()
                })
                .OrderByDescending(x => x.LikeCount)
                .FirstOrDefaultAsync();

            if (result == null)
                return Ok(new { message = "আপনি এখনো কাউকে Like দেননি" });

            return Ok(result);
        }

        // URL: POST api/Likes/{postId}/toggle
        // কাজ: Like থাকলে Unlike করা, না থাকলে Like করা — একই endpoint দুটোই করে
        [Authorize]
        [HttpPost("{postId}/toggle")]
        public async Task<IActionResult> ToggleLike(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                // আগে থেকে Like ছিল → এখন Unlike করা হচ্ছে
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { liked = false, message = "Unlike করা হয়েছে" });
            }
            else

            {
                // আগে Like ছিল না → এখন নতুন Like দেওয়া হচ্ছে
                var like = new Like
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Likes.Add(like);
                await _context.SaveChangesAsync();
                return Ok(new { liked = true, message = "Like দেওয়া হয়েছে" });
            }
        }
    }
}