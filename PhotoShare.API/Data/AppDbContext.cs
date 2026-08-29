using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Models;

namespace PhotoShare.API.Data
{
    // IdentityDbContext ব্যবহার করার মানে হলো, মাইক্রোসফট আইডেন্টিটির সব টেবিল (ইউজার, রোল, টোকেন ইত্যাদি) 
    // অটোমেটিকভাবে তৈরি হবে। আর <AppUser> দিয়ে বোঝানো হচ্ছে যে আমাদের কাস্টমাইজ করা ইউজার ক্লাসটি ব্যবহৃত হবে।
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        // ডাটাবেসের কানেকশন স্ট্রিং এবং অন্যান্য সেটিং রিসিভ করার জন্য এই কনস্ট্রাক্টর
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet হলো ডাটাবেসের এক একটি টেবিল। EF Core এগুলো দেখেই ডাটাবেসে টেবিল তৈরি করবে।
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        // নতুন যোগ হলো — Follow টেবিল
        public DbSet<Follow> Follows { get; set; }

        // OnModelCreating হলো ডাটাবেসের টেবিলগুলোর মধ্যে সম্পর্ক (Relationship) এবং 
        // অন্যান্য নিয়ম-কানুন (Fluent API) সেট করার জায়গা।
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity-এর নিজস্ব টেবিলগুলো ঠিকঠাকমতো তৈরি হওয়ার জন্য এই লাইনটা অবশ্যই ডাকতে হবে। 
            base.OnModelCreating(modelBuilder);

            // ১. Post এবং User এর সম্পর্ক
            modelBuilder.Entity<Post>()
                .HasOne<AppUser>() // একটি Post এর একজন User থাকে
                .WithMany()        // কিন্তু একজন User এর অনেক Post থাকতে পারে
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            // Cascade মানে হলো: ইউজার ডিলিট হয়ে গেলে তার সব Post অটোমেটিকভাবে ডিলিট হয়ে যাবে।

            // ২. Comment এবং Post এর সম্পর্ক
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Post) // একটি Comment একটি নির্দিষ্ট Post এর অধীনে থাকে
                .WithMany(p => p.Comments) // একটি Post এর অনেক Comment থাকতে পারে
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            // Post ডিলিট হলে তার সব Comment ডিলিট হয়ে যাবে।

            // ৩. Like এবং Post এর সম্পর্ক
            modelBuilder.Entity<Like>()
                .HasOne(l => l.Post) // একটি Like একটি নির্দিষ্ট Post এর সাথে যুক্ত
                .WithMany(p => p.Likes) // একটি Post এ অনেক Like থাকতে পারে
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            // Post ডিলিট হলে তার সব Like ডিলিট হয়ে যাবে।

            // ৪. Follow → Follower (AppUser) সম্পর্ক
            // Restrict ব্যবহার করা হচ্ছে যাতে "multiple cascade path" সমস্যা না হয়।
            // (ডাটাবেসে একই টেবিলে একাধিক দিক থেকে Cascade Delete আসলে ডাটাবেস ইঞ্জিন এরর দেয়, তাই Restrict দেওয়া হয়েছে)
            modelBuilder.Entity<Follow>()
                .HasOne<AppUser>() // একজন Follower আসলে একজন AppUser
                .WithMany()
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
            // Restrict মানে: ইউজার ডিলিট করলেও Follow রেকর্ড অটো ডিলিট হবে না (ম্যানুয়ালি করতে হবে)।

            // ৫. Follow → Following (AppUser) সম্পর্ক
            modelBuilder.Entity<Follow>()
                .HasOne<AppUser>() // যাকে Follow করা হচ্ছে, সেও একজন AppUser
                .WithMany()
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}