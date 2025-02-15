using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities.Extensions;
using Microsoft.Azure.Cosmos;
using System.Linq;

namespace Chapi.Api.Services
{
    public sealed partial class ChapiService : IChapiServiceApplication
    {
        public async Task<ApplicationWithId> GetApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            return await _applicationService.GetItem(application, cancellationToken);
        }

        public async Task<List<ApplicationWithId>> GetApplicationsByPlatform(string organization, CancellationToken cancellationToken = default)
        {
            return await _applicationService.GetItemsWhereKeyIsValue(nameof(ApplicationWithId.Platform), organization, cancellationToken);
        }

        public async Task<List<ApplicationWithId>> GetAllApplications(CancellationToken cancellationToken = default)
        {
            return await _applicationService.GetAllItems(cancellationToken);
        }

        public async Task<ApplicationWithId> CreateApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var validatedData = await ValidateApplication(application, cancellationToken);

            await UpdateApplicationsRelatedItems(null, application, validatedData, cancellationToken);

            return await _applicationService.CreateItem(application, cancellationToken);
        }

        public async Task<ApplicationWithId> PatchApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var existingApplication = await _applicationService.GetItem(application, cancellationToken);
            var newApplication = new ApplicationWithId(existingApplication);
            newApplication.SoftOverwrite(application);

            var validatedData = await ValidateApplication(newApplication, cancellationToken);

            await UpdateApplicationsRelatedItems(existingApplication, newApplication, validatedData, cancellationToken);

            return await _applicationService.UpdateItem(newApplication, hard: false, cancellationToken);
        }

        public async Task<ApplicationWithId> PutApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var existingApplication = await _applicationService.GetItem(application);
            var validatedData = await ValidateApplication(application, cancellationToken);

            await UpdateApplicationsRelatedItems(existingApplication, application, validatedData, cancellationToken);

            return await _applicationService.UpdateItem(application, hard: true, cancellationToken);
        }

        public async Task DeleteApplication(ApplicationWithId applicationQuery, CancellationToken cancellationToken = default)
        {
            var application = await _applicationService.GetItem(applicationQuery, cancellationToken);
            await UpdateApplicationsRelatedItems(application, null, null, cancellationToken);
            await _applicationService.DeleteItem(application, cancellationToken);
        }

        public async Task<ChapiData> ValidateApplication(ApplicationWithId application, CancellationToken cancellationToken = default)
        {
            var result = new ChapiData();

            await EnsureUsersExist(application, application.Users, result, cancellationToken);

            await EnsureGroupsExist(application, application.GroupsIds, result, cancellationToken);

            return result;
        }

        private async Task UpdateApplicationsRelatedItems(ApplicationWithId? before, ApplicationWithId? after, ChapiData? validatedData, CancellationToken cancellationToken = default)
        {
            string applicationId = before?.GetId() ?? after?.GetId() ?? throw new BadRequestException("Updating null to null???");

            await AddItemToUsers(validatedData, after?.Users, applicationId, AddApplicationToUser, cancellationToken);
            await RemoveItemFromUsersBeforeChange(validatedData, before?.Users, after?.Users, applicationId, RemoveApplicationFromUser, cancellationToken);

            await AddItemToGroups(validatedData, after?.GroupsIds, applicationId, AddApplicationToGroup, cancellationToken);
            await RemoveItemFromGroupsBeforeChange(validatedData, before?.GroupsIds, after?.GroupsIds, applicationId, RemoveApplicationFromGroup, cancellationToken);
        }

        public async Task<ApplicationWithId> MigrateApplication(ApplicationWithId application, string newPartition, CancellationToken cancellationToken = default)
        {
            application.Platform = newPartition;

            await ValidateApplication(application, cancellationToken);
            await _applicationService.DeleteItem(application, cancellationToken);
            return await _applicationService.CreateItem(application, cancellationToken);
        }

        private bool AddApplicationToUser(UserWithId user, string applicationId)
        {
            var indexOfApp = user.Applications.FindIndex(x => x.Name == applicationId);

            if (indexOfApp == -1)
            {
                user.Applications.Add(new ApplicationAccess() { Name = applicationId });
                return true;
            }
            return false;
        }

        private bool RemoveApplicationFromUser(UserWithId user, string applicationId)
        {
            var indexOfApp = user.Applications.FindIndex(x => x.Name == applicationId);

            if (indexOfApp != -1)
            {
                user.Applications.RemoveAt(indexOfApp);
                return true;
            }
            return false;
        }


        private bool AddApplicationToGroup(GroupWithId group, string applicationId)
        {
            var indexOfApp = group.Applications.FindIndex(x => x.Name == applicationId);

            if (indexOfApp == -1)
            {
                group.Applications.Add(new ApplicationAccess() { Name = applicationId });
                return true;
            }
            return false;
        }


        private bool RemoveApplicationFromGroup(GroupWithId group, string applicationId)
        {
            var indexOfApp = group.Applications.FindIndex(x => x.Name == applicationId);

            if (indexOfApp != -1)
            {
                group.Applications.RemoveAt(indexOfApp);
                return true;
            }
            return false;
        }

    }
}
