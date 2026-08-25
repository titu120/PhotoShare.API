namespace PhotoShare.API.Models
{
    public class Post
    {
        public Guid Id { get; private set; }
        public string Caption { get; private set; }
        public string ImageUrl { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User User { get; private set; }
        public ICollection<Like> Likes { get; private set; } = new List<Like>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        private Post() { }

        private Post(string caption, string imageUrl, Guid userId)
        {
            Id = Guid.NewGuid();
            Caption = caption;
            ImageUrl = imageUrl;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public static Post Create(string caption, string imageUrl, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL খালি হতে পারবে না");

            return new Post(caption, imageUrl, userId);
        }
    }
}