namespace PhotoShare.API.DTOs
{
    // Client থেকে নতুন Comment দেওয়ার সময় যা পাঠাবে
    public class CreateCommentDto
    {
        public string Content { get; set; }
    }
}