using AspNetCoreTesting.Api.Data;
using AspNetCoreTesting.Api.Data.Entities;
using AspNetCoreTesting.Api.Services;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreTesting.Api.Tests
{
    public class UsersControllerTests
    {
        private const string SqlConnectionString = "Server=localhost,14331;Database=AspNetCoreTesting;User Id=sa;Password=P@ssword123";
        private INotificationService NotificationServiceFake = A.Fake<INotificationService>();

        [Fact]
        public async Task Get_returns_all_users()
        {
            var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.ConfigureTestServices(services => {
                    var options = new DbContextOptionsBuilder<ApiContext>()
                                    .UseSqlServer(SqlConnectionString)
                                    .Options;
                    services.AddSingleton(options);
                    services.AddSingleton<ApiContext>();
                    services.AddSingleton(NotificationServiceFake);
                });
            });

            using (var services = application.Services.CreateScope())
            {
                IDbContextTransaction? transaction = null;
                try
                {
                    var ctx = services.ServiceProvider.GetRequiredService<ApiContext>();
                    transaction = ctx.Database.BeginTransaction();

                    var conn = ctx.Database.GetDbConnection();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = transaction.GetDbTransaction();
                        cmd.CommandText = "SET IDENTITY_INSERT Users ON; " +
                                        "INSERT INTO Users (Id, FirstName, LastName) VALUES" +
                                        "(1, 'John', 'Doe'), " +
                                        "(2, 'Jane', 'Doe'); " +
                                        "SET IDENTITY_INSERT Users OFF;";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    var client = application.CreateClient();

                    var response = await client.GetAsync("/users");

                    dynamic users = JArray.Parse(await response.Content.ReadAsStringAsync());

                    Assert.Equal(2, users.Count);
                    Assert.Equal("John", (string)users[0].firstName);
                    Assert.Equal("Doe", (string)users[1].lastName);
                }
                finally
                {
                    transaction?.Rollback();
                }
            }
        }

        [Fact]
        public async Task Put_returns_Created_if_successful()
        {
            var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.ConfigureTestServices(services => {
                    var options = new DbContextOptionsBuilder<ApiContext>()
                                    .UseSqlServer(SqlConnectionString)
                                    .Options;
                    services.AddSingleton(options);
                    services.AddSingleton<ApiContext>();
                    services.AddSingleton(NotificationServiceFake);
                });
            });

            using (var services = application.Services.CreateScope())
            {
                IDbContextTransaction? transaction = null;
                try
                {
                    var ctx = services.ServiceProvider.GetRequiredService<ApiContext>();
                    transaction = ctx.Database.BeginTransaction();

                    var client = application.CreateClient();

                    var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                    dynamic user = JObject.Parse(await response.Content.ReadAsStringAsync());

                    Assert.Equal("John", (string)user.firstName);
                    Assert.Equal("Doe", (string)user.lastName);
                    Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
                    Assert.Matches("^http:\\/\\/localhost\\/users\\/\\d+$", response.Headers.Location!.AbsoluteUri.ToLower());

                    var userId = int.Parse(response.Headers.Location!.PathAndQuery.Substring(response.Headers.Location!.PathAndQuery.LastIndexOf("/") + 1));

                    var conn = ctx.Database.GetDbConnection();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.Transaction = transaction.GetDbTransaction();
                        cmd.CommandText = $"SELECT TOP 1 * FROM Users WHERE Id = {userId}";
                        using (var rs = await cmd.ExecuteReaderAsync())
                        {
                            Assert.True(await rs.ReadAsync());
                            Assert.Equal("John", rs["FirstName"]);
                            Assert.Equal("Doe", rs["LastName"]);
                        }
                    }
                }
                finally
                {
                    transaction?.Rollback();
                }
            }
        }

        [Fact]
        public async Task Put_returns_sends_notification_if_successful()
        {
            var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.ConfigureTestServices(services => {
                    var options = new DbContextOptionsBuilder<ApiContext>()
                                    .UseSqlServer(SqlConnectionString)
                                    .Options;
                    services.AddSingleton(options);
                    services.AddSingleton<ApiContext>();
                    services.AddSingleton(NotificationServiceFake);
                });
            });

            using (var services = application.Services.CreateScope())
            {
                IDbContextTransaction? transaction = null;
                try
                {
                    var ctx = services.ServiceProvider.GetRequiredService<ApiContext>();
                    transaction = ctx.Database.BeginTransaction();

                    var client = application.CreateClient();

                    var response = await client.PutAsJsonAsync("/users/", new { firstName = "John", lastName = "Doe" });

                    A.CallTo(() =>
                        NotificationServiceFake.SendUserCreatedNotification(A<User>.That.Matches(x => x.FirstName == "John" && x.LastName == "Doe"))
                    ).MustHaveHappened();
                }
                finally
                {
                    transaction?.Rollback();
                }
            }
        }
    }
}