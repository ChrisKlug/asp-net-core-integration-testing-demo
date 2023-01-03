using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AspNetCoreTesting.Api.Tests.Infrastructure
{
    public abstract class TestBase
    {
        protected async Task RunTestInternal(Func<IServiceProvider, Task> populateDatabase, Func<HttpClient, Task> test, 
                                             Func<IServiceProvider, Task> validateDatabase, string? environmentName = null,
                                             IDictionary<string, string>? configuration = null)
        {
            var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.ConfigureTestServices(ConfigureTestServices);

                if (environmentName != null)
                    builder.UseEnvironment(environmentName);
                    
                if (configuration != null)
                    builder.ConfigureAppConfiguration((ctx, config) => {
                        config.AddInMemoryCollection(configuration);
                    });
            });

            using (var services = application.Services.CreateScope())
            {
                IEnumerable<IDbContextTransaction> transactions = new IDbContextTransaction[0];
                try
                {
                    transactions = InitializeTransactions(services.ServiceProvider);

                    await populateDatabase(services.ServiceProvider);

                    var client = application.CreateClient();

                    await test(client);

                    await validateDatabase(services.ServiceProvider);
                }
                finally
                {
                    foreach (var transaction in transactions)
                    {
                        transaction.Rollback();
                    }
                }
            }
        }

        protected abstract void ConfigureTestServices(IServiceCollection services);
        protected abstract IEnumerable<IDbContextTransaction> InitializeTransactions(IServiceProvider services);
    }
}
