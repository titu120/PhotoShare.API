using Microsoft.AspNetCore.Mvc;
using PhotoShare.API.Data;

namespace PhotoShare.API.Controllers
{
    // এই Controller এর সব URL শুরু হবে api/Comments দিয়ে
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        // Database এর সাথে কথা বলার টুল
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }
    }
}