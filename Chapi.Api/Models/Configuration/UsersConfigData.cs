namespace Chapi.Api.Models.Configuration
{
    public class CrudConfigDataDto<T> : IConfigData<CrudConfigData<T>>
    {
        public string? DatabaseName { get; set; }
        public string? ContainerName { get; set; }

        public CrudConfigData<T> ToValidated()
        {
            if(string.IsNullOrEmpty(DatabaseName)) throw new ArgumentNullException(nameof(DatabaseName));
            if(string.IsNullOrEmpty(ContainerName)) throw new ArgumentNullException(nameof(ContainerName));

            return new CrudConfigData<T>(DatabaseName, ContainerName);
        }

        public static void AddSingleton(WebApplicationBuilder builder, string configKey)
        {
            var configData = builder.Configuration.GetSection(configKey).Get<CrudConfigDataDto<T>>();
            if (configData == null) throw new InvalidOperationException($"configKey data is missing or invalid.");
            builder.Services.AddSingleton(configData.ToValidated());
        }
    }
    public record CrudConfigData<T>(string DatabaseName, string ContainerName);
}
