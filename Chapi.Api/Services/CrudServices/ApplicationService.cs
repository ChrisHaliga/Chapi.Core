using Chapi.Api.Models;
using Chapi.Api.Models.Configuration;
using Chapi.Api.Models.Exceptions.Common;
using Microsoft.Azure.Cosmos;
using System.Threading;

namespace Chapi.Api.Services.CrudServices
{
    public class ApplicationService : CrudServiceBase<ApplicationWithId>
    {
        public ApplicationService(CrudConfigData<ApplicationWithId> config, CosmosConfigData cosmosConfig, ICacheService cache, RuntimeInfo runtimeInfo) : base(config, cosmosConfig, cache, runtimeInfo)
        {
        }

#region Virtual Overrides

        public override async Task<ApplicationWithId> GetItemById(string? name, CancellationToken cancellationToken = default) =>  
            await _GetItem(new ApplicationWithId(new Application() { Name = name }), cancellationToken);

        public override async Task<List<ApplicationWithId>> GetItemsByPartitionKey(string organization, CancellationToken cancellationToken = default) => 
            await _GetItemsWhereKeyIsValue(new KeyValuePair<string, string>(nameof(Application.Platform).ToLower(), organization), cancellationToken);

        #endregion

        #region Abstract Overrides

        #endregion

        public async Task<ApplicationWithId> ValidateRolesExist(ApplicationAccess application, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(application.Name))
            {
                throw new BadRequestException($"{nameof(application.Name)} is required for {nameof(Application)}");
            }

            var existingApplication = await GetItemById(application.Name, cancellationToken);

            foreach (var role in application.Roles)
            {
                if (existingApplication.Roles.First(x => x.Name == role) == null)
                {
                    throw new BadRequestException($"{nameof(role)} is not a valid role for {application.Name}");
                }
            }

            return existingApplication;
        }

        public async Task<ApplicationWithId> AssignGroupInApplication(string id, ApplicationWithId application, CancellationToken cancellationToken = default) =>
            await ChangeGroupInApplication(id, application, true, cancellationToken);
        public async Task<ApplicationWithId> UnassignGroupInApplication(string id, ApplicationWithId application, CancellationToken cancellationToken = default) =>
            await ChangeGroupInApplication(id, application, false, cancellationToken);


        private async Task<ApplicationWithId> ChangeGroupInApplication(string id, ApplicationWithId application, bool assignment, CancellationToken cancellationToken = default)
        {
            if ((application.Groups.TryGetValue(id, out bool isMember) || isMember) != assignment)
            {
                application.Groups[id] = assignment;

                return await PatchItem(application, cancellationToken);
            }

            return application;
        }
    }
}
