using Chapi.Api.Controllers;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Chapi.Api.Services.CrudServices;
using Chapi.IntegrationTests.Spies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Chapi.Api.Models.User;

namespace Chapi.IntegrationTests
{
    public class ControllerTests : IClassFixture<ConfigurationFixture>
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheSpy;

        private readonly UsersController _usersController;
        private readonly GroupsController _groupsController;

        public ControllerTests(ConfigurationFixture fixture)
        {
            _configuration = fixture.Configuration;

            var usersConfigData = GetCrudConfigData<UserWithId>(_configuration, "UsersConfigData");
            var groupsConfigData = GetCrudConfigData<GroupWithId>(_configuration, "GroupsConfigData");

            var cosmosDbiUri = _configuration.GetSection("CosmosConfigData")["CosmosDbUri"] ?? throw new ArgumentException("Appsetting \"CosmosConfigData:CosmosDbUri\" is missing");

            var services = new ServiceCollection();
            services.AddDistributedMemoryCache();

            var cache = services.BuildServiceProvider().GetRequiredService<IDistributedCache>();
            _cacheSpy = new CacheServiceSpy(cache);

            var runtimeInfo = new RuntimeInfo(true);

            var groupService = new GroupService(groupsConfigData, new CosmosConfigData(cosmosDbiUri), _cacheSpy, runtimeInfo);
            var userService = new UserService(usersConfigData, new CosmosConfigData(cosmosDbiUri), _cacheSpy, runtimeInfo, groupService);

            _usersController = new UsersController(userService, runtimeInfo);
            _groupsController = new GroupsController(groupService, runtimeInfo);
        }

        private static CrudConfigData<T> GetCrudConfigData<T>(IConfiguration configuration, string configKey)
        {
            var configData = configuration.GetSection(configKey).Get<CrudConfigDataDto<T>>();
            if (configData == null) throw new InvalidOperationException($"configKey data is missing or invalid.");
            return configData.ToValidated();
        }

        private static User _testerMcgee = new User()
        {
            Email = "tester_mcgee@chapi-testing.com",
            Organization = "developers",
            Name = "Tester McGee",
            ProfilePicture = "A fine portrait of Mr. Tester McGee",
            Access =
            [
                new UserAccess()
                {
                    Application = "Chapi Testing App",
                    Roles = ["reader", "tester"]
                }
            ]
        };

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[] {
                _testerMcgee,
                typeof(OkObjectResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _testerMcgee.Email,
                    Organization = _testerMcgee.Organization,
                },
                typeof(OkObjectResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = string.Empty,
                    Organization = _testerMcgee.Organization,
                    Name = _testerMcgee.Name,
                    ProfilePicture = _testerMcgee.ProfilePicture,
                    Access = _testerMcgee.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = null,
                    Organization = _testerMcgee.Organization,
                    Name = _testerMcgee.Name,
                    ProfilePicture = _testerMcgee.ProfilePicture,
                    Access = _testerMcgee.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _testerMcgee.Email,
                    Organization = string.Empty,
                    Name = _testerMcgee.Name,
                    ProfilePicture = _testerMcgee.ProfilePicture,
                    Access = _testerMcgee.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _testerMcgee.Email,
                    Organization = null,
                    Name = _testerMcgee.Name,
                    ProfilePicture = _testerMcgee.ProfilePicture,
                    Access = _testerMcgee.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User(),
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _testerMcgee.Email,
                    Organization = "This Organization Does Not Exist",
                    Name = _testerMcgee.Name,
                    ProfilePicture = _testerMcgee.ProfilePicture,
                    Access = _testerMcgee.Access
                    },
                typeof(NotFoundResult)
            };

        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public async Task CreateTests(User user, Type expectedResult)
        {
            IActionResult result;

            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            try
            {
                result = await _usersController.Post(user, cancellationSource.Token);
                Assert.IsType(expectedResult, result);
            }
            finally
            {
                // Always clean up after test execution (pass/fail)
                await CleanupTestUser(user);
            }
        }

        private async Task CleanupTestUser(User user)
        {
            using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            await _usersController.Delete(
                new UserMinimalDto { Email = user.Email, Organization = user.Organization },
                cancellationSource.Token);
        }
    }
}
