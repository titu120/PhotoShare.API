using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Models;

namespace PhotoShare.API.Data
{
    // আগে ছিল IdentityDbContext<IdentityUser>
    // এখন IdentityDbContext<AppUser> — কারণ আমাদের নিজের বানানো AppUser (Bio সহ) ব্যবহার হবে
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Database এর প্রতিটা টেবিলের জন্য একটা করে DbSet
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // Entity গুলোর মধ্যে সম্পর্ক (relationship) ঠিক করা হচ্ছে এখানে
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Post → AppUser সম্পর্ক (আগে IdentityUser ছিল, এখন AppUser)
            // একটা Post এর একজন owner (User) থাকে
            modelBuilder.Entity<Post>()
                .HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment → Post সম্পর্ক
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Like → Post সম্পর্ক
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}