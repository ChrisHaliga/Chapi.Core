using Chapi.Api.Services.Database;

namespace Chapi.Api.Models.Configuration
{
    internal class CosmosConfigDataDto : IConfigData<CosmosConfigData>
    {
        public string? CosmosDbUri { get; set; }

        public CosmosConfigData ToValidated()
        {
            if (string.IsNullOrEmpty(CosmosDbUri)) throw new ArgumentNullException(nameof(CosmosDbUri));

            return new CosmosConfigData(CosmosDbUri);
        }
    }
    public record CosmosConfigData(string CosmosDbUri);
}
