using Godot;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public static class SupabaseSession
{
    // A refresh token can rotate on every use; never refresh it in parallel.
    private static readonly SemaphoreSlim RefreshLock = new SemaphoreSlim(1, 1);

    public static async Task<bool> RestoreAsync(Node caller)
    {
        await RefreshLock.WaitAsync();
        try
        {
            if (string.IsNullOrEmpty(PlayerData.RefreshToken) &&
                !PlayerData.RestoreLocalSession())
                return false;

            if (!SupabaseConfig.IsConfigured)
                return false;

            string payload = JsonSerializer.Serialize(new
            {
                refresh_token = PlayerData.RefreshToken
            });

            var response = await SupabaseHttp.SendAsync(
                caller,
                SupabaseConfig.AuthUrl + "/token?grant_type=refresh_token",
                HttpClient.Method.Post, payload);

            if (!response.IsSuccess)
            {
                GD.PushWarning("Could not refresh the Supabase login (HTTP " +
                               response.StatusCode + ", request " + response.StartError + ").");
                // Keep the existing refresh token on disk for a future retry.
                return false;
            }

            using var json = JsonDocument.Parse(response.Body);
            string access = json.RootElement.GetProperty("access_token").GetString() ?? "";
            string refresh = json.RootElement.GetProperty("refresh_token").GetString() ?? "";
            if (access.Length == 0 || refresh.Length == 0)
                return false;

            PlayerData.AccessToken = access;
            PlayerData.RefreshToken = refresh;
            if (!PlayerData.RememberLocalSession())
                GD.PushWarning("Session refreshed but could not be saved to this device.");
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning("Could not restore the Supabase login: " + e.Message);
            return false;
        }
        finally
        {
            RefreshLock.Release();
        }
    }
}
