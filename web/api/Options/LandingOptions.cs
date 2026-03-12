namespace EnvioMailFrontEnd.Options
{
    public class LandingOptions
    {
        public LandingOptions()
        {
            this.UrlVerify = string.Empty;
            this.SiteKey = string.Empty;
            this.SecretKey = string.Empty;
        }
        public string UrlVerify { get; set; }
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
    }

}
