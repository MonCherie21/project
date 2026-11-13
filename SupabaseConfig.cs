// Public settings only. NEVER put a database password or secret/service_role key here.
public static class SupabaseConfig
{
    public const string ProjectUrl = "https://YOUR-PROJECT.supabase.co";
    public const string PublishableKey = "YOUR_PUBLISHABLE_KEY";

    public static bool IsConfigured =>
        !ProjectUrl.Contains("YOUR-PROJECT") && !PublishableKey.Contains("YOUR_");

    public static string AuthUrl => ProjectUrl.TrimEnd('/') + "/auth/v1";
    public static string DataUrl => ProjectUrl.TrimEnd('/') + "/rest/v1";
}
