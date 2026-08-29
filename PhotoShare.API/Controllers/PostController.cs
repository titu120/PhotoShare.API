using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var post = Post.Create(dto.Caption, dto.ImageUrl, userId);

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

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
        // কাজ: সব Post এর list দেখানো, সবচেয়ে নতুন Post সবার উপরে (OrderByDescending)
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
    }
}