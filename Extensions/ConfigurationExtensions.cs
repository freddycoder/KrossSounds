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
}