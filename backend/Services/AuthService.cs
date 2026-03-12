using Azure.Core;
using Azure.Identity;
using EnvioMail.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace BackEndEnvioMail.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenCredential GetTokenCredential()
        {

            TokenCredential? credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = true
            });

            return credential;
        }
    }
}
