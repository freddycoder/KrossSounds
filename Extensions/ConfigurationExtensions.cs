public static class ConfigurationExtensions
{
    public static bool UseHsts(this IConfiguration configuration)
    {
        return string.Equals(
            bool.TrueString, 
            configuration["USE_HSTS"]?.Trim(), 
            StringComparison.InvariantCultureIgnoreCase);
    }
}