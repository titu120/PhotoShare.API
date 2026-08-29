using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// কন্ট্রোলার সার্ভিসগুলো অ্যাপ্লিকেশনে যোগ করা হচ্ছে
builder.Services.AddControllers();

// ডেটাবেস কানেকশন কনফিগার করা (SQL Server ব্যবহার করে AppDbContext রেজিস্টার করা)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity এবং Identity API Endpoints কনফিগার করা (ইউজার ম্যানেজমেন্ট ও অথেন্টিকেশনের জন্য)
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// CORS (Cross-Origin Resource Sharing) পলিসি তৈরি করা হচ্ছে
// এটি ফ্রন্টএন্ড (যেমন React অ্যাপ) থেকে আসা রিকোয়েস্টগুলোকে ব্যাকএন্ডে হিট করার অনুমতি দেয়
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",   // Create React App-এর ডিফল্ট পোর্ট
                "http://localhost:5173"    // Vite (আধুনিক React বিল্ড টুল)-এর ডিফল্ট পোর্ট
              )
              .AllowAnyHeader()            // যেকোনো ধরনের HTTP Header এলাও করা
              .AllowAnyMethod()            // যেকোনো ধরনের HTTP Method (GET, POST, PUT, DELETE) এলাও করা
              .AllowCredentials();         // টোকেন বা কুকি (Credentials) পাঠানোর অনুমতি দেওয়া
    });
});

// Swagger এবং API ডকুমেন্টেশনের জন্য সার্ভিস যোগ করা
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ডেভেলপমেন্ট এনভায়রনমেন্টে Swagger UI চালু রাখা
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP রিকোয়েস্টগুলোকে স্বয়ংক্রিয়ভাবে HTTPS-এ রিডাইরেক্ট করা
app.UseHttpsRedirection();

// CORS পলিসি মিডলওয়্যার চালু করা হচ্ছে
// এটি অবশ্যই অথেন্টিকেশন মিডলওয়্যারের আগে বসাতে হবে, যাতে ব্রাউজারের প্রি-ফ্লাইট রিকোয়েস্ট ব্লক না হয়
app.UseCors("AllowReactApp");

// অথেন্টিকেশন (ইউজার কে তা যাচাই করা) মিডলওয়্যার
app.UseAuthentication();

// অথরাইজেশন (ইউজারের নির্দিষ্ট কাজ করার অনুমতি আছে কি না তা দেখা) মিডলওয়্যার
app.UseAuthorization();

// কন্ট্রোলারগুলোর রাউটিং ম্যাপ করা
app.MapControllers();

// Identity-এর বিল্ট-ইন এন্ডপয়েন্টগুলো (যেমন /register, /login ইত্যাদি) ম্যাপ করা
app.MapIdentityApi<AppUser>();

// অ্যাপ্লিকেশনটি রান বা চালু করা
app.Run();