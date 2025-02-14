using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities.Extensions;

namespace Chapi.Api.Services
{
    sealed partial class ChapiService : IChapiService
    {
        public async Task<GroupWithId> GetGroup(GroupWithId group, CancellationToken cancellationToken = default) =>
            await _groupService.GetItem(group, cancellationToken);
        public async Task<List<GroupWithId>> GetGroupByOrganization(string organization, CancellationToken cancellationToken = default) =>
            await _groupService.GetItemsWhereKeyIsValue(nameof(Group.Organization), organization, cancellationToken);
        public async Task<List<GroupWithId>> GetAllGroups(GroupWithId group, CancellationToken cancellationToken = default) =>
            await _groupService.GetAllItems(cancellationToken);

        public async Task<ChapiData> ValidateGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            var existingParent = await _groupService.GetItemIfExists(new GroupWithId(group.Organization), cancellationToken);

            if (existingParent == null)
            {
                throw new BadRequestException(group, $"Parent \"{group.Organization}\" does not exist");
            }

            result.Groups.Add(existingParent.GetId(), existingParent);

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

                foreach (var role in applicationAccess.Roles)
                {
                    if (existingApplication.Roles.Find(x => x.Name == role) == null)
                    {
                        throw new BadRequestException($"Role \"{role}\" for Application \"{applicationAccess.Name}\" does not exist");
                    }
                }

                result.Applications.Add(existingApplication.GetId(), existingApplication);
            }

            return result;
        }

        private async Task UpdateApplicationsRelatedItems(ApplicationWithId? before, ApplicationWithId? after, ChapiData? validatedData, CancellationToken cancellationToken = default)
        {
            TODO
        }

        public async Task<GroupWithId> CreateGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateGroup(group, cancellationToken);

            foreach (var user in validatedData.Users)
            {
                user.Groups.AddIfNotExists(group.GetId());
                await _userService.PatchItem(user, cancellationToken);
            }

            foreach (var application in validatedData.Applications)
            {
                application.Groups[group.GetId()] = true;
                await _applicationService.PatchItem(application, cancellationToken);
            }

            return await _groupService.CreateItem(group, cancellationToken);
        }

        public async Task<GroupWithId> PatchGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            var existingGroup = await _groupService.GetItem(group, cancellationToken);
            existingGroup.SoftOverwrite(group);

            return await _groupService.UpdateItem(existingGroup, hard:false, cancellationToken);
        }

        public async Task<GroupWithId> PutGroup(GroupWithId group, CancellationToken cancellationToken = default)
        {
            return await _groupService.UpdateItem(group, hard: true, cancellationToken);
        }

        public async Task DeleteGroup(string groupId, bool deleteChildren = false, CancellationToken cancellationToken = default)
        {
            var group = await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);

            deleteChildren = deleteChildren || group.Organization == Group.RootOrganization || string.IsNullOrEmpty(group.Organization);

            if (group.Organization == Group.RootOrganization || string.IsNullOrEmpty(group.Organization))
            {
                foreach (var organizationUser in await _userService.GetItemsWhereKeyIsValue(nameof(User.Organization), group.GetId(), cancellationToken))
                {
                    await DeleteUser(organizationUser, cancellationToken);
                }

                foreach (var childId in group.Children)
                {
                    await DeleteGroup(childId, deleteChildren, cancellationToken);
                }
            }
            else
            {
                foreach(var childGroup in group.Children)
                {
                    await _groupService.GetItemById(groupId, cancellationToken);
                    await MigrateGroup(childGroup, group.Organization, cancellationToken);
                }
            }

            

            foreach (var user in group.Members)
            {
                var existingUser = await _userService.GetItemIfExists(new UserWithId(user.Key), cancellationToken);
                if (existingUser != null)
                {
                    existingUser.Groups.Remove(group.GetId());
                    await _userService.UpdateItem(existingUser, hard: true, cancellationToken);
                }
            }

            foreach (var application in group.Applications)
            {
                var existingApplication = await _applicationService.GetItemIfExists(new ApplicationWithId(application.Name), cancellationToken);
                if (existingApplication != null)
                {
                    existingApplication.Groups.Remove(group.GetId());
                    await _applicationService.UpdateItem(existingApplication, hard:true, cancellationToken);
                }
            }

            await _groupService.DeleteItem(group, cancellationToken);
        }

        public async Task<GroupWithId> MigrateGroup(GroupWithId item, string newPartition, CancellationToken cancellationToken = default)
        {
            item.Organization = newPartition;

            await ValidateGroup(item, cancellationToken);
            await _groupService.DeleteItem(item, cancellationToken);
            return await _groupService.CreateItem(item, cancellationToken);
        }
    }
}
