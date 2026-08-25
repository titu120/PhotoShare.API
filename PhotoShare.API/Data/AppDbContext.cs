using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Models;

namespace PhotoShare.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User → Post : One-to-Many relationship
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)          // একটা Post এর একটাই User থাকে
                .WithMany(u => u.Posts)        // একটা User এর অনেকগুলো Post থাকতে পারে
                .HasForeignKey(p => p.UserId)  // Foreign Key হলো Post.UserId
                .OnDelete(DeleteBehavior.Cascade); // User delete হলে তার সব Post ও delete হবে
        }
    }
}