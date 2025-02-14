using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;

namespace Chapi.Api.Services
{
    sealed partial class ChapiService : IChapiService
    {
        public async Task<UserWithId> GetUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            return await _userService.GetItem(user, cancellationToken);
        }

        public async Task<List<UserWithId>> GetUsersByOrganization(string organization, CancellationToken cancellationToken = default)
        {
            return await _userService.GetItemsWhereKeyIsValue(nameof(UserWithId.Organization), organization, cancellationToken);
        }

        public async Task<List<UserWithId>> GetAllUsers(UserWithId user, CancellationToken cancellationToken = default)
        {
            return await _userService.GetAllItems(cancellationToken);
        }

        public async Task<UserWithId> CreateUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateUser(user, cancellationToken);

            await UpdateUsersRelatedItems(null, user, validatedData, cancellationToken);

            return await _userService.CreateItem(user, cancellationToken);
        }

        public async Task<UserWithId> PatchUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userService.GetItem(user, cancellationToken);
            var newUser = new UserWithId(existingUser);
            newUser.SoftOverwrite(user);

            var validatedData = await ValidateUser(newUser, cancellationToken);

            await UpdateUsersRelatedItems(existingUser, newUser, validatedData, cancellationToken);

            return await _userService.UpdateItem(newUser, hard:false, cancellationToken);
        }

        public async Task<UserWithId> PutUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            var existingUser = await _userService.GetItem(user);
            var validatedData = await ValidateUser(user, cancellationToken);

            await UpdateUsersRelatedItems(existingUser, user, validatedData, cancellationToken);

            return await _userService.UpdateItem(user, hard:true, cancellationToken);
        }

        public async Task DeleteUser(UserWithId user, CancellationToken cancellationToken = default)
        {
            await UpdateUsersRelatedItems(user, null, null, cancellationToken);
            await _userService.DeleteItem(user, cancellationToken);
        }

        public async Task<UserWithId> MigrateUser(UserWithId user, string newOrganization, CancellationToken cancellationToken = default)
        {
            var testUser = new UserWithId(user);

            testUser.Email = "thisShouldNeverBeReal" + user.Email;
            testUser.Id = testUser.Email;
            testUser.Organization = newOrganization;

            await ValidateUser(testUser);

            user.Organization = newOrganization;

            await DeleteUser(user);
            return await CreateUser(user);
        }

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

                result.Groups.Add(existingGroup.GetId(), existingGroup);
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

                result.Applications.Add(existingApplication.GetId(), existingApplication);
            }

            return result;
        }

        private async Task UpdateUsersRelatedItems(UserWithId? before, UserWithId? after, ChapiData? validatedData, CancellationToken cancellationToken = default)
        {
            string userId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException("Updating null to null???");

            foreach (var groupId in after?.Groups ?? [])
            {
                var group = validatedData?.Groups[groupId] ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);
                if (!group.Members[userId])
                {
                    group.Members[userId] = true;
                    await _groupService.PatchItem(group, cancellationToken);
                }
            }

            List<string> groupsToRemove = (after == null ? before?.Groups : before?.Groups.Where(x => !after.Groups.Contains(x)).ToList()) ?? [];
            foreach (var groupId in groupsToRemove)
            {
                var group = validatedData?.Groups[groupId] ?? await _groupService.GetItem(new GroupWithId(groupId), cancellationToken);
                if (group.Members[userId])
                {
                    group.Members.Remove(userId);
                    await _groupService.PatchItem(group, cancellationToken);
                }
            }

            foreach (var applicationAccess in after?.Applications ?? [])
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications[applicationAccess.Name] ?? await _applicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);
                    if (!application.Users[userId])
                    {
                        application.Users[userId] = true;
                        await _applicationService.PatchItem(application, cancellationToken);
                    }
                }
            }

            List<ApplicationAccess> applicationsToRemove = (after == null ? before?.Applications : before?.Applications.Where(x => after.Applications.Find(y => x.Name == y.Name) == null).ToList()) ?? [];
            foreach (var applicationAccess in applicationsToRemove)
            {
                if (!string.IsNullOrEmpty(applicationAccess.Name))
                {
                    var application = validatedData?.Applications[applicationAccess.Name] ?? await _applicationService.GetItem(new ApplicationWithId(applicationAccess.Name), cancellationToken);
                    if (application.Users[userId])
                    {
                        application.Users.Remove(userId);
                        await _applicationService.PatchItem(application, cancellationToken);
                    }
                }
            }
        }
    }
}
