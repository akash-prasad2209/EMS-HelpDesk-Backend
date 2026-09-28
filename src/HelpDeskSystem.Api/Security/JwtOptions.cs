namespace HelpDeskSystem.Api.Security;

// Bound from the "Jwt" section of appsettings.json via IOptions<JwtOptions> -
// no configuration read by string key anywhere else in the code.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
