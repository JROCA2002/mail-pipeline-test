using Azure.Core;

namespace EnvioMail.Services.Interfaces
{
    public interface IAuthService
    {
         TokenCredential GetTokenCredential();
    }
}
