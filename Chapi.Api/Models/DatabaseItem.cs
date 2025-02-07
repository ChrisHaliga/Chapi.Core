using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace Chapi.Api.Models
{
    public abstract class DatabaseItem<T> where T : DatabaseItemWithId
    {
        public DatabaseItem(){}
        internal abstract string? GetPartitionKeyString();
        public abstract string GetId();

        public PartitionKey GetPartitionKey()
        {
            var partitionKey = GetPartitionKeyString();
            return new PartitionKey(partitionKey);
        }
        public string GetCacheKey(string databaseName, string containerName) => $"{databaseName}-{containerName}-{GetId()}";

        public T ToCosmosItemWithId()
        {
            var target = CreateInstance();
            InjectValues(this, target);
            target.Id = GetId();
            return target;
        }

        public void FromCosmosItemWithId(T source)
        {
            InjectValues(source, this);
        }

        internal abstract T CreateInstance();

        internal static void InjectValues<T1, T2>(T1 from, T2 to, bool hard = false)
        {
            if(from == null || to == null) return;

            var properties = from.GetType().GetProperties();

            foreach (var property in properties)
            {
                var value = property.GetValue(from);
                if (hard || value != null)
                {
                    to.GetType().GetProperty(property.Name)?.SetValue(to, value);
                }
            }
        }
    }

    public interface DatabaseItemWithId
    {
        [JsonProperty(PropertyName = "id")] // Can probably get rid of this, but unsure
        public string? Id { get; set; }
    }
}
