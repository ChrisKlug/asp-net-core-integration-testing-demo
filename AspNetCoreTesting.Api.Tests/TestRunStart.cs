using AspNetCoreTesting.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("AspNetCoreTesting.Api.Tests.TestRunStart", "AspNetCoreTesting.Api.Tests")]

namespace AspNetCoreTesting.Api.Tests
{
    public class TestRunStart : XunitTestFramework
    {
        public TestRunStart(IMessageSink messageSink) : base(messageSink)
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                                .UseSqlServer("Server=localhost,14331;Database=AspNetCoreTesting;User Id=sa;Password=P@ssword123;");
            var dbContext = new ApiContext(options.Options);
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }
    }
}
