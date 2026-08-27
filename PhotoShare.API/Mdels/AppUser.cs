using Microsoft.AspNetCore.Identity;

namespace PhotoShare.API.Models
{
    // IdentityUser এর সব built-in feature (Username, Email, PasswordHash) থাকবে
    // সাথে আমাদের প্রজেক্টের জন্য দরকারি Bio, ProfilePictureUrl যোগ করা হলো
    // এটাকে বলে "Extend" করা — পুরনো class এর সব রেখে নতুন কিছু যোগ করা
    public class AppUser : IdentityUser
    {
        // ব্যবহারকারীর নিজের সম্পর্কে ছোট বর্ণনা (optional, তাই ? দেওয়া)
        public string? Bio { get; set; }

        // Profile picture এর URL (optional)
        public string? ProfilePictureUrl { get; set; }
    }
}