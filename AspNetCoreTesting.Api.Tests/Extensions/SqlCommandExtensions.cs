using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace AspNetCoreTesting.Api.Tests.Extensions
{
    public static class SqlCommandExtensions
    {
        public static Task AddUser(this SqlCommand cmd, int id = 1, string firstName = "John", string lastName = "Doe")
        {
            cmd.CommandText = "SET IDENTITY_INSERT Users ON; " +
                            "INSERT INTO Users (Id, FirstName, LastName) " +
                            $"VALUES ({id}, '{firstName}', '{lastName}'); " +
                            "SET IDENTITY_INSERT Users OFF;";
            return cmd.ExecuteNonQueryAsync();
        }
    }
}
