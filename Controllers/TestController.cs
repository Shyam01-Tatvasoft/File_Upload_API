using Microsoft.AspNetCore.Mvc;
using Backend.Interfaces;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IGreetingService _greetingService;

        public TestController(IGreetingService greetingService)
        {
            _greetingService = greetingService;
        }

        [HttpGet("{name}")]
        public IActionResult Get(string name)
        {
            var message = _greetingService.GetGreeting(name);
            return Ok(new { message });
        }
    }
}