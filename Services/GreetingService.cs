using Backend.Interfaces;

namespace Backend.Services
{
    public class GreetingService : IGreetingService
    {
        public string GetGreeting(string name)
        {
            return $"Hello {name}, your File Upload API is wired up correctly!";
        }
    }
}