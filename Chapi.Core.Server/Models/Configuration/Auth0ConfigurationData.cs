using Chapi.Api.Models.Configuration;

namespace Chapi.Core.Server.Models.Configuration
{
    internal class Auth0ConfigDataDto : IConfigData<Auth0ConfigData>
    {
        public string? Domain { get; set; }
        public string? Audience { get; set; }
        public string? ClientId { get; set; }

        public Auth0ConfigData ToValidated()
        {
            if (string.IsNullOrEmpty(Domain)) throw new ArgumentNullException(nameof(Domain));
            if (string.IsNullOrEmpty(Audience)) throw new ArgumentNullException(nameof(Audience));
            if (string.IsNullOrEmpty(ClientId)) throw new ArgumentNullException(nameof(ClientId));


            return new Auth0ConfigData(Domain, Audience, ClientId);
        }
    }

    public record Auth0ConfigData(string Domain, string Audience, string ClientId);

}
