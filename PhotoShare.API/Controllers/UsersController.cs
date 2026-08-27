using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PhotoShare.API.Data;

namespace PhotoShare.API.Controllers
{
    // এই Controller এর সব URL শুরু হবে api/Users দিয়ে
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // Database এর সাথে কথা বলার টুল
        private readonly AppDbContext _context;

        // Identity দিয়ে User খোঁজা/ম্যানেজ করার টুল
        private readonly UserManager<IdentityUser> _userManager;

        // এই দুইটা টুল Controller কে সরবরাহ করা হচ্ছে (constructor)
        public UsersController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // URL: GET api/Users/{id}
        // কাজ: নির্দিষ্ট একজন user এর profile (Username, Email) দেখানো, Password ছাড়া
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserProfile(string id)
        {
            // দেওয়া id দিয়ে database এ user খোঁজা হচ্ছে
            var user = await _userManager.FindByIdAsync(id);

            // user না পাওয়া গেলে "404 Not Found" পাঠানো হচ্ছে
            if (user == null)
                return NotFound(new { message = "User পাওয়া যায়নি" });

            // user পাওয়া গেলে শুধু Id, Username, Email পাঠানো হচ্ছে (Password বাদ)
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }
    }
}