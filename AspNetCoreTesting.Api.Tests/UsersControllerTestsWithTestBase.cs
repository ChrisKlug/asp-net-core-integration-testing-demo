using AspNetCoreTesting.Api.Data.Entities;
using AspNetCoreTesting.Api.Tests.Infrastructure;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreTesting.Api.Tests
{
    public class UsersControllerTestsWithTestBase : UserControllerTestBase
    {
        public Task Get_returns_all_users()
            => RunTest(
                    populateDatabase: async cmd =>
                    {
                        cmd.CommandText = "SET IDENTITY_INSERT Users ON; " +
                                        "INSERT INTO Users (Id, FirstName, LastName) VALUES" +
                                        "(1, 'John', 'Doe'), " +
                                        "(2, 'Jane', 'Doe'); " +
                                        "SET IDENTITY_INSERT Users OFF;";
                        await cmd.ExecuteNonQueryAsync();
                    },
                    test: async client =>
                    {
                        var response = await client.GetAsync("/users");

                        dynamic users = JArray.Parse(await response.Content.ReadAsStringAsync());

                        Assert.Equal(2, users.Count);
                        Assert.Equal("John", (string)users[0].firstName);
                        Assert.Equal("Doe", (string)users[1].lastName);
                    }
                );

        [Fact]
        public Task Put_returns_Created_if_successful()
        {
            var userId = -1;

            return RunTest(
                        test: async client =>
                        {
                            var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                            dynamic user = JObject.Parse(await response.Content.ReadAsStringAsync());

                            Assert.Equal("John", (string)user.firstName);
                            Assert.Equal("Doe", (string)user.lastName);
                            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
                            Assert.Matches("^http:\\/\\/localhost\\/users\\/\\d+$", response.Headers.Location!.AbsoluteUri.ToLower());

                            userId = int.Parse(response.Headers.Location!.PathAndQuery.Substring(response.Headers.Location!.PathAndQuery.LastIndexOf("/") + 1));
                        },
                        validateDatabase: async cmd =>
                        {
                            cmd.CommandText = $"SELECT TOP 1 * FROM Users WHERE Id = {userId}";
                            using (var rs = await cmd.ExecuteReaderAsync())
                            {
                                Assert.True(await rs.ReadAsync());
                                Assert.Equal("John", rs["FirstName"]);
                                Assert.Equal("Doe", rs["LastName"]);
                            }
                        }
                    );
        }

        [Fact]
        public Task Put_returns_sends_notification_if_successful()
            => RunTest(async client =>
            {
                var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                A.CallTo(() =>
                    NotificationServiceFake.SendUserCreatedNotification(A<User>.That.Matches(x => x.FirstName == "John" && x.LastName == "Doe"))
                ).MustHaveHappened();
            });
    }
}