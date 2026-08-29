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
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            // Token থেকে বর্তমান logged-in user এর ID বের করা হচ্ছে
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Post টা সত্যিই আছে কিনা check করা হচ্ছে
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            // নতুন Like তৈরি করা হচ্ছে
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
    }
}