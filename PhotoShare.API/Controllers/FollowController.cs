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

        // URL: GET api/Follow/{id}/is-following/{targetId}
        // কাজ: {id} user টা {targetId} user কে Follow করে কিনা চেক করা
        [HttpGet("{id}/is-following/{targetId}")]
        public async Task<IActionResult> IsFollowing(string id, string targetId)
        {
            var isFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == id && f.FollowingId == targetId);

            return Ok(new { isFollowing });
        }

        // URL: GET api/Follow/{id}/mutual
        // কাজ: এই user এর সাথে যাদের Mutual Follow আছে (একে অপরকে Follow করছে) তাদের list বের করা
        [HttpGet("{id}/mutual")]
        public async Task<IActionResult> GetMutualFollows(string id)
        {
            // এই user যাদের Follow করছে, তাদের ID list
            var following = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // এই user কে যারা Follow করছে, তাদের ID list
            var followers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.FollowerId)
                .ToListAsync();

            // দুই list এর মধ্যে যেগুলো "কমন" (উভয় জায়গায় আছে) সেগুলোই Mutual
            var mutualIds = following.Intersect(followers).ToList();

            // সেই ID গুলো দিয়ে আসল User তথ্য বের করা হচ্ছে
            var mutualUsers = await _context.Users
                .Where(u => mutualIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(mutualUsers);
        }


        // URL: GET api/Follow/{id}/suggested
        // কাজ: এই user এর followers রা যাদের follow করে, কিন্তু এই user নিজে follow করে না — তাদের suggest করা
        [HttpGet("{id}/suggested")]
        public async Task<IActionResult> GetSuggestedUsers(string id)
        {
            // এই user কে যারা Follow করছে (আমার followers)
            var myFollowers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.FollowerId)
                .ToListAsync();

            // এই user নিজে যাদের Follow করছে (এইগুলো বাদ দিতে হবে suggestion থেকে)
            var alreadyFollowing = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // আমার followers-রা যাদের Follow করে, তাদের সবার ID (duplicate সহ হতে পারে)
            var followersOfMyFollowers = await _context.Follows
                .Where(f => myFollowers.Contains(f.FollowerId))
                .Select(f => f.FollowingId)
                .ToListAsync();

            // Suggestion তালিকা তৈরি: followers-দের follow করা মানুষদের মধ্যে যারা...
            var suggestedIds = followersOfMyFollowers
                .Distinct()                              // duplicate বাদ (একজনকে বার বার suggest না করা)
                .Where(userId => userId != id)            // নিজেকে suggest না করা
                .Where(userId => !alreadyFollowing.Contains(userId))  // যাদের আগে থেকেই follow করছি, তাদের বাদ
                .ToList();

            var suggestedUsers = await _context.Users
                .Where(u => suggestedIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(suggestedUsers);
        }




    }
}