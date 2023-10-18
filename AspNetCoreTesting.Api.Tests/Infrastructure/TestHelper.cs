using Bazinga.AspNetCore.Authentication.Basic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class TestHelper<TEntryPoint> where TEntryPoint : class
    {
        private Dictionary<Type, Func<IServiceProvider, DbContext>> _dbContexts = new();
        private Dictionary<Type, Func<SqlCommand, Task>> _dbPreparations = new();
        private List<Action<IServiceCollection>> _serviceOverrides = new();
        private List<Action<HttpClient>> _clientPreparations = new();
        private Dictionary<Type, Func<SqlCommand, Task>> _postTestDbValidations = new();
        private string? _environmentName;
        private IDictionary<string, string?>? _config;

        public TestHelper<TEntryPoint> AddDbContext<TContext>(string connectionString) where TContext : DbContext
        {
            _dbContexts.Add(typeof(TContext), x =>
            {
                var options = new DbContextOptionsBuilder<TContext>()
                                    .UseSqlServer(connectionString)
                                    .Options;
                return (TContext)Activator.CreateInstance(typeof(TContext), options)!;
            });

            return this;
        }

        public TestHelper<TEntryPoint> PrepareDb<TContext>(Func<SqlCommand, Task> callback) where TContext : DbContext
        {
            _dbPreparations.Add(typeof(TContext), callback);

            return this;
        }

        public TestHelper<TEntryPoint> AddFakeAuth(string username, string password)
        {
            _serviceOverrides.Add(services =>
            {
                services.AddAuthentication()
                                .AddBasicAuthentication(credentials => Task.FromResult(credentials.username == username && credentials.password == password));

                services.AddAuthorization(config =>
                {
                    config.DefaultPolicy = new AuthorizationPolicyBuilder(config.DefaultPolicy)
                                                .AddAuthenticationSchemes(BasicAuthenticationDefaults.AuthenticationScheme)
                                                .Build();
                });
            });

            return this;
        }

        public TestHelper<TEntryPoint> AddFakeClientAuth(string username, string password)
        {
            _clientPreparations.Add(client =>
            {
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{username}:{password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            });

            return this;
        }

        public TestHelper<TEntryPoint> OverrideServices(Action<IServiceCollection> callback)
        {
            _serviceOverrides.Add(callback);

            return this;
        }

        public TestHelper<TEntryPoint> ValidatePostTestDb<TContext>(Func<SqlCommand, Task> callback) where TContext : DbContext
        {
            _postTestDbValidations.Add(typeof(TContext), callback);

            return this;
        }

        public TestHelper<TEntryPoint> SetEnvironmentName(string name)
        {
            _environmentName = name;

            return this;
        }

        public TestHelper<TEntryPoint> AddConfiguration(IDictionary<string, string?> config)
        {
            _config = config;

            return this;
        }

        public async Task Run(Func<HttpClient, Task> test)
        {
            var application = new WebApplicationFactory<TEntryPoint>().WithWebHostBuilder(builder => {
                if (_environmentName != null)
                    builder.UseEnvironment(_environmentName);
                
                if (_config != null)
                {
                    var config = new ConfigurationBuilder()
                                    .AddInMemoryCollection(_config)
                                    .Build();
                    builder.UseConfiguration(config);
                }

                builder.ConfigureTestServices(services => {

                    foreach (var key in _dbContexts.Keys)
                    {
                        services.AddSingleton(key, x => _dbContexts[key](x));
                    }

                    foreach (var fn in _serviceOverrides)
                    {
                        fn(services);
                    }
                });
            });

            using (var services = application.Services.CreateScope())
            {
                var transactions = new Dictionary<Type, IDbContextTransaction>();

                try
                {
                    foreach (var key in _dbContexts.Keys)
                    {
                        var ctx = (DbContext)services.ServiceProvider.GetRequiredService(key);

                        transactions.Add(key, ctx.Database.BeginTransaction());

                        if (_dbPreparations.ContainsKey(key))
                        {

                            var conn = ctx.Database.GetDbConnection();
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = transactions[key].GetDbTransaction();
                                await _dbPreparations[key]((SqlCommand)cmd);
                            }
                        }
                    }

                    var client = application.CreateClient();

                    foreach (var fn in _clientPreparations)
                    {
                        fn(client);
                    }

                    await test(client);

                    foreach (var key in _dbContexts.Keys)
                    {
                        if (_postTestDbValidations.ContainsKey(key))
                        {
                            var ctx = (DbContext)services.ServiceProvider.GetRequiredService(key);
                            var conn = ctx.Database.GetDbConnection();

                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.Transaction = transactions.GetValueOrDefault(key)?.GetDbTransaction();
                                await _postTestDbValidations[key]((SqlCommand)cmd);
                            }
                        }
                    }
                } 
                finally
                {
                    foreach (var tran in transactions.Values)
                    {
                        tran.Rollback();
                    }
                }
            }
        }
    }
}
