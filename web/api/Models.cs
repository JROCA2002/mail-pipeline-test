namespace EnvioMailFrontEnd
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

    public class GoogleCaptchaResponse
    {
        public bool Success { get; set; }
        public DateTime ChallengeTs { get; set; }
        public string Hostname { get; set; }
        public List<string> ErrorCodes { get; set; }

        public GoogleCaptchaResponse()
        {
            this.Success = false;
            this.Hostname = string.Empty;
            this.ChallengeTs = DateTime.Now;
            this.ErrorCodes = new List<string>();
        }
    }
}