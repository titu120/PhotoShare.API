using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.DTOs;
using PhotoShare.API.Models;
using System.Security.Claims;

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

        // URL: POST api/Posts
        // কাজ: নতুন Post তৈরি করা, শুধু login করা user-ই পারবে
        // Post টা automatic সেই user এর সাথে link হয়ে যাবে
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            // Token থেকে বর্তমানে logged-in user এর ID বের করা হচ্ছে
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // নতুন Post object বানানো হচ্ছে
            var post = Post.Create(dto.Caption, dto.ImageUrl, userId);

            // Database এ যোগ করা হচ্ছে
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // তৈরি হওয়া Post এর তথ্য ফেরত পাঠানো হচ্ছে
            return Ok(new
            {
                post.Id,
                post.Caption,
                post.ImageUrl,
                post.UserId,
                post.CreatedAt
            });
        }

        // URL: GET api/Posts
        // কাজ: সব Post এর list দেখানো, সবচেয়ে নতুন Post সবার উপরে
        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)   // নতুন থেকে পুরাতন সাজানো হচ্ছে
                .Select(p => new
                {
                    p.Id,
                    p.Caption,
                    p.ImageUrl,
                    p.UserId,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(posts);
        }

        // URL: GET api/Posts/{id}
        // কাজ: একটা নির্দিষ্ট Post এর বিস্তারিত তথ্য দেখানো, Like/Comment সংখ্যা সহ
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var post = await _context.Posts
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Caption,
                    p.ImageUrl,
                    p.UserId,
                    p.CreatedAt,
                    LikeCount = p.Likes.Count,       // এই Post এর মোট Like সংখ্যা
                    CommentCount = p.Comments.Count  // এই Post এর মোট Comment সংখ্যা
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            return Ok(post);
        }
    }
}