using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhotoShare.API.Data;
using PhotoShare.API.Models;
using System.Text;

// অ্যাপ তৈরির শুরু
var builder = WebApplication.CreateBuilder(args);

// Controller ব্যবহার করার সুবিধা যোগ করা হচ্ছে
builder.Services.AddControllers();

// Database এর সাথে connect করার জন্য AppDbContext কে register করা হচ্ছে
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity চালু করা হচ্ছে, এখন IdentityUser এর বদলে AppUser ব্যবহার হচ্ছে (Bio, ProfilePictureUrl সহ)
builder.Services.AddIdentityApiEndpoints<AppUser>()
    .AddEntityFrameworkStores<AppDbContext>();

// Swagger UI চালু করার সুবিধা
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


// শুধু Development mode এ Swagger UI চালু হবে
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// HTTP কে automatic HTTPS এ পাঠানো হবে
app.UseHttpsRedirection();

// কে "লগইন করা আছে" তা যাচাই করার middleware
app.UseAuthentication();

// লগইন করা user এর "কী করার অনুমতি আছে" তা যাচাই করার middleware
app.UseAuthorization();

// আমাদের নিজের বানানো Controller গুলোর route চালু করা হচ্ছে
app.MapControllers();

// Identity এর নিজস্ব endpoint (register, login...) চালু করা হচ্ছে, এখন AppUser দিয়ে
app.MapIdentityApi<AppUser>();

// অ্যাপ চালু হচ্ছে, request নেওয়ার জন্য প্রস্তুত
app.Run();