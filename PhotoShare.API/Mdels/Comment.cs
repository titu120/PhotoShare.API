namespace PhotoShare.API.Models
{
    // ডাটাবেসের 'Comment' টেবিলকে রিপ্রেজেন্ট করার জন্য এই ক্লাসটি।
    // কোন ইউজার, কোন পোস্টে কী মন্তব্য (comment) করেছে—তার রেকর্ড এখানে থাকবে।
    public class Comment
    {
        // প্রতিটি কমেন্টের জন্য একটি নিজস্ব ইউনিক আইডি (Primary Key)
        public Guid Id { get; set; }

        // কমেন্টের মূল টেক্সট বা লেখা (যেটা ইউজার টাইপ করবে)
        public string Content { get; set; }

        // কোন পোস্টে কমেন্ট করা হয়েছে তার আইডি (Foreign Key)
        public Guid PostId { get; set; }

        // কে কমেন্ট করেছে তার আইডি (Identity-র ডিফল্ট string আইডি)
        public string UserId { get; set; }

        // ঠিক কখন কমেন্ট করা হয়েছে, সেই সময়টা সেভ রাখার জন্য
        public DateTime CreatedAt { get; set; }

        // --- Navigation Property ---
        // Entity Framework Core (EF Core) কে বোঝানোর জন্য যে, 
        // এই 'Comment' অবজেক্টটি ডাটাবেসের একটি নির্দিষ্ট 'Post' এর অংশ।
        // এর মাধ্যমে কোড থেকে খুব সহজেই comment.Post লিখে ওই পোস্টের বিস্তারিত জানা যায়।
        public Post Post { get; set; }
    }
}