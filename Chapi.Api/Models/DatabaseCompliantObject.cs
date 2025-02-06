using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Chapi.Api.Models
{
    public abstract class DatabaseCompliantObject<Dto> where Dto : DatabaseDto
    {
        [JsonProperty(PropertyName = "id")]
        public string? Id { get; set; }

        [JsonIgnore]
        public PartitionKey PartitionKey { get; set; }

        protected DatabaseCompliantObject(){}
        protected DatabaseCompliantObject(Dto databaseDto)
        {
            Id = databaseDto.Id();

            var partitionKey = databaseDto.PartionKey();
            PartitionKey = string.IsNullOrEmpty(partitionKey) ? PartitionKey.None : new PartitionKey(partitionKey);
        }

        public abstract DatabaseDto ToDatabaseDto();

        public string CacheKey(string databaseName, string containerName) => $"{databaseName}-{containerName}-{Id}";

    }

    public abstract class DatabaseDto
    {
        public abstract string Id();
        public abstract string? PartionKey();
    }
}
