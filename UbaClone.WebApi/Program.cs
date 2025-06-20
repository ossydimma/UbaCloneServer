using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UbaClone.WebApi.Data;
using UbaClone.WebApi.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;



var builder = WebApplication.CreateBuilder(args);

// Combine both JwtBearer and MicrosoftIdentityWebApi configurations
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

        options.RequireHttpsMetadata = true; // Always ensure HTTPS
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // No clock skew, tokens expire exactly when they should
        };
    });

// Add other services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My Api",
        Version = "v1"
    });
});

builder.Services.AddScoped<IUsersRepository, UsersRepository>();

var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DefaultConnection");

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseNpgsql(conn);

});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection")
        ?? Environment.GetEnvironmentVariable("RedisConnection");
    options.Configuration = redisConnection;
    options.InstanceName = "SampleDb";
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policyBuilder =>
    {
        policyBuilder.WithOrigins("https://uba-mobile-app.onrender.com") 
            .AllowAnyHeader()
            .AllowAnyMethod(); 
    });
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll", policy =>
//     {
//         policy
//             .AllowAnyOrigin()      // 👈 Allows ANY origin
//             .AllowAnyMethod()      // GET, POST, PUT, etc.
//             .AllowAnyHeader();     // Accepts all headers
//     });
// });



builder.Services.AddAuthorization();

var app = builder.Build();

Console.WriteLine($"Current Environment: {app.Environment.EnvironmentName}");

// Enable Swagger as the default page
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UBACLONE API V1");
        c.RoutePrefix = string.Empty; // Set Swagger as the default page
    });
}


app.UseCors("AllowFrontend");
// app.UseHttpsRedirection();
// app.UseCors("AllowAll");
app.UseAuthentication();  // Use authentication middleware
app.UseAuthorization();   // Use authorization middleware
app.MapControllers();

app.MapGet("/test-cors", () => Results.Ok("CORS works"))
   .RequireCors("AllowFrontend");
app.Run();
