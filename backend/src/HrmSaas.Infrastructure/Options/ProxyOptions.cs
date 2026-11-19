namespace HrmSaas.Infrastructure.Options;

public sealed class ProxyOptions
{
    public const string SectionName = "ProxyOptions";

    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];
}
