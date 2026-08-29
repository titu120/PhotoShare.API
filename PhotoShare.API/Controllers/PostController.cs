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
            // Token থেকে বর্তমান logged-in user এর ID বের করা হচ্ছে
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // নতুন Post object বানানো হচ্ছে
            var post = Post.Create(dto.Caption, dto.ImageUrl, userId);

            // Database এ যোগ করা হচ্ছে
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { post.Id, post.Caption, post.ImageUrl, post.UserId, post.CreatedAt });
        }

        // URL: GET api/Posts?page=1&pageSize=10
        // কাজ: সব Post দেখানো, নতুন থেকে পুরাতন, পাতায় পাতায় (Pagination), সাথে "কত আগে" লেখা
        [HttpGet]
        public async Task<IActionResult> GetAllPosts(int page = 1, int pageSize = 10)
        {
            // Database থেকে এই পাতার জন্য দরকারি Post গুলো আনা হচ্ছে
            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)   // নতুন থেকে পুরাতন সাজানো
                .Skip((page - 1) * pageSize)           // আগের পাতাগুলোর Post বাদ
                .Take(pageSize)                         // এই পাতার জন্য নির্দিষ্ট সংখ্যক Post
                .ToListAsync();

            // প্রতিটা Post এর সাথে "TimeAgo" (কত আগে) যোগ করে নতুন shape এ পাঠানো হচ্ছে
            var result = posts.Select(p => new
            {
                p.Id,
                p.Caption,
                p.ImageUrl,
                p.UserId,
                p.CreatedAt,
                TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt)
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
                .OrderByDescending(p => p.Likes.Count)   // Like সংখ্যা অনুযায়ী বেশি থেকে কম
                .Take(10)                                 // শুধু প্রথম ১০টা
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
        // ⚠️ এটাও GetPostById({id}) এর আগে থাকতে হবে
        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts(string keyword)
        {
            var posts = await _context.Posts
                .Where(p => p.Caption.Contains(keyword))   // Caption এ keyword আছে কিনা
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new { p.Id, p.Caption, p.ImageUrl, p.UserId, p.CreatedAt })
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
                    LikeCount = p.Likes.Count,
                    CommentCount = p.Comments.Count
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

            // Validation: শুধু নিজের Post-ই edit করা যাবে
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

            // Validation: শুধু নিজের Post-ই delete করা যাবে
            if (post.UserId != currentUserId)
                return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post সফলভাবে মুছে ফেলা হয়েছে" });
        }
    }
}