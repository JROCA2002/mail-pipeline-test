namespace BackEndEnvioMail.Options
{
    public class MailServiceOptions
    {
        public MailServiceOptions()
        {
            this.MailFrom = string.Empty;
            this.MailTo = string.Empty;
            this.MailFromTitulo = string.Empty;
            this.Subject = string.Empty;
        }

        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string MailFromTitulo { get; set; }
        public string Subject { get; set; }

    }
}