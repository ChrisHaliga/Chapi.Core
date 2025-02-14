using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.CrudServices;
using Swashbuckle.AspNetCore.SwaggerGen;
using static Chapi.Api.Services.ChapiService;

namespace Chapi.Api.Services
{
    interface IChapiService
    {
        Task<UserWithId> GetUser(UserWithId group, CancellationToken cancellationToken = default);
        Task<List<UserWithId>> GetUsersByOrganization(string parent, CancellationToken cancellationToken = default);
        Task<List<UserWithId>> GetAllUsers(UserWithId group, CancellationToken cancellationToken = default);
        Task<ChapiData> ValidateUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> CreateUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> PatchUser(UserWithId group, CancellationToken cancellationToken = default);
        Task<UserWithId> PutUser(UserWithId group, CancellationToken cancellationToken = default);
        Task DeleteUser(UserWithId user, CancellationToken cancellationToken = default);
        Task<UserWithId> MigrateUser(UserWithId item, string newPartition, CancellationToken cancellationToken = default);


        Task<GroupWithId> GetGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<List<GroupWithId>> GetGroupByOrganization(string parent, CancellationToken cancellationToken = default);
        Task<List<GroupWithId>> GetAllGroups(GroupWithId group, CancellationToken cancellationToken = default);
        Task<ChapiData> ValidateGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> CreateGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> PatchGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> PutGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task DeleteGroup(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> MigrateGroup(GroupWithId item, string newPartition, CancellationToken cancellationToken = default);


        Task<ApplicationWithId> GetApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<List<ApplicationWithId>> GetApplicationByPlatform(string platform, CancellationToken cancellationToken = default);
        Task<List<ApplicationWithId>> GetAllApplications(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<ChapiService.ChapiData> ValidateApplication(ApplicationWithId application, CancellationToken cancellationToken = default);
        Task<ApplicationWithId> CreateApplication(ApplicationWithId user, CancellationToken cancellationToken = default);
        Task<GroupWithId> PatchApplication(GroupWithId group, CancellationToken cancellationToken = default);
        Task<GroupWithId> PutApplication(GroupWithId group, CancellationToken cancellationToken = default);
        Task DeleteApplication(ApplicationWithId user, CancellationToken cancellationToken = default);
        Task<ApplicationWithId> MigrateApplication(ApplicationWithId application, string newPartition, CancellationToken cancellationToken = default);
    }

    sealed partial class ChapiService : IChapiService
    {
        private readonly UserService _userService;
        private readonly GroupService _groupService;
        private readonly ApplicationService _applicationService;

        public ChapiService(UserService userService, GroupService groupService, ApplicationService applicationService)
        {
            _userService = userService;
            _groupService = groupService;
            _applicationService = applicationService;
        }

        public class ChapiData //This is mostly just to save db calls when validating before taking action
        {
            public Dictionary<string, UserWithId> Users { get; set; } = new Dictionary<string, UserWithId>();
            public Dictionary<string, GroupWithId> Groups { get; } = new Dictionary<string, GroupWithId>();
            public Dictionary<string, ApplicationWithId> Applications { get; } = new Dictionary<string, ApplicationWithId>();
        }
    }
}
