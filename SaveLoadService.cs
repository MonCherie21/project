using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// Uses the three-slot player_saves table already created in Supabase.
public static class SaveLoadService
{
    // A quick succession of victories must not let an older autosave
    // finish after and overwrite a newer progress snapshot.
    private static readonly SemaphoreSlim SaveLock = new SemaphoreSlim(1, 1);

    private static void CheckSlot(int slot)
    {
        if (slot < 1 || slot > 3)
            throw new ArgumentOutOfRangeException(nameof(slot), "Only slots 1 through 3 exist.");
    }

    private static async Task EnsureSessionAsync(Node caller)
    {
        if (!await SupabaseSession.RestoreAsync(caller))
            throw new InvalidOperationException(
                "Cannot restore your login: " + SupabaseSession.LastError);

        if (PlayerData.PlayerId <= 0)
            throw new InvalidOperationException("No registered player found on this device.");
    }

    public static async Task<List<GameSaveSlot>> ListAsync(Node caller)
    {
        await EnsureSessionAsync(caller);
        string url = SupabaseConfig.DataUrl +
            "/player_saves?select=slot_number,saved_at,save_data" +
            "&player_id=eq." + PlayerData.PlayerId + "&order=slot_number.asc";

        var response = await SupabaseHttp.SendAsync(
            caller, url, HttpClient.Method.Get, accessToken: PlayerData.AccessToken);

        if (!response.IsSuccess)
            throw new InvalidOperationException(
                "Could not load save slots (HTTP " + response.StatusCode +
                ", request " + response.StartError + ").");

        var slots = new List<GameSaveSlot>();
        using var json = JsonDocument.Parse(response.Body);
        foreach (var row in json.RootElement.EnumerateArray())
        {
            var snapshot = JsonSerializer.Deserialize<GameSaveData>(
                row.GetProperty("save_data").GetRawText());
            if (snapshot == null)
                continue;

            slots.Add(new GameSaveSlot
            {
                Number = row.GetProperty("slot_number").GetInt32(),
                SavedAt = row.GetProperty("saved_at").GetDateTimeOffset(),
                Data = snapshot
            });
        }
        return slots;
    }

    public static async Task SaveAsync(Node caller, int slot, GameSaveData snapshot)
    {
        CheckSlot(slot);
        await SaveLock.WaitAsync();
        try
        {
            await EnsureSessionAsync(caller);

            string url = SupabaseConfig.DataUrl +
                "/player_saves?on_conflict=player_id,slot_number";
            string payload = JsonSerializer.Serialize(new
            {
                player_id = PlayerData.PlayerId,
                slot_number = slot,
                save_data = snapshot,
                saved_at = DateTimeOffset.UtcNow
            });

            var response = await SupabaseHttp.SendAsync(
                caller, url, HttpClient.Method.Post, payload,
                PlayerData.AccessToken, "resolution=merge-duplicates,return=minimal");

            if (!response.IsSuccess)
                throw new InvalidOperationException(
                    "Could not save slot " + slot + " (HTTP " + response.StatusCode +
                    ", request " + response.StartError + ").");
        }
        finally
        {
            SaveLock.Release();
        }
    }

    // Slot deletion removes ALL checkpoints belonging to the selected account.
    // If RLS blocks DELETE, the local account is kept so progress isn't lost
    // just because the client could not reach/update the server.
    public static async Task DeleteAllSavesAsync(Node caller)
    {
        await SaveLock.WaitAsync();
        try
        {
            var existing = await ListAsync(caller);
            if (existing.Count == 0) return;

            string url = SupabaseConfig.DataUrl + "/player_saves?player_id=eq." +
                         PlayerData.PlayerId;
            var response = await SupabaseHttp.SendAsync(caller, url,
                HttpClient.Method.Delete, accessToken: PlayerData.AccessToken,
                preference: "return=representation");

            if (!response.IsSuccess)
                throw new InvalidOperationException("The server could not delete saves (HTTP " +
                    response.StatusCode + "). Check your Supabase delete policies.");

            using var json = JsonDocument.Parse(response.Body);
            if (json.RootElement.ValueKind != JsonValueKind.Array ||
                json.RootElement.GetArrayLength() < existing.Count)
                throw new InvalidOperationException(
                    "Supabase did not confirm removal of every save. Check your DELETE policy.");
        }
        finally
        {
            SaveLock.Release();
        }
    }

    public static void PrepareLoad(GameSaveSlot slot)
    {
        CheckSlot(slot.Number);
        if (slot.Data.Scene != "res://world.tscn")
            throw new InvalidOperationException("This save uses an unsupported game scene.");

        PlayerData.WorldPosition = new Vector2(slot.Data.PositionX, slot.Data.PositionY);
        PlayerData.HasLoadedPosition = true;
        GameProgress.LoadProgress(slot.Data.CompletedLevels, slot.Number);
    }
}
