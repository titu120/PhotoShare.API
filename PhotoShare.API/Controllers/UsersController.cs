using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    // [Route] নির্দেশ করে এই Controller এর লিংক কেমন হবে। 
    // "api/[controller]" মানে হলো লিংকটি হবে: api/Users
    [Route("api/[controller]")]
    [ApiController] // এটি বোঝায় যে এটি একটি API Controller, যা অটোমেটিক কিছু সুবিধা দেয় (যেমন- Model State Validation)
    public class UsersController : ControllerBase
    {
        // ডাটাবেসের সাথে সরাসরি কথা বলার জন্য EF Core এর টুল
        private readonly AppDbContext _context;

        // ASP.NET Identity এর একটি স্পেশাল টুল, যা ইউজার খোঁজা বা আপডেট করার কাজকে সহজ করে দেয়
        private readonly UserManager<AppUser> _userManager;

        // Dependency Injection (DI) এর মাধ্যমে এই টুলগুলো Controller এ আনা হচ্ছে
        public UsersController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // URL: GET api/Users/{id}
        // কাজ: user profile দেখানো, সাথে FollowersCount, FollowingCount, IsFollowedByCurrentUser
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // বর্তমানে কেউ login করা থাকলে তার ID বের করা (না থাকলে null)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.Bio,
                user.ProfilePictureUrl,

                // এই user কে যতজন Follow করছে
                FollowersCount = await _context.Follows.CountAsync(f => f.FollowingId == id),

                // এই user কতজনকে Follow করছে
                FollowingCount = await _context.Follows.CountAsync(f => f.FollowerId == id),

                // বর্তমান logged-in user কি এই profile এর মালিককে Follow করছে
                IsFollowedByCurrentUser = currentUserId != null &&
                    await _context.Follows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id)
            });
        }

        // URL: PUT api/Users/me
        // কাজ: শুধু নিজের Bio/ProfilePicture আপডেট করা
        [Authorize] // [Authorize] মানে হলো টোকেন ছাড়া (লগইন ছাড়া) কেউ এই লিংকে ঢুকতে পারবে না।
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            // ম্যাজিক লাইন! যে ইউজার লগইন করে রিকোয়েস্ট পাঠিয়েছে, তার টোকেন থেকে তার ID টা নিরাপদে বের করে আনা হচ্ছে।
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // ক্লায়েন্টের পাঠানো নতুন ডাটাগুলো সেট করা হচ্ছে
            user.Bio = request.Bio;
            user.ProfilePictureUrl = request.ProfilePictureUrl;

            // ডাটাবেসে সেভ করা হচ্ছে
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Bio,
                user.ProfilePictureUrl
            });
        }

        // URL: GET api/Users/{id}/followers
        // কাজ: এই ইউজারকে যারা যারা ফলো করছে (ফলোয়ার), তাদের লিস্ট দেখানো
        [HttpGet("{id}/followers")]
        public async Task<IActionResult> GetFollowers(string id)
        {
            // ধাপ ১: Follow টেবিল থেকে বের করা হলো কে কে এই 'id' কে ফলো করছে (শুধু তাদের আইডিগুলো নেওয়া হলো)
            var followers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.FollowerId)
                .ToListAsync();

            // ধাপ ২: যাদের আইডি পাওয়া গেলো, Users টেবিল থেকে তাদের নাম ও ইমেইল বের করে আনা হলো
            var followerUsers = await _context.Users
                .Where(u => followers.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            return Ok(followerUsers);
        }

        // URL: GET api/Users/{id}/following
        // কাজ: এই ইউজার নিজে কাকে কাকে ফলো করছে (ফলোয়িং), তাদের লিস্ট দেখানো
        [HttpGet("{id}/following")]
        public async Task<IActionResult> GetFollowing(string id)
        {
            // লজিকটা ফলোয়ারের মতোই, শুধু Where কন্ডিশনটা উল্টে গেছে (FollowerId == id)
            var following = await _context.Follows
                .Where(f => f.FollowerId == id)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var followingUsers = await _context.Users
                .Where(u => following.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            return Ok(followingUsers);
        }

        // URL: GET api/Users/{id}/posts
        // কাজ: একজন নির্দিষ্ট ইউজারের সব Post দেখানো
        [HttpGet("{id}/posts")]
        public async Task<IActionResult> GetUserPosts(string id)
        {
            // LINQ Where দিয়ে ফিল্টার করা হচ্ছে — শুধু এই UserId এর Post গুলো আসবে।
            // OrderByDescending দিয়ে নতুন পোস্টগুলো আগে (উপরে) দেখানো হচ্ছে।
            var posts = await _context.Posts
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Caption,
                    p.ImageUrl,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(posts);
        }
    }

    // এটাকে বলা হয় DTO (Data Transfer Object)। 
    // ইউজার প্রোফাইল আপডেটের সময় ক্লায়েন্ট (যেমন React বা Android অ্যাপ) ঠিক কী কী ডাটা পাঠাবে, 
    // তার একটা নির্দিষ্ট ছাঁচ বা কাঠামো হলো এই ক্লাসটি।
    public class UpdateProfileRequest
    {
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}