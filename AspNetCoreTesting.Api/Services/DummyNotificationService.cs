using AspNetCoreTesting.Api.Data.Entities;

namespace AspNetCoreTesting.Api.Services
{
    public class DummyNotificationService : INotificationService
    {
        public Task SendUserCreatedNotification(User user)
        {
            Console.WriteLine($"User {user.FirstName} {user.LastName} was added!");
            return Task.CompletedTask;
        }
    }
}
