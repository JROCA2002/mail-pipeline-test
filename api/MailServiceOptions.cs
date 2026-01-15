public class MailServiceOptions
{
    public MailServiceOptions()
    {
        this.MailFrom = string.Empty;
        this.MailTo = string.Empty;
        this.SmtpClient = string.Empty;
        this.Subject = string.Empty;
        this.MailFromTitulo = string.Empty;
    }

    public string MailFrom { get; set; }
    public string MailTo { get; set; }
    public string SmtpClient { get; set; }
    public int SmtpClientPort { get; set; }
    public bool SmtpClientEnableSSL { get; set; }
    public bool SmtpClientUseDefaultCredentials { get; set; }
    public bool IsBodyHtml { get; set; }
    public string Subject { get; set; }
    public string MailFromTitulo { get; set; }
}