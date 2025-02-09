using Chapi.Api.Controllers;
using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Services;
using Chapi.Api.Services.CrudServices;
using Chapi.IntegrationTests.Fixtures;
using Chapi.IntegrationTests.MemberData;
using Chapi.IntegrationTests.Spies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static Chapi.Api.Models.User;

namespace Chapi.IntegrationTests
{
    public class ControllerTests : IClassFixture<ControllerTestsFixture>
    {
        private readonly ControllerTestsFixture _fixture;
        private IConfiguration _configuration => _fixture.Configuration;
        private UsersController _usersController => _fixture.UsersController;
        private GroupsController _groupsController => _fixture.GroupsController;
        private ICacheService _cacheSpy => _fixture.CacheSpy;

        public ControllerTests(ControllerTestsFixture fixture)
        {
            _fixture = fixture;
        }

        public static IEnumerable<object[]> PostUserTestData() => UserMemberData.PostTestData();
        public static IEnumerable<object?[]> GetUserTestData() => UserMemberData.GetUserTestData();
        public static IEnumerable<object[]> PutUserTestData() => UserMemberData.PutTestData();
        public static IEnumerable<object[]> PatchUserTestData() => UserMemberData.PatchTestData();
        public static IEnumerable<object[]> DeleteUserTestData() => UserMemberData.DeleteTestData();


        [Theory]
        [MemberData(nameof(PostUserTestData))]
        public async Task UserCreateTests(User user, Type expectedResult)
        {
            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var result = await _usersController.Post(user, cancellationSource.Token);
                Assert.IsType(expectedResult, result);
            }
            finally
            {
                await UserMemberData.CleanupAsync(_usersController);
            }
        }

        [Theory]
        [MemberData(nameof(GetUserTestData))]
        public async Task UserGetTests(string? email, string? organization, Type expectedResult)
        {
            await UserMemberData.SetupAsync(_usersController);

            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var result = await _usersController.Get(email, organization, cancellationSource.Token);
                Assert.IsType(expectedResult, result);
            }
            finally
            {
                await UserMemberData.CleanupAsync(_usersController);
            }
        }

        [Theory]
        [MemberData(nameof(PutUserTestData))]
        public async Task UserPutTests(User user, Type expectedResult)
        {
            await UserMemberData.SetupAsync(_usersController);

            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var result = await _usersController.Put(user, cancellationSource.Token);
                Assert.IsType(expectedResult, result);
            }
            finally
            {
                await UserMemberData.CleanupAsync(_usersController);
            }
        }

        [Theory]
        [MemberData(nameof(PatchUserTestData))]
        public async Task UserPatchTests(User user, Type expectedResult)
        {
            await UserMemberData.SetupAsync(_usersController);

            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var result = await _usersController.Patch(user, cancellationSource.Token);
                Assert.IsType(expectedResult, result);
            }
            finally
            {
                await UserMemberData.CleanupAsync(_usersController);
            }
        }

        [Theory]
        [MemberData(nameof(DeleteUserTestData))]
        public async Task UserDeleteTests(UserMinimalDto userMinimal, Type expectedResult)
        {
            await UserMemberData.SetupAsync(_usersController);

            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var result = await _usersController.Delete(userMinimal, cancellationSource.Token);
                Assert.IsType(expectedResult, result);
            }
            finally
            {
                await UserMemberData.CleanupAsync(_usersController);
            }
        }
    }
}
