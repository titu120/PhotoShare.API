using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotoShare.API.Data;
using PhotoShare.API.DTOs;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostsController(AppDbContext context)
        {
            _context = context;
        }

        // URL: POST api/Posts
        // কাজ: নতুন Post তৈরি করা, শুধু login করা user-ই পারবে
        // Post টা automatic সেই user এর সাথে link হয়ে যাবে (কে বানালো, তার ID বসে যাবে)
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
    }
}