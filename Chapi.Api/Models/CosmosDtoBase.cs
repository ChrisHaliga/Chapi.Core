using Newtonsoft.Json;

namespace Chapi.Api.Models
{
    public class CosmosDtoBase
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
