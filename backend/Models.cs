
namespace BackEndEnvioMail.Models
{

    public class MailLandingPageRequest
    {
        public string Token { get; set; }
        public string Nombre { get; set; }
        public string Empresa { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Mensaje { get; set; }

        public MailLandingPageRequest()
        {
            this.Token = string.Empty;
            this.Nombre = string.Empty;
            this.Empresa = string.Empty;
            this.Email = string.Empty;
            this.Telefono = string.Empty;
            this.Mensaje = string.Empty;
        }
    }

}