using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FollowController(AppDbContext context)
        {
            _context = context;
        }

        // URL: POST api/Follow/{id}
        // কাজ: একজন user কে Follow করা, নিজেকে নিজে বা দুইবার Follow করা যাবে না
        [Authorize]
        [HttpPost("{id}")]
        public async Task<IActionResult> FollowUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == id)
                return BadRequest(new { message = "নিজেকে নিজে Follow করা যাবে না" });

            var targetUserExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!targetUserExists)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // Validation: আগে থেকেই Follow করা আছে কিনা check করা হচ্ছে
            var alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            if (alreadyFollowing)
                return BadRequest(new { message = "আপনি ইতিমধ্যে এই user কে Follow করছেন" });

            var follow = new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = currentUserId,
                FollowingId = id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Follow করা হয়েছে" });
        }

        // URL: DELETE api/Follow/{id}
        // কাজ: আগে করা Follow তুলে নেওয়া (Unfollow)
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> UnfollowUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // এই Follow record টা খোঁজা হচ্ছে
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            if (follow == null)
                return NotFound(new { message = "আপনি এই user কে Follow করেননি" });

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Unfollow করা হয়েছে" });
        }



    }
}