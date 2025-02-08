namespace Chapi.Api.Models.Configuration
{
    public class RuntimeInfo
    {
        public bool IsDevelopment { get; }

        public RuntimeInfo(bool isDevelopment)
        {
            IsDevelopment = isDevelopment;
        }
    }
}
