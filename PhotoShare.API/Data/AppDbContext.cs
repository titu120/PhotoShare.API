using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Models;

namespace PhotoShare.API.Data
{
    // এই class টাই Database এর সাথে যোগাযোগের মূল কেন্দ্র
    // IdentityDbContext<IdentityUser> ব্যবহার হচ্ছে কারণ Identity (Register/Login system) ব্যবহার হচ্ছে
    // এটা normal DbContext এর মতোই, কিন্তু সাথে Identity এর টেবিল (AspNetUsers ইত্যাদি) automatic যোগ করে দেয়
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        // Constructor — connection string ইত্যাদি options বাইরে থেকে আসে (Program.cs থেকে)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // প্রতিটা DbSet মানে একটা Database Table
        // DbSet<Post> Posts → Database এ "Posts" নামে টেবিল হবে
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // এই method এ Entity গুলোর মধ্যে সম্পর্ক (Relationship) ঠিক করে দেওয়া হয়
        // Migration বানানোর সময় EF Core এই তথ্য পড়ে বুঝে নেয় কী কী Foreign Key/constraint লাগবে
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Post → IdentityUser সম্পর্ক
            // একটা Post এর একজন User (owner) থাকে, User delete হলে তার Post ও delete হবে (Cascade)
            modelBuilder.Entity<Post>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → Post সম্পর্ক
            // একটা Post এর অনেক Comment থাকতে পারে, Post delete হলে তার সব Comment ও delete হবে
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Like → Post সম্পর্ক
            // একটা Post এর অনেক Like থাকতে পারে, Post delete হলে তার সব Like ও delete হবে
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}