namespace PhotoShare.API.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string ImageUrl { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User User { get; set; }
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}