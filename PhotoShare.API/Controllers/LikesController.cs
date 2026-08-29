using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    /// <summary>
    /// লাইক, আনলাইক, পোস্টের লাইক লিস্ট এবং টগলিং সংক্রান্ত সকল রিকোয়েস্ট হ্যান্ডেল করার কন্ট্রোলার।
    /// এই কন্ট্রোলারের সব API-এর বেস রুট হবে: api/Likes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        // ডেটাবেসের সাথে যোগাযোগের প্রধান মাধ্যম (Entity Framework Core DbContext)
        private readonly AppDbContext _context;

        // সাময়িক নোটিফিকেশন জমানোর জন্য ইন-মেমোরি লিস্ট (সার্ভার রিস্টার্ট হলে এগুলো মুছে যাবে)
        private static List<Notification> _notifications = new List<Notification>();

        public LikesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// নির্দিষ্ট কোনো পোস্টে লাইক দেওয়ার জন্য এন্ডপয়েন্ট। 
        /// একই ইউজার একটি পোস্টে একাধিকবার লাইক দিতে পারবে না।
        /// </summary>
        /// <param name="postId">যে পোস্টে লাইক দেওয়া হবে তার আইডি</param>
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            // টোকেন থেকে বর্তমান লগইন করা ইউজারের ইউনিক আইডি বের করা হচ্ছে
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // পোস্টটি ডেটাবেসে আছে কি না এবং পোস্টের মালিকের আইডি পাওয়ার জন্য চেক করা হচ্ছে
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            // ভ্যালিডেশন: ইউজার ইতিমধ্যে এই পোস্টে লাইক দিয়েছে কি না তা চেক করা
            var alreadyLiked = await _context.Likes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);

            if (alreadyLiked)
                return BadRequest(new { message = "আপনি ইতিমধ্যে এই Post এ Like দিয়েছেন" });

            // নতুন লাইক অবজেক্ট তৈরি
            var like = new Like
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // ডেটাবেসে লাইক যুক্ত করা এবং সেভ করা
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            // পোস্টের মালিকের জন্য একটি নোটিফিকেশন তৈরি করে ইন-মেমোরি লিস্টে রাখা হচ্ছে
            var notification = new Notification
            {
                Message = $"{userId} আপনার Post এ Like দিয়েছে",
                ToUserId = post.UserId,
                CreatedAt = DateTime.UtcNow
            };
            _notifications.Add(notification);

            return Ok(new { message = "Post এ Like দেওয়া হয়েছে" });
        }

        /// <summary>
        /// পূর্বে দেওয়া লাইক তুলে নেওয়ার (Unlike) এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="postId">যে পোস্টের লাইক রিমুভ করা হবে তার আইডি</param>
        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> UnlikePost(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ইউজারের দেওয়া আগের লাইকটি ডেটাবেস থেকে খুঁজে বের করা
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null)
                return NotFound(new { message = "আপনি এই Post এ Like দেননি" });

            // লাইক রিমুভ করে ডেটাবেস আপডেট করা
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Unlike করা হয়েছে" });
        }

        /// <summary>
        /// নির্দিষ্ট কোনো পোস্টে কারা কারা লাইক দিয়েছে, তাদের ইউজারনেমসহ তালিকা দেখানোর এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="postId">পোস্টের আইডি</param>
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostLikes(Guid postId)
        {
            // ওই পোস্টের সব লাইক থেকে ইউজারদের আইডিগুলো আলাদা করা হচ্ছে
            var userIds = await _context.Likes
                .Where(l => l.PostId == postId)
                .Select(l => l.UserId)
                .ToListAsync();

            // প্রাপ্ত ইউজার আইডিগুলোর বিপরীতে ইউজারদের নাম ও আইডি ডেটাবেস থেকে তুলে আনা হচ্ছে
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// বর্তমান লগইন করা ইউজার সবচেয়ে বেশি কার পোস্টে লাইক দিয়েছে, তা বের করার এন্ডপয়েন্ট।
        /// </summary>
        [Authorize]
        [HttpGet("most-liked-user")]
        public async Task<IActionResult> GetMostLikedUser()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ইউজারের দেওয়া লাইকগুলোর সাথে পোস্ট টেবিল জয়েন করে পোস্টের মালিককে (Owner) গ্রুপ ও কাউন্ট করা হচ্ছে
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

        /// <summary>
        /// সিঙ্গেল ক্লিকে লাইক টগল করার এন্ডপয়েন্ট। 
        /// আগে থেকে লাইক থাকলে তা আনলাইক হয়ে যাবে, আর না থাকলে নতুন লাইক যোগ হবে।
        /// </summary>
        /// <param name="postId">পোস্টের আইডি</param>
        [Authorize]
        [HttpPost("{postId}/toggle")]
        public async Task<IActionResult> ToggleLike(Guid postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // পোস্টের অস্তিত্ব যাচাই করা
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            // ইতিপূর্বে লাইক দেওয়া আছে কি না চেক করা
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                // লাইক করা থাকলে তা রিমুভ (Unlike) করা হচ্ছে
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { liked = false, message = "Unlike করা হয়েছে" });
            }
            else
            {
                // লাইক না থাকলে নতুন লাইক যোগ করা হচ্ছে
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