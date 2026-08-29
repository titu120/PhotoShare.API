using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace PhotoShare.API.Models
{
    // ডাটাবেসের 'Post' টেবিলকে রিপ্রেজেন্ট করার জন্য এই ক্লাসটি তৈরি করা হয়েছে (Entity Class)।
    public class Post
    {
        // `private set` মানে হলো বাইরের কোনো ক্লাস (যেমন Controller) সরাসরি এগুলোর মান পরিবর্তন করতে পারবে না। 
        // মান পরিবর্তন করতে হলে এই ক্লাসের ভেতরের নির্দিষ্ট মেথড ব্যবহার করতে হবে। এতে ডাটা সুরক্ষিত থাকে।
        public Guid Id { get; private set; }
        public string Caption { get; private set; }
        public string ImageUrl { get; private set; } // আপলোড করা ছবির লিংক বা পাথ
        public string UserId { get; private set; } // কোন ইউজার পোস্টটি তৈরি করেছে তার আইডি
        public DateTime CreatedAt { get; private set; } // পোস্টটি কখন তৈরি হয়েছে তার সময়

        // --- Navigation Properties ---
        // ডাটাবেসের রিলেশনশিপ বোঝানোর জন্য। একটি পোস্টের অধীনে অনেকগুলো লাইক ও কমেন্ট থাকতে পারে।
        // কোড যেন ক্র্যাশ (Null Reference Exception) না করে, তাই শুরুতেই একটি ফাঁকা লিস্ট (new List) সেট করে দেওয়া হয়েছে।
        public ICollection<Like> Likes { get; private set; } = new List<Like>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        /* এর কাজ হলো Entity Framework Core (EF Core) এর জন্য। EF Core যখন ডাটাবেস থেকে ডাটা পড়ে এনে 
           তাকে অবজেক্টে রূপান্তর করে, তখন তার এমন একটা ফাঁকা কনস্ট্রাক্টর লাগে। 
           যেহেতু এটা private, তাই আমরা নিজেরা কোড লিখে এটা কল করতে পারব না, শুধু ডাটাবেস (EF Core) এটা ব্যবহার করতে পারবে। */
        private Post() { }

        // এই কনস্ট্রাক্টরটিও private রাখা হয়েছে। অর্থাৎ, অন্য জায়গা থেকে 'new Post()' লিখে পোস্ট বানানো যাবে না।
        // এটি শুধুমাত্র নিচের Create() মেথডের ভেতর থেকেই কল করা সম্ভব।
        private Post(string caption, string imageUrl, string userId)
        {
            Id = Guid.NewGuid(); // ডাটাবেসে সেভ হওয়ার আগেই একটি নতুন ইউনিক আইডি (Guid) তৈরি করা হচ্ছে
            Caption = caption;
            ImageUrl = imageUrl;
            UserId = userId;
            CreatedAt = DateTime.UtcNow; // বর্তমান সময় (UTC টাইমজোনে) সেট করা হচ্ছে
        }

        // নতুন পোস্ট তৈরি করার একমাত্র রাস্তা হলো এই Static Factory Method টি।
        // এটি ব্যবহার করার সুবিধা হলো, অবজেক্ট তৈরির আগেই আমরা প্রয়োজনীয় ডাটা চেক (validation) করে নিতে পারি।
        public static Post Create(string caption, string imageUrl, string userId)
        {
            // ভ্যালিডেশন: ছবি ছাড়া কোনো পোস্ট তৈরি করা যাবে না। যদি ছবির URL খালি থাকে, তবে এরর দেওয়া হবে।
            if (string.IsNullOrWhiteSpace(imageUrl))
                throw new ArgumentException("Image URL খালি হতে পারবে না");

            // সব ডাটা ঠিক থাকলে, উপরের প্রাইভেট কনস্ট্রাক্টরকে কল করে নতুন পোস্টের অবজেক্ট রিটার্ন করা হচ্ছে
            return new Post(caption, imageUrl, userId);
        }

        // নতুন method — Caption পরিবর্তন বা এডিট করার জন্য।
        // যেহেতু Caption প্রপার্টিতে 'private set' দেওয়া আছে, তাই সরাসরি বদলানো যায় না। 
        // এই মেথড দিয়ে বদলালে আমাদের সম্পূর্ণ কন্ট্রোল থাকে (চাইলে এখানেও ভ্যালিডেশন বসানো যায়)।
        public void UpdateCaption(string newCaption)
        {
            Caption = newCaption;
        }
    }
}