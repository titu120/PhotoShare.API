namespace PhotoShare.API.Models
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string? Bio { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public ICollection<Post> Posts { get; private set; } = new List<Post>();
        public ICollection<Like> Likes { get; private set; } = new List<Like>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        // EF Core এর জন্য empty private constructor লাগে
        private User() { }

        // আসল constructor — private, বাইরে থেকে সরাসরি call করা যাবে না
        private User(string username, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            CreatedAt = DateTime.UtcNow;
        }

        // Static Factory Method — User বানানোর একমাত্র সঠিক উপায়
        public static User Create(string username, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username খালি হতে পারবে না");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email খালি হতে পারবে না");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash খালি হতে পারবে না");

            return new User(username, email, passwordHash);
        }

        public void UpdateProfile(string? bio, string? profilePictureUrl)
        {
            Bio = bio;
            ProfilePictureUrl = profilePictureUrl;
        }
    }
}