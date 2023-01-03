using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreTesting.Api.Tests
{
    public class SettingsControllerTests
    {
        [Fact]
        public async Task Gets_production_setting_if_no_other_configuration_is_set_up()
        {
            var application = new WebApplicationFactory<Program>();

            var client = application.CreateClient();

            var response = await client.GetAsync("/settings/mysetting");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Production setting", await response.Content.ReadAsStringAsync());
        }
        
        [Fact]
        public async Task Gets_test_setting_if_environment_name_is_set_to_Test()
        {
            var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.UseEnvironment("Test");
            });

            var client = application.CreateClient();

            var response = await client.GetAsync("/settings/mysetting");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Test setting", await response.Content.ReadAsStringAsync());
        }
        
        [Fact]
        public async Task Gets_in_memory_setting_if_set()
        {
            var application = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => {
                builder.ConfigureAppConfiguration(config => {
                    config.AddInMemoryCollection(new Dictionary<string, string> {
                        { "MySetting", "In-memory setting" }
                    });
                });
            });

            var client = application.CreateClient();

            var response = await client.GetAsync("/settings/mysetting");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("In-memory setting", await response.Content.ReadAsStringAsync());
        }
    }
}