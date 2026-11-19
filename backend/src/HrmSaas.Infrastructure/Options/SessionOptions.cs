namespace HrmSaas.Infrastructure.Options;

public sealed class SessionOptions
{
    public const string SectionName = "SessionOptions";

    public string CookieName { get; set; } = "HrmSaas.Sid";
    public string XsrfCookieName { get; set; } = "XSRF-TOKEN";
    public int SlidingExpirationMinutes { get; set; } = 30;
    public int AbsoluteExpirationHours { get; set; } = 12;
    public int LastAccessedUpdateThresholdSeconds { get; set; } = 120;
    public bool RequireSecureCookie { get; set; }
}
