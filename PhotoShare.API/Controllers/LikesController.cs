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

        public LikesController(AppDbContext context)
        {
            _context = context;
        }

        // URL: POST api/Likes/{postId}
        // কাজ: একটা Post এ Like দেওয়া, শুধু login করা user-ই পারবে
        // একই user একই Post দুইবার Like দিতে পারবে না
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            // Validation: এই user আগে থেকেই এই Post এ Like দিয়েছে কিনা check করা হচ্ছে
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

            // এই user এর এই Post এ দেওয়া Like টা খোঁজা হচ্ছে
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null)
                return NotFound(new { message = "আপনি এই Post এ Like দেননি" });

            // Like মুছে ফেলা হচ্ছে
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

            // সেই UserId গুলো দিয়ে আসল User এর তথ্য (নাম) বের করা হচ্ছে
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(users);
        }






    }
}