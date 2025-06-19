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
builder.Services.AddDbContext<DataContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("DefaultConnection");
    options.UseSqlServer(connectionString);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConnection = builder.Configuration.GetConnectionString("RedisConnection")
        ?? Environment.GetEnvironmentVariable("RedisConnection");
    options.Configuration = redisConnection;
    options.InstanceName = "SampleDb";
});

// builder.Services.AddDbContext<DataContext>(options =>
// {
//     options.UseSqlServer("Data Source=DESKTOP-DRLUK05\\SQLEXPRESS;Initial Catalog=UbaCloneDb;Integrated Security=True;Trust Server Certificate=True");
// });
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
//     options.InstanceName = "SampleDb";
// });
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowFrontend", policyBuilder =>
//     {
//         policyBuilder.WithOrigins("http://localhost:3000", "https://uba-mobile-app.vercel.app") 
//             .AllowAnyHeader()
//             .AllowAnyMethod()
//             .AllowCredentials(); 
//     });
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()      // 👈 Allows ANY origin
            .AllowAnyMethod()      // GET, POST, PUT, etc.
            .AllowAnyHeader();     // Accepts all headers
    });
});



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




// app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();  // Use authentication middleware
app.UseAuthorization();   // Use authorization middleware
app.MapControllers();

app.MapGet("/test-cors", () => Results.Ok("CORS works"))
   .RequireCors("AllowFrontend");
app.Run();


//  "AzureAd": {
//     "Instance": "https://login.microsoftonline.com/",
//     "Domain": "ositaristgmail.onmicrosoft.com",
//     "TenantId": "edd4c6ec-8599-4d1a-b149-6aa4d4b120f1",
//     "ClientId": "72f07754-1bdd-4ecf-9796-9dc6a81b7139",
//     "CallbackPath": "/signin-oidc",
//     "Scopes": ""
//   }
