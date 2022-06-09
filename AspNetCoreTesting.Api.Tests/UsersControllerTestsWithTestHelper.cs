using AspNetCoreTesting.Api.Data;
using AspNetCoreTesting.Api.Data.Entities;
using AspNetCoreTesting.Api.Services;
using AspNetCoreTesting.Api.Tests.Extensions;
using AspNetCoreTesting.Api.Tests.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreTesting.Api.Tests
{
    public class UsersControllerTestsWithTestHelper
    {
        private const string SqlConnectionString = "Server=localhost,14331;Database=AspNetCoreTesting;User Id=sa;Password=P@ssword123";
        private INotificationService NotificationServiceFake = A.Fake<INotificationService>();

        [Fact]
        public async Task Get_returns_all_users()
        {
            await GetTestRunner()
                    .PrepareDb<ApiContext>(async cmd => {
                        await cmd.AddUser(1, "John", "Doe");
                        await cmd.AddUser(2,"Jane", "Doe");
                    })
                    .Run(async client => {
                        var response = await client.GetAsync("/users");

                        dynamic users = JArray.Parse(await response.Content.ReadAsStringAsync());

                        Assert.Equal(2, users.Count);
                        Assert.Equal("John", (string)users[0].firstName);
                        Assert.Equal("Doe", (string)users[1].lastName);
                    });
        }

        [Fact]
        public async Task Get_ID_returns_User_if_it_exists()
        {
            await GetTestRunner()
                    .PrepareDb<ApiContext>(async cmd => {
                        await cmd.AddUser();
                    })
                    .Run(async client => {
                        var response = await client.GetAsync("/users/1");

                        dynamic user = JObject.Parse(await response.Content.ReadAsStringAsync());

                        Assert.Equal(1, (int)user.id);
                        Assert.Equal("John", (string)user.firstName);
                        Assert.Equal("Doe", (string)user.lastName);
                    });
        }

        [Fact]
        public async Task Get_ID_returns_404_if_user_id_does_not_exist()
        {
            await GetTestRunner()
                    .Run(async client => {
                        var response = await client.GetAsync("/users/1");

                        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
                    });
        }

        [Fact]
        public async Task Put_returns_BadRequest_if_missing_first_name()
        {
            await GetTestRunner()
                    .Run(async client => {
                        var response = await client.PutAsJsonAsync("/users/", new { lastName = "Doe"  });

                        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
                    });
        }

        [Fact]
        public async Task Put_returns_BadRequest_if_missing_last_name()
        {
            await GetTestRunner()
                    .Run(async client => {
                        var response = await client.PutAsJsonAsync("/users/", new { firstName = "John" });

                        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
                    });
        }

        [Fact]
        public async Task Put_returns_Created_if_successful()
        {
            var userId = -1;

            await GetTestRunner()
                    .ValidatePostTestDb<ApiContext>(async cmd => {
                        cmd.CommandText = $"SELECT TOP 1 * FROM Users WHERE Id = {userId}";
                        using (var rs = await cmd.ExecuteReaderAsync())
                        {
                            Assert.True(await rs.ReadAsync());
                            Assert.Equal("John", rs["FirstName"]);
                            Assert.Equal("Doe", rs["LastName"]);
                        }
                    })
                    .Run(async client => {
                        var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                        dynamic user = JObject.Parse(await response.Content.ReadAsStringAsync());

                        Assert.Equal("John", (string)user.firstName);
                        Assert.Equal("Doe", (string)user.lastName);

                        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
                        Assert.Matches("^http:\\/\\/localhost\\/users\\/\\d+$", response.Headers.Location!.AbsoluteUri.ToLower());

                        userId = int.Parse(response.Headers.Location!.PathAndQuery.Substring(response.Headers.Location!.PathAndQuery.LastIndexOf("/") + 1));
                    });
        }

        [Fact]
        public async Task Put_returns_sends_notification_if_successful()
        {
            await GetTestRunner()
                    .Run(async client => {
                        await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });
                        A.CallTo(() => 
                            NotificationServiceFake.SendUserCreatedNotification(A<User>.That.Matches(x => x.FirstName == "John" && x.LastName == "Doe"))
                        ).MustHaveHappened();
                    });
        }

        private TestHelper<Program> GetTestRunner()
            => new TestHelper<Program>()
                        .AddDbContext<ApiContext>(SqlConnectionString)
                        .OverrideServices(services => {
                            services.AddSingleton(NotificationServiceFake);
                        });
    }
}