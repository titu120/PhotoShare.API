using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    /// <summary>
    /// ইউজার ফলো, আনফলো, ফলোয়িং স্ট্যাটাস চেক, মিউচুয়াল এবং সাজেস্টেড ইউজার সংক্রান্ত সকল লজিক হ্যান্ডেল করার কন্ট্রোলার।
    /// এই কন্ট্রোলারের সব API-এর বেস রুট হবে: api/Follow
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FollowController : ControllerBase
    {
        // ডেটাবেসের সাথে যোগাযোগের জন্য প্রধান মাধ্যম (Entity Framework Core DbContext)
        private readonly AppDbContext _context;

        public FollowController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// নির্দিষ্ট কোনো ইউজারকে ফলো করার এন্ডপয়েন্ট। 
        /// এখানে নিজেকে নিজে ফলো করা বা একই ইউজারকে দুইবার ফলো করার বিরুদ্ধে সুরক্ষা দেওয়া আছে।
        /// </summary>
        /// <param name="id">যাকে ফলো করা হবে তার ইউজার আইডি</param>
        [Authorize]
        [HttpPost("{id}")]
        public async Task<IActionResult> FollowUser(string id)
        {
            // টোকেন থেকে বর্তমান লগইন করা ইউজারের ইউনিক আইডি বের করা হচ্ছে
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ভ্যালিডেশন ১: ইউজার যেন নিজেকে নিজে ফলো করতে না পারে
            if (currentUserId == id)
                return BadRequest(new { message = "নিজেকে নিজে Follow করা যাবে না" });

            // ভ্যালিডেশন ২: যাকে ফলো করা হচ্ছে সে ডেটাবেসে আদতে আছে কি না চেক করা
            var targetUserExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!targetUserExists)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // ভ্যালিডেশন ৩: ইতিপূর্বে তাকে ফলো করা আছে কি না তা চেক করা
            var alreadyFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            if (alreadyFollowing)
                return BadRequest(new { message = "আপনি ইতিমধ্যে এই user কে Follow করছেন" });

            // নতুন ফলো অবজেক্ট তৈরি
            var follow = new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = currentUserId,
                FollowingId = id,
                CreatedAt = DateTime.UtcNow
            };

            // ডেটাবেসে ফলো রেকর্ড যুক্ত করা এবং সেভ করা
            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Follow করা হয়েছে" });
        }

        /// <summary>
        /// পূর্বে করা ফলো তুলে নেওয়ার (Unfollow) এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="id">যাকে আনফলো করা হবে তার ইউজার আইডি</param>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> UnfollowUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ডেটাবেস থেকে সংশ্লিষ্ট ফলো রেকর্ডটি খুঁজে বের করা
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            if (follow == null)
                return NotFound(new { message = "আপনি এই user কে Follow করেননি" });

            // ফলো রেকর্ড রিমুভ করে ডেটাবেস আপডেট করা
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Unfollow করা হয়েছে" });
        }

        /// <summary>
        /// নির্দিষ্ট কোনো ইউজার অন্য কোনো ইউজারকে ফলো করে কি না, তা বুলিয়ান (true/false) আকারে চেক করার এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="id">যে ফলো করছে তার আইডি</param>
        ///_param name="targetId">যাকে চেক করা হবে তার আইডি</param>
        [HttpGet("{id}/is-following/{targetId}")]
        public async Task<IActionResult> IsFollowing(string id, string targetId)
        {
            var isFollowing = await _context.Follows
                .AnyAsync(f => f.FollowerId == id && f.FollowingId == targetId);

            return Ok(new { isFollowing });
        }

        /// <summary>
        /// নির্দিষ্ট কোনো ইউজারের সাথে যাদের মিউচুয়াল ফলো (পরস্পরকে ফলো করে) আছে, তাদের তালিকা বের করার এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="id">ইউজারের আইডি</param>
        [HttpGet("{id}/mutual")]
        public async Task<IActionResult> GetMutualFollows(string id)
        {
            // এই ইউজার যাদের ফলো করছে তাদের আইডির তালিকা
            var following = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // এই ইউজারকে যারা ফলো করছে (তার ফলোয়ার্স) তাদের আইডির তালিকা
            var followers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.FollowerId)
                .ToListAsync();

            // দুটি তালিকার ইন্টারসেকশন বা কমন আইডিগুলো বের করা (যাঁরা উভয় লিস্টে আছেন)
            var mutualIds = following.Intersect(followers).ToList();

            // প্রাপ্ত আইডিগুলোর বিপরীতে ইউজারদের বিস্তারিত তথ্য ডেটাবেس থেকে তুলে আনা
            var mutualUsers = await _context.Users
                .Where(u => mutualIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(mutualUsers);
        }

        /// <summary>
        /// ইউজারের ফলোয়াররা যাদের ফলো করে, কিন্তু এই ইউজার নিজে এখনো ফলো করে না—এমন অ্যাকাউন্ট সাজেস্ট করার এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="id">ইউজারের আইডি</param>
        [HttpGet("{id}/suggested")]
        public async Task<IActionResult> GetSuggestedUsers(string id)
        {
            // এই ইউজারকে যারা ফলো করে (তার ফলোয়ারদের আইডি)
            var myFollowers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.FollowerId)
                .ToListAsync();

            // এই ইউজার নিজে যাদের ফলো করছে (এদের সাজেশন থেকে বাদ দিতে হবে)
            var alreadyFollowing = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // আমার ফলোয়াররা যাদের ফলো করে, তাদের সবার আইডি সংগ্রহ করা
            var followersOfMyFollowers = await _context.Follows
                .Where(f => myFollowers.Contains(f.FollowerId))
                .Select(f => f.FollowingId)
                .ToListAsync();

            // সাজেশনের জন্য ফিল্টারিং: ডুপ্লিকেট বাদ দেওয়া, নিজেকে বাদ দেওয়া এবং যাদের অলরেডি ফলো করছি তাদের বাদ দেওয়া
            var suggestedIds = followersOfMyFollowers
                .Distinct()
                .Where(userId => userId != id)
                .Where(userId => !alreadyFollowing.Contains(userId))
                .ToList();

            // চূড়ান্ত সাজেস্টেড ইউজারদের তথ্য ডেটাবেস থেকে তুলে আনা
            var suggestedUsers = await _context.Users
                .Where(u => suggestedIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(suggestedUsers);
        }
    }
}