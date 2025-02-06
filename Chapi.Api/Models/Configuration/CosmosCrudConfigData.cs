namespace Chapi.Api.Models.Configuration
{
    public class CosmosCrudConfigDataDto : IConfigData<CosmosCrudConfigData>
    {
        public string? DatabaseName { get; set; }
        public string? ContainerName { get; set; }

        public CosmosCrudConfigData ToValidated()
        {
            if (string.IsNullOrEmpty(DatabaseName)) throw new ArgumentNullException(nameof(DatabaseName));
            if (string.IsNullOrEmpty(ContainerName)) throw new ArgumentNullException(nameof(ContainerName));

            return new CosmosCrudConfigData(DatabaseName, ContainerName);
        }

    }
    public record CosmosCrudConfigData(string DatabaseName, string ContainerName);
}
