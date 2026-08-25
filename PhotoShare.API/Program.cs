using Microsoft.EntityFrameworkCore;
using PhotoShare.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Services যোগ করা
builder.Services.AddControllers();

// DbContext কে DI Container এ register করা
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();   // ← এইটা আছে তো?

var app = builder.Build();

// Swagger UI (আগে install করেছিলেন)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();