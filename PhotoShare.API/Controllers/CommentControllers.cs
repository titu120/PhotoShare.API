using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;
using PhotoShare.API.DTOs;
using PhotoShare.API.Models;
using System.Security.Claims;

namespace PhotoShare.API.Controllers
{
    /// <summary>
    /// কমেন্ট তৈরি, পোস্টের কমেন্ট লিস্ট দেখা, নির্দিষ্ট কমেন্ট ডিলিট এবং ইউজারের কমেন্ট হিস্ট্রি সংক্রান্ত সকল লজিক হ্যান্ডেল করার কন্ট্রোলার।
    /// এই কন্ট্রোলারের সব API-এর বেস রুট হবে: api/Comments
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        // ডেটাবেসের সাথে যোগাযোগের প্রধান মাধ্যম (Entity Framework Core DbContext)
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// নির্দিষ্ট কোনো পোস্টে নতুন কমেন্ট যোগ করার এন্ডপয়েন্ট। 
        /// কমেন্ট খালি বা ৫০০ ক্যারেক্টারের বেশি হতে পারবে না এবং এর জন্য লগইন থাকা বাধ্যতামূলক।
        /// </summary>
        /// <param name="postId">যে পোস্টে কমেন্ট করা হবে তার আইডি</param>
        /// <param name="dto">কমেন্টের কন্টেন্ট ধারণকারী অবজেক্ট</param>
        [Authorize]
        [HttpPost("{postId}")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentDto dto)
        {
            // ভ্যালিডেশন ১: কমেন্ট খালি বা হোয়াইটস্পেস হতে পারবে না
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest(new { message = "Comment খালি হতে পারবে না" });

            // ভ্যালিডেশন ২: কমেন্ট ৫০০ ক্যারেক্টারের বেশি হতে পারবে না
            if (dto.Content.Length > 500)
                return BadRequest(new { message = "Comment ৫০০ character এর বেশি হতে পারবে না" });

            // টোকেন থেকে বর্তমান লগইন করা ইউজারের ইউনিক আইডি বের করা হচ্ছে
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // পোস্টের অস্তিত্ব যাচাই করা
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists)
                return NotFound(new { message = "Post পাওয়া যায়নি" });

            // নতুন কমেন্ট অবজেক্ট তৈরি
            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                Content = dto.Content,
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // ডেটাবেসে কমেন্ট যুক্ত করা এবং সেভ করা
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                comment.Id,
                comment.Content,
                comment.PostId,
                comment.UserId,
                comment.CreatedAt
            });
        }

        /// <summary>
        /// নির্দিষ্ট কোনো পোস্টের সকল কমেন্ট, কমেন্টকারীর ইউজারনেম এবং প্রোফাইল পিকচারসহ দেখার এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="postId">পোস্টের আইডি</param>
        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostComments(Guid postId)
        {
            // কমেন্ট টেবিলের সাথে ইউজার টেবিল জয়েন করে প্রয়োজনীয় তথ্য তুলে আনা হচ্ছে
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .Join(_context.Users,
                      comment => comment.UserId,
                      user => user.Id,
                      (comment, user) => new
                      {
                          comment.Id,
                          comment.Content,
                          comment.CreatedAt,
                          UserId = user.Id,
                          Username = user.UserName,
                          ProfilePictureUrl = user.ProfilePictureUrl
                      })
                .ToListAsync();

            return Ok(comments);
        }

        /// <summary>
        /// নির্দিষ্ট কোনো কমেন্ট মুছে ফেলার এন্ডপয়েন্ট। 
        /// শুধুমাত্র যে কমেন্টটি লিখেছেন, তিনিই এটি ডিলিট করতে পারবেন।
        /// </summary>
        /// <param name="id">কমেন্টের আইডি</param>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
                return NotFound(new { message = "Comment পাওয়া যায়নি" });

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // ভ্যালিডেশন: শুধুমাত্র কমেন্টকারী নিজে ছাড়া অন্য কেউ এটি ডিলিট করতে পারবে না
            if (comment.UserId != currentUserId)
                return Forbid();

            // ডেটাবেস থেকে কমেন্ট রিমুভ করা এবং সেভ করা
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Comment সফলভাবে মুছে ফেলা হয়েছে" });
        }

        /// <summary>
        /// নির্দিষ্ট একজন ইউজারের করা সমস্ত কমেন্টের হিস্ট্রি বা তালিকা নতুন থেকে পুরানো ক্রমানুসারে দেখার এন্ডপয়েন্ট।
        /// </summary>
        /// <param name="userId">ইউজারের আইডি</param>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserCommentHistory(string userId)
        {
            var comments = await _context.Comments
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)   // লেটেস্ট কমেন্টগুলো আগে দেখানোর জন্য
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.PostId,
                    c.CreatedAt
                })
                .ToListAsync();

            return Ok(comments);
        }
    }
}