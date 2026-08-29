namespace PhotoShare.API.DTOs
{
    // Feed/Explore এ প্রতিটা Post ঠিক এই shape এ ফেরত পাঠানো হবে
    public class PostFeedDto
    {
        public Guid Id { get; set; }
        public string Caption { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        // Author (যে বানিয়েছে) সম্পর্কিত তথ্য
        public string AuthorId { get; set; }
        public string AuthorUsername { get; set; }
        public string AuthorProfilePictureUrl { get; set; }

        // সংখ্যা সংক্রান্ত তথ্য
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
    }
}