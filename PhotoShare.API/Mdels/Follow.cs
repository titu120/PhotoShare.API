namespace PhotoShare.API.Models
{
    // এই class দিয়ে বোঝানো হয় কে কাকে Follow করছে
    public class Follow
    {
        public Guid Id { get; set; }

        // যে Follow করছে, তার User Id
        public string FollowerId { get; set; }

        // যাকে Follow করা হচ্ছে, তার User Id
        public string FollowingId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}