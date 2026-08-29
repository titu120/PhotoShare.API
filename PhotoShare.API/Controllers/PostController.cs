using Microsoft.AspNetCore.Mvc;
using PhotoShare.API.Data;

namespace PhotoShare.API.Controllers
{
    // এই Controller এর সব URL শুরু হবে api/Posts দিয়ে
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        // Database এর সাথে কথা বলার টুল
        private readonly AppDbContext _context;

        // এই টুল Controller কে সরবরাহ করা হচ্ছে (constructor)
        public PostsController(AppDbContext context)
        {
            _context = context;
        }
    }
}