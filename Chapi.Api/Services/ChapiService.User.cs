using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;

namespace Chapi.Api.Services
{
    public partial class ChapiService
    {
        public async Task<ChapiData> ValidateUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            foreach (var groupName in user.Groups)
            {
                var existingGroup = await _groupService.GetItemIfExists(new GroupWithId(groupName), cancellationToken);

                if (existingGroup == null)
                {
                    throw new BadRequestException(user, $"Group \"{groupName}\" does not exist");
                }

                result.Groups.Add(existingGroup);
            }

            foreach (var applicationAccess in user.Applications)
            {
                var existingApplication = await _applicationService.GetItemIfExists(new ApplicationWithId(applicationAccess.Name), cancellationToken);

                if (existingApplication == null)
                {
                    throw new BadRequestException(user, $"Application \"{applicationAccess.Name}\" does not exist");
                }

                foreach (var role in applicationAccess.Roles)
                {
                    if (existingApplication.Roles.Find(x => x.Name == role) == null)
                    {
                        throw new BadRequestException(user, $"Role \"{role}\" for Application \"{applicationAccess.Name}\" does not exist");
                    }
                }

                result.Applications.Add(existingApplication);
            }

            return result;
        }

        public async Task<UserWithId> CreateUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateUser(user, cancellationToken);

            foreach (var group in validatedData.Groups)
            {
                group.AddMember(user.GetId());
                await _groupService.PatchItem(group, cancellationToken);
            }

            return await _userService.CreateItem(user, cancellationToken);
        }

        public async Task DeleteUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            foreach (var groupName in user.Groups)
            {
                var existingGroup = await _groupService.GetItemIfExists(new GroupWithId() { Name = groupName }, cancellationToken);
                existingGroup?.RemoveMember(user.GetId());
            }

            await _userService.DeleteItem(user, cancellationToken);
        }
    }
}
