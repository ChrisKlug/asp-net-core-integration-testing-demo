using AspNetCoreTesting.Api.Data;
using AspNetCoreTesting.Api.Services;
using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCoreTesting.Api.Tests.Infrastructure
{
    public abstract class UserControllerTestBase : TestBase
    {
        private const string SqlConnectionString = "Server=localhost,14331;Database=AspNetCoreTesting;User Id=sa;Password=P@ssword123";
        protected INotificationService NotificationServiceFake = A.Fake<INotificationService>();

        protected Task RunTest(Func<HttpClient, Task> test, Func<DbCommand, Task>? populateDatabase = null, Func<DbCommand, Task>? validateDatabase = null)
        {
            return RunTestInternal(
                    async services =>
                    {
                        if (populateDatabase != null)
                        {
                            var ctx = services.GetRequiredService<ApiContext>();
                            var conn = ctx.Database.GetDbConnection();
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = ctx.Database.CurrentTransaction?.GetDbTransaction();
                                await populateDatabase(cmd);
                            }
                        }
                    },
                    test,
                    async services =>
                    {
                        if (validateDatabase != null)
                        {
                            var ctx = services.GetRequiredService<ApiContext>();
                            var conn = ctx.Database.GetDbConnection();
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = ctx.Database.CurrentTransaction?.GetDbTransaction();
                                await validateDatabase(cmd);
                            }
                        }
                    }
                );
        }

        protected override void ConfigureTestServices(IServiceCollection services)
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                                            .UseSqlServer(SqlConnectionString)
                                            .Options;
            services.AddSingleton(options);
            services.AddSingleton<ApiContext>();
            services.AddSingleton(NotificationServiceFake);
        }
        protected override IEnumerable<IDbContextTransaction> InitializeTransactions(IServiceProvider services)
        {
            var ctx = services.GetRequiredService<ApiContext>();
            return new[] { ctx.Database.BeginTransaction() };
        }
    }
}
