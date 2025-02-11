using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Microsoft.Azure.Cosmos;

namespace Chapi.Api.Services
{
    public partial class ChapiService
    {
        public async Task<ChapiData> ValidateGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            foreach (var user in group.Members)
            {
                var existingUser = await _userService.GetItemIfExists(new UserWithId(user.Key), cancellationToken);

                if (existingUser == null)
                {
                    throw new BadRequestException(group, $"User \"{user.Key}\" does not exist");
                }

                result.Users.Add(existingUser);
            }

            foreach (var applicationAccess in group.Applications)
            {
                var existingApplication = await _applicationService.GetItemIfExists(new ApplicationWithId(applicationAccess.Name), cancellationToken);

                if (existingApplication == null)
                {
                    throw new BadRequestException(group, $"Application \"{applicationAccess.Name}\" does not exist");
                }

                ValidateApplicationAccess(applicationAccess,existingApplication);

                result.Applications.Add(existingApplication);
            }

            return result;
        }

        public async Task<GroupWithId> CreateGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateGroup(group, cancellationToken);

            foreach (var user in validatedData.Users)
            {
                user.AddGroup(group.GetId());
                await _userService.PatchItem(user, cancellationToken);
            }

            foreach (var application in validatedData.Applications)
            {
                application.AddGroup(group.GetId());
                await _applicationService.PatchItem(application, cancellationToken);
            }

            return await _groupService.CreateItem(group, cancellationToken);
        }

        public async Task DeleteGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            foreach (var user in group.Members)
            {
                var existingUser = await _userService.GetItemIfExists(new UserWithId(user.Key), cancellationToken);
                existingUser?.RemoveGroup(group.GetId());
            }

            foreach (var application in group.Applications)
            {
                var existingApplication = await _applicationService.GetItemIfExists(new ApplicationWithId(application.Name), cancellationToken);
                existingApplication?.RemoveGroup(group.GetId());
            }

            await _groupService.DeleteItem(group, cancellationToken);
        }
    }
}
