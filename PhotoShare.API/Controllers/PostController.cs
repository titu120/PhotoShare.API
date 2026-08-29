using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.DTOs;
using PhotoShare.API.Helpers;
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

            return Ok(new { post.Id, post.Caption, post.ImageUrl, post.UserId, post.CreatedAt });
        }

        // URL: GET api/Posts?page=1&pageSize=10
        // কাজ: সব Post দেখানো, Pagination, TimeAgo, LikeCount, IsLikedByCurrentUser সহ
        [HttpGet]
        public async Task<IActionResult> GetAllPosts(int page = 1, int pageSize = 10)
        {
            // বর্তমানে কেউ login করা থাকলে তার ID বের করা (login না থাকলে null হবে)
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = posts.Select(p => new
            {
                p.Id,
                p.Caption,
                p.ImageUrl,
                p.UserId,
                p.CreatedAt,
                TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt),
                LikeCount = p.Likes.Count,
                // এই user login করা থাকলে এবং এই Post এ Like দিয়ে থাকলে true হবে
                IsLikedByCurrentUser = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId)
            });

            return Ok(result);
        }

        // URL: GET api/Posts/most-liked
        // কাজ: সবচেয়ে বেশি Like পাওয়া Post গুলো দেখানো (Top 10)
        // ⚠️ এটা অবশ্যই GetPostById({id}) এর আগে থাকতে হবে
        [HttpGet("most-liked")]
        public async Task<IActionResult> GetMostLikedPosts()
        {
            var posts = await _context.Posts
                .OrderByDescending(p => p.Likes.Count)
                .Take(10)
                .Select(p => new
                {
                    p.Id,
                    p.Caption,
                    p.ImageUrl,
                    p.UserId,
                    p.CreatedAt,
                    LikeCount = p.Likes.Count
                })
                .ToListAsync();

            return Ok(posts);
        }

        // URL: GET api/Posts/search?keyword=xyz
        // কাজ: Caption এর মধ্যে keyword খুঁজে সেই সব Post বের করা
        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts(string keyword)
        {
            var posts = await _context.Posts
                .Where(p => p.Caption.Contains(keyword))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.Id, p.Caption, p.ImageUrl, p.UserId, p.CreatedAt })
                .ToListAsync();

            return Ok(posts);
        }

        // URL: GET api/Posts/{id}
        // কাজ: একটা নির্দিষ্ট Post এর বিস্তারিত তথ্য, Like/Comment সংখ্যা, IsLikedByCurrentUser সহ
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var post = await _context.Posts
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Caption,
                    p.ImageUrl,
                    p.UserId,
                    p.CreatedAt,
                    LikeCount = p.Likes.Count,
                    CommentCount = p.Comments.Count,
                    IsLikedByCurrentUser = currentUserId != null && p.Likes.Any(l => l.UserId == currentUserId)
                })
                .FirstOrDefaultAsync();

            if (post == null)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            return Ok(post);
        }

        // URL: PUT api/Posts/{id}
        // কাজ: Post এর Caption পরিবর্তন করা, শুধু যে বানিয়েছে সে-ই পারবে
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto dto)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != currentUserId)
                return Forbid();

            post.UpdateCaption(dto.Caption);
            await _context.SaveChangesAsync();

            return Ok(new { post.Id, post.Caption, post.ImageUrl, post.UserId, post.CreatedAt });
        }

        // URL: DELETE api/Posts/{id}
        // কাজ: Post মুছে ফেলা, শুধু যে বানিয়েছে সে-ই পারবে
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != currentUserId)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post সফলভাবে মুছে ফেলা হয়েছে" });
        }
    }
}