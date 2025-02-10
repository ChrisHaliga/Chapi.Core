using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services;
using Chapi.Api.Wrappers;
using Chapi.IntegrationTests.Fixtures;
using Microsoft.Extensions.Configuration;
using Moq;
using static Chapi.Api.Models.User;

namespace Chapi.IntegrationTests
{
    public class CosmosWrapperTests : IClassFixture<ConfigurationFixture>
    {
        private readonly IConfiguration _configuration;
        private readonly Mock<ICacheService> _cacheMock;

        private readonly CosmosWrapper _usersCosmosWrapper;
        private readonly CosmosWrapper _groupsCosmosWrapper;


        public CosmosWrapperTests(ConfigurationFixture fixture)
        {
            _configuration = fixture.Configuration;

            var usersConfigData = GetCrudConfigData(_configuration, "UsersConfigData");
            var groupsConfigData = GetCrudConfigData(_configuration, "GroupsConfigData");

            var cosmosDbiUri = _configuration.GetSection("CosmosConfigData")["CosmosDbUri"] ?? throw new ArgumentException("Appsetting \"CosmosConfigData:CosmosDbUri\" is missing");

            _cacheMock = new Mock<ICacheService>();

            _usersCosmosWrapper = new CosmosWrapper(usersConfigData.DatabaseName, usersConfigData.ContainerName, cosmosDbiUri, _cacheMock.Object, new RuntimeInfo(true));
            _groupsCosmosWrapper = new CosmosWrapper(groupsConfigData.DatabaseName, groupsConfigData.ContainerName, cosmosDbiUri, _cacheMock.Object, new RuntimeInfo(true));
        }

        private static CrudConfigData<object> GetCrudConfigData(IConfiguration configuration, string configKey)
        {
            var configData = configuration.GetSection(configKey).Get<CrudConfigDataDto<object>>();
            if (configData == null) throw new InvalidOperationException($"configKey data is missing or invalid.");
            return configData.ToValidated();
        }

        [Fact]
        public async Task CRUD_Selfcleaning_HappyPath()
        {
            var testUser = new User()
            {
                Email = "tester_mcgee@chapi-testing.com",
                Organization = "Chapi Testers",
                Name = "Tester McGee",
                ProfilePicture = "A fine portrait of Mr. Tester McGee"
            };

            // Create User
            UserWithId createUserResult;
            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                createUserResult = await _usersCosmosWrapper.CreateItemAsync(new UserWithId(testUser), cancellationSource.Token);
            }
            catch(ConflictException) //Test user was not cleaned up. Instead of failing, try deleting it and try again.
            {
                using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    await _usersCosmosWrapper.DeleteItemAsync(new UserWithId(testUser), cancellationSource.Token);
                }

                using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    createUserResult = await _usersCosmosWrapper.CreateItemAsync(new UserWithId(testUser), cancellationSource.Token);
                }
            }

            Assert.Equal(testUser.Email, createUserResult.Email);
            Assert.Equal(testUser.Organization, createUserResult.Organization);
            Assert.Equal(testUser.Name, createUserResult.Name);


            // Get User
            UserWithId? getUserResult;
            using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                getUserResult = await _usersCosmosWrapper.GetItemAsync(new UserWithId(testUser), cancellationSource.Token);
            }

            Assert.NotNull(getUserResult);
            Assert.Equal(testUser.Email, getUserResult.Email);
            Assert.Equal(testUser.Organization, getUserResult.Organization);
            Assert.Equal(testUser.Name, getUserResult.Name);


            // Update User Soft
            var updatedUser = new User()
            {
                Email = testUser.Email,
                Organization = testUser.Organization,
                Name = "Tester Mcgee Jr."
            };

            UserWithId? updatedUserResult;
            using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                updatedUserResult = await _usersCosmosWrapper.UpdateItemAsync(new UserWithId(updatedUser), false, cancellationSource.Token);
            }

            Assert.NotNull(updatedUserResult);

            using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                getUserResult = await _usersCosmosWrapper.GetItemAsync(new UserWithId(testUser), cancellationSource.Token);
            }

            Assert.NotNull(getUserResult);
            Assert.Equal(updatedUserResult.Email, testUser.Email);
            Assert.Equal(updatedUserResult.Organization, testUser.Organization);

            Assert.Equal(updatedUserResult.ProfilePicture, testUser.ProfilePicture); //Soft update SHOULD NOT have overwritten this

            Assert.NotEqual(updatedUserResult.Name, testUser.Name);
            Assert.Equal(updatedUserResult.Name, updatedUser.Name);


            // Update User Hard
            using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                updatedUserResult = await _usersCosmosWrapper.UpdateItemAsync(new UserWithId(updatedUser), true, cancellationSource.Token);
            }

            Assert.NotNull(updatedUserResult);

            using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                getUserResult = await _usersCosmosWrapper.GetItemAsync(new UserWithId(testUser), cancellationSource.Token);
            }

            Assert.NotNull(getUserResult);
            Assert.Equal(updatedUserResult.Email, testUser.Email);
            Assert.Equal(updatedUserResult.Organization, testUser.Organization);

            Assert.NotEqual(updatedUserResult.ProfilePicture, testUser.ProfilePicture); //Hard update SHOULD have overwritten this
            Assert.Null(getUserResult.ProfilePicture);

            Assert.NotEqual(updatedUserResult.Name, testUser.Name);
            Assert.Equal(updatedUserResult.Name, updatedUser.Name);


            // Delete User
            using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
            {
                await _usersCosmosWrapper.DeleteItemAsync(new UserWithId(testUser), cancellationSource.Token);
            }

            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                getUserResult = await _usersCosmosWrapper.GetItemAsync(new UserWithId(testUser), cancellationSource.Token);
            }
            catch(Exception e)
            {
                Assert.IsType<NotFoundException>(e);
            }
        }
    }
}