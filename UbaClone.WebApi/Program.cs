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
        Version = "v1",
        Description = "An Api for Uba Mobile app clone",
        Contact = new OpenApiContact
        {
            Name = "Osita Chris",
            Email = "chrisjerry070@gmail.com",
            Url = new Uri("https://example.com/terms")
        }
    });
});
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "SampleDb";
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policyBuilder =>
    {
        policyBuilder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

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


app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins"); ;
app.UseAuthentication();  // Use authentication middleware
app.UseAuthorization();   // Use authorization middleware
app.MapControllers();
app.Run();
