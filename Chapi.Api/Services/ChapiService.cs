using Chapi.Api.Models;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Services.CrudServices;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.Azure.Cosmos;
using System.Threading;

namespace Chapi.Api.Services
{
    sealed partial class ChapiService
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
            public List<UserWithId> Users { get; set; } = new List<UserWithId>();
            public List<GroupWithId> Groups { get; } = new List<GroupWithId>();
            public List<ApplicationWithId> Applications { get; } = new List<ApplicationWithId>();
        }

        private void ValidateApplicationAccess(ApplicationAccess applicationAccess, ApplicationWithId applicationWithId)
        {
            foreach (var role in applicationAccess.Roles)
            {
                if (applicationWithId.Roles.Find(x => x.Name == role) == null)
                {
                    throw new BadRequestException($"Role \"{role}\" for Application \"{applicationAccess.Name}\" does not exist");
                }
            }
        }
    }
}
