using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities.Extensions;

namespace Chapi.Api.Services
{
    sealed partial class ChapiService : IChapiService
    {
        public async Task<ApplicationWithId> GetApplication(ApplicationWithId application, CancellationToken cancellationToken = default) =>
            await _applicationService.GetItem(application, cancellationToken);

        public async Task<List<ApplicationWithId>> GetApplicationByPlatform(string platform, CancellationToken cancellationToken = default) =>
            await _applicationService.GetItemsWhereKeyIsValue(nameof(Application.Platform), platform, cancellationToken);

        public async Task<List<ApplicationWithId>> GetAllApplications(ApplicationWithId application, CancellationToken cancellationToken = default) =>
            await _applicationService.GetAllItems(cancellationToken);

        public async Task<ChapiData> ValidateApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            foreach (var user in application.Users)
            {
                var existingUser = await _userService.GetItemIfExists(new UserWithId(user.Key), cancellationToken);

                if (existingUser == null)
                {
                    throw new BadRequestException(application, $"User \"{user.Key}\" does not exist");
                }

                result.Users.Add(existingUser.GetId(), existingUser);
            }

            foreach (var group in application.Groups)
            {
                var existingGroup = await _groupService.GetItemIfExists(new GroupWithId(group.Key), cancellationToken);

                if(existingGroup == null)
                {
                    throw new BadRequestException(application, $"Group \"{group.Key}\" does not exist");
                }

                result.Groups.Add(existingGroup.GetId(), existingGroup);
            }

            return result;
        }

        private async Task UpdateApplicationsRelatedItems(ApplicationWithId? before, ApplicationWithId? after, ChapiData? validatedData, CancellationToken cancellationToken = default)
        {
            TODO
        }


        public async Task<ApplicationWithId> UpdateApplication(ApplicationWithId application, bool hard = false, CancellationToken cancellationToken = default)
        {
            var updateItem = application;
            if (!hard)
            {
                var existingApplication = await _applicationService.GetItem(application, cancellationToken);
                existingApplication.SoftOverwrite(application);
                updateItem = existingApplication;
            }

            await ValidateApplication(updateItem);

            return await _applicationService.UpdateItem(updateItem, hard, cancellationToken);
        }

        public async Task<ApplicationWithId> CreateApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateApplication(application, cancellationToken);

            var applicationAccess = new ApplicationAccess() { Name = application.GetId() };

            foreach (var user in validatedData.Users)
            {
                user.Applications.AddIfNotExists(applicationAccess, x=> x.Name == applicationAccess.Name);
                await _userService.PatchItem(user, cancellationToken);
            }

            foreach (var group in validatedData.Groups)
            {
                group.Applications.AddIfNotExists(applicationAccess, x => x.Name == applicationAccess.Name);
                await _groupService.PatchItem(group, cancellationToken);
            }

            return await _applicationService.CreateItem(application, cancellationToken);
        }

        public async Task DeleteApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            foreach (var user in application.Users)
            {
                var existingUser = await _userService.GetItemIfExists(new UserWithId(user.Key), cancellationToken);
                if (existingUser != null)
                {
                    existingUser.Applications.Remove(x => x.Name == application.GetId());
                }
            }

            foreach (var groupName in application.Groups)
            {
                var existingGroup = await _groupService.GetItemIfExists(new GroupWithId(groupName.Key), cancellationToken);
                if (existingGroup != null)
                {
                    existingGroup.Applications.Remove(x => x.Name == application.GetId());
                }
            }
        }

        public async Task<ApplicationWithId> MigrateApplication(ApplicationWithId application, string newPartition, CancellationToken cancellationToken = default)
        {
            application.Platform = newPartition;

            await ValidateApplication(application, cancellationToken);
            await _applicationService.DeleteItem(application, cancellationToken);
            return await _applicationService.CreateItem(application, cancellationToken);
        }
    }
}
