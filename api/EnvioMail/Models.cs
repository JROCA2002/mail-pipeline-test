using System;
public class MailLandingPageRequest
{
    public string Token { get; set; }
    public string Nombre { get; set; }
    public string Empresa { get; set; }
    public string Email { get; set; }
    public string Telefono { get; set; }
    public string Mensaje { get; set; }
}

public class GoogleCaptchaResponse
{
    public bool Success { get; set; }
    public DateTime ChallengeTs { get; set; }
    public string Hostname { get; set; }
    public List<string> ErrorCodes { get; set; }
}

