using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Public Supabase URL and publishable/anon key only.
// NEVER use a database password, secret API key, or service_role key in Godot.
//
// Set your real values ONCE in res://supabase.local.json. That file is ignored
// by Git, so updating World.cs or pulling a branch won't overwrite it.
public static class SupabaseConfig
{
    private const string LocalProjectFile = "res://supabase.local.json";
    private const string LocalUserFile = "user://supabase.local.json";

    public sealed class LocalSettings
    {
        [JsonPropertyName("project_url")]
        public string ProjectUrl { get; set; } = "";

        [JsonPropertyName("publishable_key")]
        public string PublishableKey { get; set; } = "";
    }

    private static LocalSettings _settings;

    private static LocalSettings Settings => _settings ??= LoadSettings();

    public static string ProjectUrl => Settings.ProjectUrl;
    public static string PublishableKey => Settings.PublishableKey;

    public static bool IsConfigured =>
        Uri.TryCreate(ProjectUrl, UriKind.Absolute, out Uri uri) &&
        uri.Scheme == Uri.UriSchemeHttps &&
        !ProjectUrl.Contains("YOUR-PROJECT", StringComparison.OrdinalIgnoreCase) &&
        !string.IsNullOrWhiteSpace(PublishableKey) &&
        !PublishableKey.Contains("YOUR_", StringComparison.OrdinalIgnoreCase);

    public static string AuthUrl => ProjectUrl.TrimEnd('/') + "/auth/v1";
    public static string DataUrl => ProjectUrl.TrimEnd('/') + "/rest/v1";

    // Useful if someone changes their local file while the game is running.
    public static void Reload() => _settings = null;

    private static LocalSettings LoadSettings()
    {
        // Project-local values take priority during development. A user-data
        // copy is supported for exported builds. Environment variables are
        // an optional fallback for testers and CI.
        foreach (string path in new[] { LocalProjectFile, LocalUserFile })
        {
            if (!Godot.FileAccess.FileExists(path))
                continue;

            try
            {
                using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                    continue;

                var settings = JsonSerializer.Deserialize<LocalSettings>(file.GetAsText());
                if (settings == null)
                    throw new InvalidOperationException("Missing settings.");

                settings.ProjectUrl = (settings.ProjectUrl ?? "").Trim().TrimEnd('/');
                settings.PublishableKey = (settings.PublishableKey ?? "").Trim();
                return settings;
            }
            catch (Exception ex)
            {
                GD.PushWarning("Could not load " + path + ": " + ex.Message);
            }
        }

        return new LocalSettings
        {
            ProjectUrl = (Environment.GetEnvironmentVariable("YOKAI_SUPABASE_URL") ?? "").Trim(),
            PublishableKey = (Environment.GetEnvironmentVariable("YOKAI_SUPABASE_KEY") ?? "").Trim()
        };
    }
}
