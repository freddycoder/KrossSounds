public static class ConfigurationExtensions
{
    public static bool UseHsts(this IConfiguration configuration)
    {
        return string.Equals(
            bool.TrueString, 
            configuration["USE_HSTS"]?.Trim(), 
            StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool UseCookiePolicy(this IConfiguration configuration)
    {
        return string.Equals(
            bool.TrueString, 
            configuration["USE_COOKIE_POLICY"]?.Trim(), 
            StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool UseAntiforgery(this IConfiguration configuration)
    {
        return string.Equals(
            bool.TrueString, 
            configuration["USE_ANTIFORGERY"]?.Trim(), 
            StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool UseSession(this IConfiguration configuration)
    {
        return string.Equals(
            bool.TrueString, 
            configuration["USE_SESSION"]?.Trim(), 
            StringComparison.InvariantCultureIgnoreCase);
    }

    public static bool UseCors(this IConfiguration configuration, out string[] corsOrigins)
    {
        var corsOrigin = configuration["CORS_ORIGIN"];
        corsOrigins = corsOrigin?.Split(',') ?? [];
        return !string.IsNullOrWhiteSpace(corsOrigin);
    }

    public static bool AddXFrameOptionsDeny(this IConfiguration configuration)
    {
        return string.Equals(
            bool.TrueString, 
            configuration["ADD_XFRAMEOPTIONDENY"]?.Trim(), 
            StringComparison.InvariantCultureIgnoreCase);
    }
}