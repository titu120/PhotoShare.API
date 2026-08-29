namespace PhotoShare.API.DTOs
{
    /// <summary>
    /// ক্লায়েন্ট (ফ্রন্টএন্ড) থেকে নতুন পোস্ট তৈরির সময় যে ডেটাগুলো পাঠানো হবে, তার কাঠামো বা DTO (Data Transfer Object)।
    /// </summary>
    public class CreatePostDto
    {
        // পোস্টের ক্যাপশন বা লেখা
        public string Caption { get; set; }

        // আপলোড করা ছবির লিংক বা URL
        public string ImageUrl { get; set; }
    }
}