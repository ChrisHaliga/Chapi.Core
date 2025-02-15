using Chapi.Api.Models.Exceptions;
using Chapi.Api.Models.Exceptions.Common;
using Chapi.Api.Utilities;
using Newtonsoft.Json;

namespace Chapi.Api.Models
{
    public interface IDatabaseItem
    {
        string? GetIdFieldName();
        string GetId();
        string? GetPartitionKey();
        string GetPartitionKeyFieldName();
    }

    public class DatabaseItemBuildValidator: IBuildValidator
    {
        public void Validate()
        {
            var childClasses = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(DatabaseItem)) && !t.IsAbstract);

            foreach (var type in childClasses)
            {
                var hasParameterlessConstructor = type.GetConstructor(Type.EmptyTypes) != null;
                if (!hasParameterlessConstructor)
                {
                    throw new BuildValidationException(
                        type.FullName!,
                        new InvalidOperationException($"Class {type.Name} must have a parameterless constructor.")
                    );
                }
            }
        }
    }

    public abstract class DatabaseItem : IDatabaseItem
    {
        public static string TemporaryIdSuffix = "_temporary";
        public abstract string? GetPartitionKey();
        public abstract string GetPartitionKeyFieldName();
        public string GetId() => MapToId() ?? throw new BadRequestException();
        protected abstract string? MapToId();
        public abstract string GetIdFieldName();
    }

    public interface IDatabaseItemWithId : IDatabaseItem
    {
        [JsonProperty(PropertyName = "id")] // Can probably get rid of this, but unsure
        public string? Id { get; set; }
    }
}
