using AspNetCoreTesting.Api.Data;
using AspNetCoreTesting.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreTesting.Api.Services
{
    public class Users : IUsers
    {
        private readonly ApiContext _context;
        private readonly INotificationService _notificationService;

        public Users(ApiContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public Task<User[]> All()
        {
            return _context.Users.ToArrayAsync();
        }

        public Task<User?> WithId(int id)
        {
            return _context.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User> Add(string firstName, string lastName)
        {
            var user = User.Create(firstName, lastName);
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
            await _notificationService.SendUserCreatedNotification(user);
            return user;
        }
    }
}
