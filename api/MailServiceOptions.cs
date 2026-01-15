public class MailServiceOptions
{
    public string MailFrom { get; set; }
    public string MailTo { get; set; }
    public string SmtpClient { get; set; }
    public int SmtpClientPort { get; set; }
    public bool SmtpClientEnableSSL { get; set; }
    public bool SmtpClientUseDefaultCredentials { get; set; }
    public bool IsBodyHtml { get; set; }
}