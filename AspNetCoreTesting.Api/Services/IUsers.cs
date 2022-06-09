using AspNetCoreTesting.Api.Data.Entities;

namespace AspNetCoreTesting.Api.Services
{
    public interface IUsers
    {
        Task<User[]> All();
        Task<User?> WithId(int id);
        Task<User> Add(string firstName, string lastName);
    }
}
