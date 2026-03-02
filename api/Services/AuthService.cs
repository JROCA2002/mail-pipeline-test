using Azure.Core;
using Azure.Identity;
using EnvioMail.Options;
using EnvioMail.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace EnvioMail.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly GraphMailOptions _graph_options;
        public AuthService(IConfiguration configuration, IOptions<GraphMailOptions> graphOptions)
        {
            _configuration = configuration;
            _graph_options = graphOptions.Value;
        }

        public TokenCredential GetTokenCredential()
        {
            string provider = (_configuration["MailProvider"] ?? "SMTP").Trim().ToUpperInvariant();

            bool useUserAssigned = false; // Change to true to use a user-assigned identity

            ManagedIdentityId identityId = useUserAssigned
                ? ManagedIdentityId.FromUserAssignedObjectId("YOUR-USER-ASSIGNED-CLIENT-ID")
                : ManagedIdentityId.SystemAssigned;

            TokenCredential credential =
             provider == "GRAPH_MI"
                 ? new ManagedIdentityCredential(identityId)
                 : new ClientSecretCredential(
                     _graph_options.TenantId,
                     _graph_options.ClientId,
                     _graph_options.ClientSecret);

            var graphClient = new GraphServiceClient(credential,new[] { "https://graph.microsoft.com/.default" });

            return credential;
        }
    }
}
