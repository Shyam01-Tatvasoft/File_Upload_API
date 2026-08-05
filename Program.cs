using Microsoft.EntityFrameworkCore;
using Backend.Interfaces;
using Backend.Services;
using Backend.Data;
using Backend.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Backend.Validators;
using CloudinaryDotNet;
using Backend.Configurations;
using Backend.Repositories;
using Backend.Middleware;
using Serilog;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IGreetingService, GreetingService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<FileMappingProfile>());
builder.Services.AddValidatorsFromAssemblyContaining<FileUploadDtoValidator>();
builder.Services.AddFluentValidationAutoValidation();
// Bind the Cloudinary section from configuration into our strongly-typed class
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("Cloudinary"));

// Register the Cloudinary client itself as a Singleton
builder.Services.AddSingleton(sp =>
{
    var settings = builder.Configuration.GetSection("Cloudinary").Get<CloudinarySettings>();
    var account = new Account(settings!.CloudName, settings.ApiKey, settings.ApiSecret);
    return new Cloudinary(account);
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 200 * 1024 * 1024; // 200 MB
});

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("UploadPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });

    options.RejectionStatusCode = 429; // Too Many Requests
});

builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

const string CorsPolicyName = "AllowReactApp";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite's default dev server port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicyName);
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();

app.Run();
