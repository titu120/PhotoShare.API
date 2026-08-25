namespace PhotoShare.API.Models
{
    public class Post
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string ImageUrl { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}