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


var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<IFileService, FileService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
