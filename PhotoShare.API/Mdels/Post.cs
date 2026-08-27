namespace PhotoShare.API.Models
{
    // এই class টা "Post" নামে একটা database table কে represent করে
    // প্রতিটা object মানে একটা Post (ছবি/caption)
    public class Post
    {
        // Post এর নিজস্ব unique ID (primary key)
        public Guid Id { get; private set; }

        // Post এর caption/লেখা অংশ
        public string Caption { get; private set; }

        // Post এর ছবির URL
        public string ImageUrl { get; private set; }

        // এই Post টা কোন User এর — এটাই Foreign Key (IdentityUser এর Id, তাই string)
        public string UserId { get; private set; }

        // Post কবে তৈরি হয়েছে
        public DateTime CreatedAt { get; private set; }

        // Navigation Property — এই Post এ যত Like আছে তার list (EF Core এর জন্য)
        public ICollection<Like> Likes { get; private set; } = new List<Like>();

        // Navigation Property — এই Post এ যত Comment আছে তার list (EF Core এর জন্য)
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        // EF Core এর জন্য খালি constructor (database থেকে data লোড করার সময় ব্যবহার হয়)
        // এটা আমরা নিজে কখনো call করি না
        private Post() { }

        // আসল constructor — বাইরে থেকে সরাসরি "new Post(...)" লেখা যাবে না (private)
        private Post(string caption, string imageUrl, string userId)
        {
            Id = Guid.NewGuid();          // নতুন unique ID বানানো হচ্ছে
            Caption = caption;
            ImageUrl = imageUrl;
            UserId = userId;
            CreatedAt = DateTime.UtcNow;  // এখনকার সময় বসানো হচ্ছে
        }

        // নতুন Post বানানোর একমাত্র সঠিক উপায় (Static Factory Method)
        // ব্যবহার হবে এভাবে: Post.Create("caption", "imageUrl", "userId")
        public static Post Create(string caption, string imageUrl, string userId)
        {
            // ImageUrl খালি হলে Post বানাতে দেওয়া হবে না (validation)
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL খালি হতে পারবে না");

            return new Post(caption, imageUrl, userId);
        }
    }
}