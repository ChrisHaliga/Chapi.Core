using Chapi.Api.Models.Configuration;
using Microsoft.Extensions.Configuration;

namespace Chapi.IntegrationTests.Fixtures
{
    public class ConfigurationFixture
    {
        public IConfiguration Configuration { get; }

        public ConfigurationFixture()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // Ensure it looks in test/bin/Debug
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            Configuration = configBuilder.Build();
        }
    }
}
