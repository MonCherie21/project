using Godot;
using System;
using System.Text.Json;

public static class PlayerData
{
    private const string SessionFile = "user://yokai_session.json";

    public static int PlayerId = 0;
    public static string PlayerName = "";
    public static string Gender = "";
    public static string AccessToken = "";
    public static string RefreshToken = "";

    // A saved position is only applied when the player deliberately loads a slot.
    public static Vector2 WorldPosition = new Vector2(-193, -405);
    public static bool HasLoadedPosition = false;

    // Shown only after a freshly created character, never on Continue/load.
    public static bool PendingMovementTutorial = false;

    public static bool HasLocalSession => FileAccess.FileExists(SessionFile);

    public static bool RestoreLocalSession()
    {
        if (!HasLocalSession)
            return false;

        try
        {
            using var file = FileAccess.Open(SessionFile, FileAccess.ModeFlags.Read);
            if (file == null)
                return false;

            var data = JsonSerializer.Deserialize<StoredSession>(file.GetAsText());
            if (data == null || data.PlayerId <= 0 || string.IsNullOrEmpty(data.RefreshToken))
                return false;

            PlayerId = data.PlayerId;
            PlayerName = data.PlayerName ?? "";
            Gender = data.Gender ?? "";
            RefreshToken = data.RefreshToken;
            AccessToken = ""; // Always request a fresh access token after restart.
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning("Could not read the locally saved login: " + e.Message);
            return false;
        }
    }

    public static bool RememberLocalSession()
    {
        if (PlayerId <= 0 || string.IsNullOrEmpty(RefreshToken))
            return false;

        try
        {
            using var file = FileAccess.Open(SessionFile, FileAccess.ModeFlags.Write);
            if (file == null)
                return false;

            file.StoreString(JsonSerializer.Serialize(new StoredSession
            {
                PlayerId = PlayerId,
                PlayerName = PlayerName,
                Gender = Gender,
                RefreshToken = RefreshToken
            }));
            file.Flush();
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning("Could not remember the login on this device: " + e.Message);
            return false;
        }
    }

    public sealed class StoredSession
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public string Gender { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }
}
