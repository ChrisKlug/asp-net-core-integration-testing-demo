namespace AspNetCoreTesting.Api.Data.Entities
{
    public class User
    {
        public User()
        {

        }

        public static User Create(string firstName, string lastName) =>
            new User { FirstName = firstName, LastName = lastName };

        public int Id { get; private set; }
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
    }
}
