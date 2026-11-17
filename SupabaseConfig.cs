using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

// Public Supabase URL and publishable/anon key only.
// NEVER use a database password, secret API key, or service_role key in Godot.
//
// Commit only the public project URL and publishable/anon key in
// res://supabase.shared.json so a teammate can connect after pulling.
// A Git-ignored supabase.local.json can override it on a developer's device.
public static class SupabaseConfig
{
    private const string LocalProjectFile = "res://supabase.local.json";
    private const string LocalUserFile = "user://supabase.local.json";
    private const string SharedProjectFile = "res://supabase.shared.json";

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

    public static bool IsConfigured => IsValidClientSettings(Settings);

    private static bool IsValidClientSettings(LocalSettings settings)
    {
        return Uri.TryCreate(settings.ProjectUrl, UriKind.Absolute, out Uri uri) &&
            uri.Scheme == Uri.UriSchemeHttps &&
            !settings.ProjectUrl.Contains("YOUR-PROJECT", StringComparison.OrdinalIgnoreCase) &&
            IsPublicClientKey(settings.PublishableKey);
    }

    // The older Supabase anon key is a JWT with role "anon". Checking the
    // role prevents accidentally placing a service_role JWT in the game.
    private static bool IsPublicClientKey(string key)
    {
        if (key.StartsWith("sb_publishable_", StringComparison.Ordinal))
            return key.Length > "sb_publishable_".Length;

        if (!key.StartsWith("eyJ", StringComparison.Ordinal))
            return false;

        string[] segments = key.Split('.');
        if (segments.Length != 3)
            return false;

        try
        {
            string payload = segments[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight((payload.Length + 3) / 4 * 4, '=');
            using var json = JsonDocument.Parse(Convert.FromBase64String(payload));
            return json.RootElement.TryGetProperty("role", out JsonElement role) &&
                role.GetString() == "anon";
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static string AuthUrl => ProjectUrl.TrimEnd('/') + "/auth/v1";
    public static string DataUrl => ProjectUrl.TrimEnd('/') + "/rest/v1";

    // Useful if someone changes their local file while the game is running.
    public static void Reload() => _settings = null;

    private static LocalSettings LoadSettings()
    {
        // Personal overrides come first. The shared tracked file lets a
        // fresh Git clone run without making each teammate copy a JSON file.
        foreach (string path in new[] { LocalProjectFile, LocalUserFile, SharedProjectFile })
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
                if (IsValidClientSettings(settings))
                    return settings;

                // A placeholder or invalid override should not hide a valid
                // shared configuration further down this list.
                if (path != SharedProjectFile)
                    GD.PushWarning("Supabase settings in " + path +
                        " are incomplete or are not a public client key.");
            }
            catch (Exception ex)
            {
                GD.PushWarning("Could not load " + path + ": " + ex.Message);
            }
        }

        var environmentSettings = new LocalSettings
        {
            ProjectUrl = (System.Environment.GetEnvironmentVariable("YOKAI_SUPABASE_URL") ?? "").Trim(),
            PublishableKey = (System.Environment.GetEnvironmentVariable("YOKAI_SUPABASE_KEY") ?? "").Trim()
        };
        return IsValidClientSettings(environmentSettings) ? environmentSettings : new LocalSettings();
    }
}
