using Chapi.Api.Models.Exceptions.Common;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System.Reflection;

namespace Chapi.Api.Models
{
    public interface IDatabaseItem
    {
        string GetId();
        string? GetPartitionKey();
    }

    public abstract class DatabaseItem : IDatabaseItem
    {
        public DatabaseItem() { }
        public abstract string? GetPartitionKey();
        public string GetId() => MapToId() ?? throw new BadRequestException();
        protected abstract string? MapToId();
    }

    public interface IDatabaseItemWithId : IDatabaseItem
    {
        [JsonProperty(PropertyName = "id")] // Can probably get rid of this, but unsure
        public string? Id { get; set; }
    }
}
