using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Activity 2: Resolve CORS errors by allowing the front-end to access this API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Activity 4: Optimize performance with output caching to minimize server load
builder.Services.AddOutputCache();

var app = builder.Build();

// Apply the policies
app.UseCors("AllowAll");
app.UseOutputCache();

// Activity 2 & 3: Correct route (/api/productlist) and nested JSON structure
// Activity 4: Cached the endpoint output for 5 minutes
app.MapGet("/api/productlist", () =>
{
    return new[]
    {
        new
        {
            Id = 1,
            Name = "Laptop",
            Price = 1200.50,
            Stock = 25,
            Category = new { Id = 101, Name = "Electronics" }
        },
        new
        {
            Id = 2,
            Name = "Headphones",
            Price = 50.00,
            Stock = 100,
            Category = new { Id = 102, Name = "Accessories" }
        }
    };
}).CacheOutput(c => c.Expire(System.TimeSpan.FromMinutes(5)));

app.Run();