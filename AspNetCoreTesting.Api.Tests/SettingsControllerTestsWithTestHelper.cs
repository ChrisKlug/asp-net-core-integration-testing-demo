using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AspNetCoreTesting.Api.Tests.Infrastructure;
using Xunit;

namespace AspNetCoreTesting.Api.Tests
{
    public class SettingsControllerTestsWithTestHelper
    {
        [Fact]
        public Task Gets_production_setting_if_no_other_configuration_is_set_up()
            => new TestHelper<Program>()
                    .Run(async client => {
                        var response = await client.GetAsync("/settings/mysetting");

                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("Production setting", await response.Content.ReadAsStringAsync());
                    });
        
        [Fact]
        public Task Gets_test_setting_if_environment_name_is_set_to_Test()
            => new TestHelper<Program>()
                    .SetEnvironmentName("Test")
                    .Run(async client =>
                    {
                        var response = await client.GetAsync("/settings/mysetting");

                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("Test setting", await response.Content.ReadAsStringAsync());
                    });
        
        [Fact]
        public Task Gets_in_memory_setting_if_set()
            => new TestHelper<Program>()
                    .AddConfiguration(new Dictionary<string, string> {
                        { "MySetting", "In-memory setting" }
                    })
                    .Run(async client =>
                    {
                        var response = await client.GetAsync("/settings/mysetting");

                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("In-memory setting", await response.Content.ReadAsStringAsync());
                    });
    }
}