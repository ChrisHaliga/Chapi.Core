using Chapi.Api.Controllers;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models;
using Chapi.Api.Services.CrudServices;
using Chapi.IntegrationTests.Spies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Chapi.Api.Services;
using Chapi.IntegrationTests.MemberData;

namespace Chapi.IntegrationTests
{
    public class ControllerTestsFixture :IDisposable
    {
        public IConfiguration Configuration { get; }
        public UsersController UsersController { get; }
        public GroupsController GroupsController { get; }
        public ApplicationsController ApplicationsController { get; }

        public CacheServiceSpy CacheSpy { get; }

        public ControllerTestsFixture()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // Ensure it looks in test/bin/Debug
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true);

            Configuration = configBuilder.Build();

            var groupsConfigData = GetCrudConfigData<GroupWithId>(Configuration, "GroupsConfigData");
            var applicationsConfigData = GetCrudConfigData<ApplicationWithId>(Configuration, "ApplicationsConfigData");
            var usersConfigData = GetCrudConfigData<UserWithId>(Configuration, "UsersConfigData");

            var cosmosDbiUri = Configuration.GetSection("CosmosConfigData")["CosmosDbUri"] ?? throw new ArgumentException("Appsetting \"CosmosConfigData:CosmosDbUri\" is missing");

            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();

            var cache = services.BuildServiceProvider().GetRequiredService<IDistributedCache>();
            CacheSpy = new CacheServiceSpy(cache);

            var runtimeInfo = new RuntimeInfo(true);

            var groupService = new GroupService(groupsConfigData, new CosmosConfigData(cosmosDbiUri), CacheSpy, runtimeInfo);
            var applicationService = new ApplicationService(applicationsConfigData, new CosmosConfigData(cosmosDbiUri), CacheSpy, runtimeInfo);
            var userService = new UserService(usersConfigData, new CosmosConfigData(cosmosDbiUri), CacheSpy, runtimeInfo);

            var chapiService = new ChapiService(userService, groupService, applicationService);

            UsersController = new UsersController(chapiService, runtimeInfo);
            GroupsController = new GroupsController(chapiService, runtimeInfo);
            ApplicationsController = new ApplicationsController(chapiService, runtimeInfo);
        }

        private static CrudConfigData<T> GetCrudConfigData<T>(IConfiguration configuration, string configKey)
        {
            var configData = configuration.GetSection(configKey).Get<CrudConfigDataDto<T>>();
            if (configData == null) throw new InvalidOperationException($"configKey data is missing or invalid.");
            return configData.ToValidated();
        }

        public void Dispose()
        {
            Task.Run(async () => await UserMemberData.RemoveTestUser(UsersController));
        }
    }
}
