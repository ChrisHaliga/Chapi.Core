namespace Chapi.Api.Services.CrudServices
{
    public static class CrudServiceBroker
    {
        private static UserService? _userService;
        private static GroupService? _groupService;
        private static ApplicationService? _applicationService;

        public static UserService? UserService { get => _userService; set { _userService ??= value; } }
        public static GroupService? GroupService { get => _groupService; set { _groupService ??= value; } }
        public static ApplicationService? ApplicationService { get => _applicationService; set { _applicationService ??= value; } }
    }
}
