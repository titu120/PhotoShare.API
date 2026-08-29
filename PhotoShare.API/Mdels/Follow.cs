namespace PhotoShare.API.Models
{
    // ডাটাবেসের 'Follow' টেবিলকে রিপ্রেজেন্ট করার জন্য এই ক্লাসটি।
    // এর কাজ হলো ইউজারদের মধ্যকার সম্পর্ক (কে কাকে ফলো করছে) রেকর্ড করে রাখা। 
    // এটাকে ডাটাবেসের ভাষায় Many-to-Many রিলেশনশিপের 'Join Table' বলা হয়।
    public class Follow
    {
        // প্রতিটি ফলো রেকর্ডের জন্য একটি নিজস্ব ইউনিক আইডি (Primary Key)
        public Guid Id { get; set; }

        // যে ইউজার নিজে থেকে ফলো করছে তার আইডি (Foreign Key)।
        // ধরুন আপনি আমাকে ফলো করলেন, তাহলে এখানে আপনার UserId বসবে।
        public string FollowerId { get; set; }

        // যাকে ফলো করা হচ্ছে তার আইডি (Foreign Key)।
        // আপনি যেহেতু আমাকে ফলো করেছেন, তাই এখানে আমার UserId বসবে।
        public string FollowingId { get; set; }

        // ঠিক কোন সময় ফলো করা হয়েছে, সেই রেকর্ড রাখার জন্য
        public DateTime CreatedAt { get; set; }
    }
}