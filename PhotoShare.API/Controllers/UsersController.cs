using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Database এর সাথে কথা বলার টুল
        private readonly AppDbContext _context;

        // Identity দিয়ে User খোঁজা/ম্যানেজ করার টুল
        private readonly UserManager<AppUser> _userManager;

        // এই দুইটা টুল Controller কে সরবরাহ করা হচ্ছে (constructor)
        public UsersController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // URL: GET api/Users/{id}
        // কাজ: নির্দিষ্ট একজন user এর profile দেখানো (password ছাড়া)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.Bio,
                user.ProfilePictureUrl
            });
        }

        // URL: PUT api/Users/me
        // কাজ: শুধু নিজের Bio/ProfilePicture আপডেট করা, [Authorize] দিয়ে সুরক্ষিত
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            user.Bio = request.Bio;
            user.ProfilePictureUrl = request.ProfilePictureUrl;

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
        // কাজ: এই user কে যারা যারা follow করছে, তাদের list দেখানো
        [HttpGet("{id}/followers")]
        public async Task<IActionResult> GetFollowers(string id)
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == id)
                .Select(f => f.FollowerId)
                .ToListAsync();

            var followerUsers = await _context.Users
                .Where(u => followers.Contains(u.Id))
                .Select(u => new { u.Id, u.UserName, u.Email })
                .ToListAsync();

            return Ok(followerUsers);
        }

        // URL: GET api/Users/{id}/following
        // কাজ: এই user কাকে কাকে follow করছে, তাদের list দেখানো
        [HttpGet("{id}/following")]
        public async Task<IActionResult> GetFollowing(string id)
        {
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
    }

    // PUT request এ client যে data (Bio, ProfilePictureUrl) পাঠাবে তার shape
    public class UpdateProfileRequest
    {
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}