namespace PhotoShare.API.DTOs
{
    /// <summary>
    /// ক্লায়েন্ট (ফ্রন্টএন্ড) থেকে নতুন কমেন্ট দেওয়ার সময় যে ডেটা পাঠানো হবে, তার কাঠামো বা DTO (Data Transfer Object)।
    /// </summary>
    public class CreateCommentDto
    {
        // কমেন্টের মূল টেক্সট বা কন্টেন্ট
        public string Content { get; set; }
    }
}