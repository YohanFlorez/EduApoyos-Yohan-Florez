namespace EduApoyos.Infrastructure.Identity;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpiracionMinutos { get; set; } = 60;
}
