namespace PhotoShare.API.Models
{
    // এটা শুধু in-memory তে রাখার জন্য একটা সহজ class
    // এখনো Database এ save হয় না (পরে PS-এর কোনো ধাপে full notification system বানানো হবে)
    public class Notification
    {
        public string Message { get; set; }
        public string ToUserId { get; set; }   // কাকে notification পাঠানো হচ্ছে
        public DateTime CreatedAt { get; set; }
    }
}