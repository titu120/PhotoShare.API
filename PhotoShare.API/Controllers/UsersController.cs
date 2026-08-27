using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        // Identity দিয়ে User খোঁজা/ম্যানেজ করার টুল (এখন AppUser টাইপে)
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
            // দেওয়া id দিয়ে database এ user খোঁজা হচ্ছে
            var user = await _userManager.FindByIdAsync(id);

            // user না পাওয়া গেলে "404 Not Found" পাঠানো হচ্ছে
            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // user পাওয়া গেলে তার তথ্য পাঠানো হচ্ছে (Password বাদে)
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
        // কাজ: শুধু নিজের Bio/ProfilePicture আপডেট করা
        // [Authorize] মানে: valid login token ছাড়া এটা ব্যবহার করা যাবে না
        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            // Login token থেকে "এখন কে logged-in আছে" তার ID বের করা হচ্ছে
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // সেই ID দিয়ে user খোঁজা হচ্ছে
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // নতুন data বসিয়ে দেওয়া হচ্ছে
            user.Bio = request.Bio;
            user.ProfilePictureUrl = request.ProfilePictureUrl;

            // Database এ পরিবর্তন সংরক্ষণ করা হচ্ছে
            await _userManager.UpdateAsync(user);

            // আপডেট হওয়া তথ্য ফেরত পাঠানো হচ্ছে
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Bio,
                user.ProfilePictureUrl
            });
        }
    }

    // PUT request এ client যে data (Bio, ProfilePictureUrl) পাঠাবে তার shape/গঠন
    public class UpdateProfileRequest
    {
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}