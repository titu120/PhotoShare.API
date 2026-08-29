namespace PhotoShare.API.Helpers
{
    /// <summary>
    /// যেকোনো সময় বা তারিখকে (DateTime) মানুষের পড়ার উপযোগী বাংলায় "কত আগে" (যেমন: ৫ মিনিট আগে, ২ দিন আগে) রূপান্তর করার হেল্পার ক্লাস।
    /// </summary>
    public static class TimeAgoHelper
    {
        /// <summary>
        /// একটি নির্দিষ্ট সময় বর্তমান সময়ের তুলনায় কতক্ষণ আগে ঘটেছে তা হিসাব করে বাংলায় স্ট্রিং রিটার্ন করে।
        /// এটি একটি স্ট্যাটিক মেথড, তাই অবজেক্ট না বানিয়ে সরাসরি ব্যবহার করা যায় (যেমন: TimeAgoHelper.GetTimeAgo(post.CreatedAt))
        /// </summary>
        /// <param name="createdAt">পোস্ট বা কমেন্ট তৈরির সময়</param>
        /// <returns>সময় ব্যবধানের বাংলা রূপ (যেমন: "১০ মিনিট আগে")</returns>
        public static string GetTimeAgo(DateTime createdAt)
        {
            // বর্তমান সময় (UTC) এবং পোস্ট তৈরির সময়ের মধ্যকার ব্যবধান বা পার্থক্য বের করা হচ্ছে
            var span = DateTime.UtcNow - createdAt;

            // ১ মিনিটের কম সময় হলে
            if (span.TotalMinutes < 1)
                return "এইমাত্র";

            // ১ ঘণ্টার কম সময় হলে (মিনিটে হিসাব)
            if (span.TotalMinutes < 60)
                return $"{(int)span.TotalMinutes} মিনিট আগে";

            // ২৪ ঘণ্টার কম সময় হলে (ঘণ্টায় হিসাব)
            if (span.TotalHours < 24)
                return $"{(int)span.TotalHours} ঘণ্টা আগে";

            // ৩০ দিনের কম সময় হলে (দিনে হিসাব)
            if (span.TotalDays < 30)
                return $"{(int)span.TotalDays} দিন আগে";

            // ৩৬৫ দিনের কম সময় হলে (মাসে হিসাব, গড় ৩০ দিনে এক মাস ধরে)
            if (span.TotalDays < 365)
                return $"{(int)(span.TotalDays / 30)} মাস আগে";

            // এক বছর বা তার বেশি সময় হলে (বছরে হিসাব)
            return $"{(int)(span.TotalDays / 365)} বছর আগে";
        }
    }
}