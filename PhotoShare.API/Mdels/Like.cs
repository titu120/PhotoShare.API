namespace PhotoShare.API.Models
{
    // ডাটাবেসের 'Like' টেবিলকে রিপ্রেজেন্ট করার জন্য এই ক্লাসটি।
    // কে, কোন পোস্টে, কখন লাইক দিয়েছে—মূলত সেই রেকর্ড এখানে থাকবে।
    public class Like
    {
        // প্রতিটি লাইকের জন্য একটি নিজস্ব ইউনিক আইডি (Primary Key)
        public Guid Id { get; set; }

        // কোন পোস্টে লাইক দেওয়া হয়েছে তার আইডি (Foreign Key)
        public Guid PostId { get; set; }

        // কে লাইক দিয়েছে তার আইডি (AppUser বা Identity-র ডিফল্ট আইডি string হয়, তাই এটাও string রাখা হয়েছে)
        public string UserId { get; set; }

        // ঠিক কখন লাইক দেওয়া হয়েছে সেই সময়টা সেভ রাখার জন্য
        public DateTime CreatedAt { get; set; }

        // --- Navigation Property ---
        // Entity Framework Core (EF Core) কে বোঝানোর জন্য যে, 
        // এই 'Like' অবজেক্টটি ডাটাবেসের একটি নির্দিষ্ট 'Post' এর সাথে যুক্ত। 
        // এর মাধ্যমে কোড থেকে খুব সহজেই like.Post.Caption বা পোস্টের অন্য ডিটেইলস বের করা যায়।
        public Post Post { get; set; }
    }
}