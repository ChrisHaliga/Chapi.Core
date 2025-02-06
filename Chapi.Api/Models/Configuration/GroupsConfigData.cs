namespace Chapi.Api.Models.Configuration
{
    public class GroupsConfigDataDto : IConfigData<GroupsConfigData>
    {
        public string? DatabaseName { get; set; }
        public string? ContainerName { get; set; }

        public GroupsConfigData ToValidated()
        {
            if(string.IsNullOrEmpty(DatabaseName)) throw new ArgumentNullException(nameof(DatabaseName));
            if(string.IsNullOrEmpty(ContainerName)) throw new ArgumentNullException(nameof(ContainerName));

            return new GroupsConfigData(DatabaseName, ContainerName);
        }
    }
    public record GroupsConfigData(string DatabaseName, string ContainerName);
}
