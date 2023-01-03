using AspNetCoreTesting.Api.Tests.Infrastructure;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace AspNetCoreTesting.Api.Tests
{
    public class SettingsControllerTestsWithTestBase : SettingsControllerTestBase
    {
        [Fact]
        public Task Gets_production_setting_if_no_other_configuration_is_set_up()
            => RunTest(
                    test: async client =>
                    {
                        var response = await client.GetAsync("/settings/mysetting");

                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("Production setting", await response.Content.ReadAsStringAsync());
                    }
                );
        
        [Fact]
        public Task Gets_test_setting_if_environment_name_is_set_to_Test()
            => RunTest(
                    test: async client =>
                    {
                        var response = await client.GetAsync("/settings/mysetting");

                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("Test setting", await response.Content.ReadAsStringAsync());
                    }, 
                    "Test"
                );
        
        [Fact]
        public Task Gets_in_memory_setting_if_set()
            => RunTest(
                    test: async client =>
                    {
                        var response = await client.GetAsync("/settings/mysetting");

                        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                        Assert.Equal("In-memory setting", await response.Content.ReadAsStringAsync());
                    }, 
                    configuration: new Dictionary<string, string> {
                        { "MySetting", "In-memory setting" }
                    }
                );
    }
}