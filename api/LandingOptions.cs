namespace EnvioMail
{
    public class LandingOptions
    {
        public LandingOptions()
        {
            this.UrlVerify = string.Empty;
            this.GoogleToken = string.Empty;
        }
        public string UrlVerify { get; set; }
        public string GoogleToken { get; set; }
    }

}
