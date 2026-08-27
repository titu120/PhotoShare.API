using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhotoShare.API.Data;
using System.Text;

// অ্যাপ তৈরি করার জন্য builder বানানো হচ্ছে (এটাই শুরুর পয়েন্ট)
var builder = WebApplication.CreateBuilder(args);

// Controller ব্যবহার করার সুবিধা যোগ করা হচ্ছে (যেমন UsersController)
builder.Services.AddControllers();

// Database এর সাথে connect করার জন্য AppDbContext কে DI (Dependency Injection) এ register করা হচ্ছে
// appsettings.json থেকে connection string নেওয়া হচ্ছে
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (Register/Login সিস্টেম) চালু করা হচ্ছে
// IdentityUser ব্যবহার হচ্ছে user হিসেবে, আর Database হিসেবে AppDbContext ব্যবহার হচ্ছে
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// API endpoint গুলো explore/দেখার জন্য সুবিধা যোগ করা
builder.Services.AddEndpointsApiExplorer();

// Swagger UI তৈরি করার সুবিধা যোগ করা (testing এর জন্য)
builder.Services.AddSwaggerGen();

// এতক্ষণ যা যা "যোগ" করা হলো, তা দিয়ে আসল app তৈরি হচ্ছে
var app = builder.Build();

// শুধু Development mode এ (নিজের কম্পিউটারে কাজ করার সময়) Swagger UI চালু হবে
// Production/live সার্ভারে এটা বন্ধ থাকবে (security এর জন্য)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP request আসলে তা automatic HTTPS এ পাঠিয়ে দেওয়া হবে (নিরাপত্তার জন্য)
app.UseHttpsRedirection();

// কে "লগইন করা আছে" সেটা যাচাই করার middleware (Authentication)
app.UseAuthentication();

// লগইন করা user এর "কী করার অনুমতি আছে" সেটা যাচাই করার middleware (Authorization)
app.UseAuthorization();

// আমাদের নিজের বানানো Controller গুলো (UsersController ইত্যাদি) এর route চালু করা হচ্ছে
app.MapControllers();

// Identity এর নিজস্ব endpoint গুলো (register, login, refresh, forgotPassword...) চালু করা হচ্ছে
// এই একটা লাইনেই সব endpoint automatic তৈরি হয়ে যায়
app.MapIdentityApi<IdentityUser>();

// অ্যাপ চালু করে দেওয়া হচ্ছে, এখন থেকে server request গ্রহণ করতে প্রস্তুত
app.Run();