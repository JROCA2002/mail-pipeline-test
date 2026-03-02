using Azure.Core;
using Azure.Identity;
using EnvioMail.Options;
using EnvioMail.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Diagnostics.Eventing.Reader;

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

            string client_id = (_configuration["Graph__ClientId"] ?? "SMTP").Trim().ToUpperInvariant();
            string rid = (_configuration["Graph__ResourceId"] ?? "SMTP").Trim().ToUpperInvariant();
            string oid = (_configuration["Graph__ObjectId"] ?? "SMTP").Trim().ToUpperInvariant();

            /*
            bool useUserAssigned = false; // Change to true to use a user-assigned identity
            var x1 = ManagedIdentityId.FromUserAssignedObjectId("YOUR-USER-ASSIGNED-CLIENT-ID")
            ManagedIdentityId identityId = useUserAssigned
                ? ManagedIdentityId.FromUserAssignedObjectId("YOUR-USER-ASSIGNED-CLIENT-ID")
                : ManagedIdentityId.SystemAssigned;
            ManagedIdentityId identityId = ManagedIdentityId.SystemAssigned;
            */

            // TODO: mejorar el if para que quede mas claro
            TokenCredential? credential = null;

            if (provider == "GRAPH_MANAGED_IDENTITY_SYSTEM_ASSIGNED"
                || provider == "GRAPH_MI")
            {

                credential = new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);

            }
            else if (provider == "GRAPH_MANAGED_IDENTITY_RESOURCE_ID")
            {

                credential = new ManagedIdentityCredential(
                        ManagedIdentityId.FromUserAssignedResourceId(new ResourceIdentifier(rid)));

            }
            else if (provider == "GRAPH_MANAGED_IDENTITY_OBJECT_ID")
            {

                credential = new ManagedIdentityCredential(
                        ManagedIdentityId.FromUserAssignedObjectId(oid));

            }
            else if (provider == "GRAPH_MANAGED_IDENTITY_CLIENT_ID")
            {

                credential = new ManagedIdentityCredential(
                        ManagedIdentityId.FromUserAssignedClientId(client_id));

            }
            else
            {
                credential = new ClientSecretCredential(
                        _graph_options.TenantId,
                        _graph_options.ClientId,
                        _graph_options.ClientSecret);
            }

            var graphClient = new GraphServiceClient(credential, new[] { "https://graph.microsoft.com/.default" });

            return credential;
        }
    }
}
