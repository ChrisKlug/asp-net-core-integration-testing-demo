using AspNetCoreTesting.Api.Data.Entities;

namespace AspNetCoreTesting.Api.Services
{
    public interface INotificationService
    {
        Task SendUserCreatedNotification(User user);
    }
}
