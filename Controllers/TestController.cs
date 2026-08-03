using Microsoft.AspNetCore.Mvc;
using Backend.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IGreetingService _greetingService;
        private readonly Cloudinary _cloudinary;

        public TestController(IGreetingService greetingService, Cloudinary cloudinary)
        {
            _greetingService = greetingService;
            _cloudinary = cloudinary;
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            var message = _greetingService.GetGreeting(name);
            return Ok(new { message });
        }

        [HttpGet("cloudinary-check")]
        public IActionResult CheckCloudinary()
        {
            // Pinging Cloudinary's "usage" endpoint confirms our credentials are valid
            var usage = _cloudinary.GetUsageAsync().Result;
            return Ok(new
            {
                message = "Cloudinary connected successfully!",
                plan = usage.Plan,
                credits = usage.Credits,
                storageUsed = usage.Storage.Used
            });
        }
    }
}