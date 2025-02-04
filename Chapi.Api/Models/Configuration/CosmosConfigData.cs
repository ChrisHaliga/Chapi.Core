using System.Diagnostics.CodeAnalysis;

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
    internal record CosmosConfigData(string CosmosDbUri);
}
