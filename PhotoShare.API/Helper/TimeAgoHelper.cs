namespace PhotoShare.API.Helpers
{
    // এই class টা শুধু একটা কাজ করে — কোনো সময়কে "কত আগে" এভাবে বাংলায় রূপান্তর করা
    public static class TimeAgoHelper
    {
        // static method — Class এর object না বানিয়েই সরাসরি call করা যায়
        // যেমন: TimeAgoHelper.GetTimeAgo(post.CreatedAt)
        public static string GetTimeAgo(DateTime createdAt)
        {
            // এখনকার সময় আর CreatedAt এর মধ্যে পার্থক্য বের করা হচ্ছে
            var span = DateTime.UtcNow - createdAt;

            if (span.TotalMinutes < 1)
                return "এইমাত্র";

            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} মিনিট আগে";

            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} ঘণ্টা আগে";

            if (span.TotalDays < 30)
                return $"{(int)span.TotalDays} দিন আগে";

            if (span.TotalDays < 365)
                return $"{(int)(span.TotalDays / 30)} মাস আগে";

            return $"{(int)(span.TotalDays / 365)} বছর আগে";
        }
    }
}