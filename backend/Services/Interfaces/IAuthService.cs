using Azure.Core;

namespace BackEndEnvioMail.Services.Interfaces
{
    public interface IAuthService
    {
        TokenCredential GetTokenCredential();
    }
}
