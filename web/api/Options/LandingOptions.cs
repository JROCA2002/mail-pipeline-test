namespace EnvioMail.Options
{
    public class LandingOptions
    {
        public LandingOptions()
        {
            this.UrlVerify = string.Empty;
            this.GoogleToken = string.Empty;
            this.SecretKey = string.Empty;
        }
        public string UrlVerify { get; set; }
        public string GoogleToken { get; set; }
        public string SecretKey { get; set; }
    }

}
