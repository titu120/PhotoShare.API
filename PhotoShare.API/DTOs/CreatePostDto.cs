namespace PhotoShare.API.DTOs
{
    // Client থেকে নতুন Post বানানোর সময় যা পাঠাবে, তার shape
    public class CreatePostDto
    {
        public string Caption { get; set; }
        public string ImageUrl { get; set; }
    }
}