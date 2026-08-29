namespace PhotoShare.API.Models
{
    public class Post
    {
        public Guid Id { get; private set; }
        public string Caption { get; private set; }
        public string ImageUrl { get; private set; }
        public string UserId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public ICollection<Like> Likes { get; private set; } = new List<Like>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        private Post() { }

        private Post(string caption, string imageUrl, string userId)
        {
            Id = Guid.NewGuid();
            Caption = caption;
            ImageUrl = imageUrl;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;
        }

        public static Post Create(string caption, string imageUrl, string userId)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL খালি হতে পারবে না");

            return new Post(caption, imageUrl, userId);
        }

        // নতুন method — Caption পরিবর্তন করার জন্য
        // Caption সরাসরি বদলানো যায় না (private set), তাই এই method দিয়েই বদলাতে হবে
        public void UpdateCaption(string newCaption)
        {
            Caption = newCaption;
        }
    }
}