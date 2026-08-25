namespace PhotoShare.API.Models
{
    public class Follow
    {
        public Guid Id { get; set; }
        public Guid FollowerId { get; set; }
        public Guid FollowingId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User Follower { get; set; }
        public User Following { get; set; }
    }
}