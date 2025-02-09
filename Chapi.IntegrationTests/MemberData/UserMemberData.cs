using Chapi.Api.Controllers;
using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Microsoft.AspNetCore.Mvc;
using static Chapi.Api.Models.User;

namespace Chapi.IntegrationTests.MemberData
{
    public static class UserMemberData
    {
        public static User DefaultTestUser => new User()
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

        private static User _defaultTestUser = DefaultTestUser;

        public static async Task SetupAsync(UsersController controller)
        {
            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                await controller.Post(DefaultTestUser, cancellationSource.Token);
            }
            catch (ConflictException){}
        }

        public static async Task CleanupAsync(UsersController controller)
        {
            try
            {
                using var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
                await controller.Delete(
                    new UserMinimalDto { Email = _defaultTestUser.Email, Organization = _defaultTestUser.Organization },
                    cancellationSource.Token);
            }
            catch (NotFoundException){}
        }

        internal static IEnumerable<object[]> PostTestData()
        {
            yield return new object[] {
                DefaultTestUser,
                typeof(OkObjectResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _defaultTestUser.Email,
                    Organization = _defaultTestUser.Organization,
                },
                typeof(OkObjectResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = string.Empty,
                    Organization = _defaultTestUser.Organization,
                    Name = _defaultTestUser.Name,
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = null,
                    Organization = _defaultTestUser.Organization,
                    Name = _defaultTestUser.Name,
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _defaultTestUser.Email,
                    Organization = string.Empty,
                    Name = _defaultTestUser.Name,
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
                },
                typeof(BadRequestResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = _defaultTestUser.Email,
                    Organization = null,
                    Name = _defaultTestUser.Name,
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
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
                    Email = _defaultTestUser.Email,
                    Organization = "This Organization Does Not Exist",
                    Name = _defaultTestUser.Name,
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
                    },
                typeof(NotFoundResult)
            };
        }

        internal static IEnumerable<object?[]> GetUserTestData()
        {
            yield return new object?[] {
                _defaultTestUser.Email,
                _defaultTestUser.Organization,
                typeof(OkObjectResult)
            };
            yield return new object?[] {
                _defaultTestUser.Email,
                null,
                typeof(OkObjectResult)
            };
            yield return new object?[] {
                null,
                _defaultTestUser.Organization,
                typeof(OkObjectResult)
            };
            yield return new object?[] {
                null,
                null,
                typeof(OkObjectResult)
            };
            yield return new object?[] {
                "This Email Does Not Exist",
                null,
                typeof(NotFoundResult)
            };
            yield return new object?[] {
                null,
                "This Organization Does Not Exist",
                typeof(NotFoundResult)
            };
            yield return new object?[] {
                "This Email Does Not Exist",
                "This Organization Does Not Exist",
                typeof(NotFoundResult)
            };
        }

        internal static IEnumerable<object[]> PutTestData()
        {
            yield return new object[] {
                DefaultTestUser,
                typeof(OkObjectResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = "NonExistentUser@example.com",
                    Organization = _defaultTestUser.Organization,
                    Name = "Updated Name",
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
                },
                typeof(NotFoundResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = string.Empty,
                    Organization = _defaultTestUser.Organization,
                    Name = _defaultTestUser.Name,
                    ProfilePicture = _defaultTestUser.ProfilePicture,
                    Access = _defaultTestUser.Access
                },
                typeof(BadRequestResult)
            };
        }

        internal static IEnumerable<object[]> PatchTestData()
        {
            yield return new object[] {
                DefaultTestUser,
                typeof(OkObjectResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = "NonExistentUser@example.com",
                    Organization = _defaultTestUser.Organization,
                    Name = "Partially Updated Name"
                },
                typeof(NotFoundResult)
            };
            yield return new object[] {
                new User()
                {
                    Email = string.Empty,
                    Organization = _defaultTestUser.Organization,
                    Name = _defaultTestUser.Name
                },
                typeof(BadRequestResult)
            };
        }

        internal static IEnumerable<object[]> DeleteTestData()
        {
            yield return new object[] {
                new UserMinimalDto()
                {
                    Email = _defaultTestUser.Email,
                    Organization = _defaultTestUser.Organization
                },
                typeof(OkResult)
            };
            yield return new object[] {
                new UserMinimalDto()
                {
                    Email = "NonExistentUser@example.com",
                    Organization = _defaultTestUser.Organization
                },
                typeof(NotFoundResult)
            };
            yield return new object[] {
                new UserMinimalDto()
                {
                    Email = string.Empty,
                    Organization = _defaultTestUser.Organization
                },
                typeof(BadRequestResult)
            };
        }
    }
}
