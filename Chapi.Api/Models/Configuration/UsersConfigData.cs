using System.Diagnostics.CodeAnalysis;

namespace Chapi.Api.Models.Configuration
{
    public class UsersConfigDataDto : IConfigData<UsersConfigData>
    {
        public string? DatabaseName { get; set; }
        public string? ContainerName { get; set; }

        public UsersConfigData ToValidated()
        {
            if(string.IsNullOrEmpty(DatabaseName)) throw new ArgumentNullException(nameof(DatabaseName));
            if(string.IsNullOrEmpty(ContainerName)) throw new ArgumentNullException(nameof(ContainerName));

            return new UsersConfigData(DatabaseName, ContainerName);
        }
    }
    public record UsersConfigData(string DatabaseName, string ContainerName);
}
