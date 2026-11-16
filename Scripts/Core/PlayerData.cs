using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

// The three local slots are separate player accounts, not three saves.
public static class PlayerData
{
    private const string OldFile = "user://yokai_session.json";
    private const string ProfilesFile = "user://yokai_profiles.json";
    private static bool _registryReadFailed;
    public static bool AccountRegistryError => _registryReadFailed;

    public static int PlayerId;
    public static string PlayerName = "";
    public static string Gender = "";
    public static string AccessToken = "";
    public static string RefreshToken = "";
    public static int ActiveAccountSlot { get; private set; }
    public static int PendingNewAccountSlot { get; set; }
    public static Vector2 WorldPosition = new Vector2(-193, -405);
    public static bool HasLoadedPosition;
    public static bool PendingMovementTutorial;

    public sealed class StoredSession
    {
        public int Slot { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public string Gender { get; set; } = "";
        public string RefreshToken { get; set; } = "";
    }

    public sealed class Registry
    {
        public int Version { get; set; } = 1;
        public List<StoredSession> Accounts { get; set; } = new();
    }

    public static bool HasLocalSession => GetAccounts().Count > 0;

    public static List<StoredSession> GetAccounts()
    {
        // The old one-account login is copied, never deleted. An empty new
        // registry is intentional: it prevents a deleted old account returning.
        if (!FileAccess.FileExists(ProfilesFile))
        {
            _registryReadFailed = false;
            var legacy = ReadLegacy();
            if (legacy == null) return new();
            legacy.Slot = 1;
            var accounts = new List<StoredSession> { legacy };
            SaveRegistry(accounts);
            return accounts;
        }

        try
        {
            using var file = FileAccess.Open(ProfilesFile, FileAccess.ModeFlags.Read);
            var registry = file == null ? null : JsonSerializer.Deserialize<Registry>(file.GetAsText());
            if (registry?.Accounts == null) throw new InvalidOperationException("Invalid player registry.");
            if (registry.Accounts.Any(a => a == null || a.Slot is < 1 or > 3 ||
                a.PlayerId <= 0 || string.IsNullOrEmpty(a.RefreshToken)) ||
                registry.Accounts.Select(a => a.Slot).Distinct().Count() != registry.Accounts.Count)
                throw new InvalidOperationException("Player slots contain invalid entries.");
            _registryReadFailed = false;
            return registry.Accounts.OrderBy(a => a.Slot).ToList();
        }
        catch (Exception e)
        {
            _registryReadFailed = true;
            GD.PushWarning("Cannot read player accounts: " + e.Message);
            return new();
        }
    }

    public static int FirstEmptyAccountSlot()
    {
        var accounts = GetAccounts();
        for (int i = 1; i <= 3; i++)
            if (!accounts.Any(a => a.Slot == i)) return i;
        return 0;
    }

    public static bool SelectAccount(int slot)
    {
        var a = GetAccounts().FirstOrDefault(account => account.Slot == slot);
        if (a == null) return false;
        ActiveAccountSlot = slot;
        PendingNewAccountSlot = 0;
        PlayerId = a.PlayerId;
        PlayerName = a.PlayerName;
        Gender = a.Gender;
        RefreshToken = a.RefreshToken;
        AccessToken = ""; // Never reuse the previous player's token.
        HasLoadedPosition = false;
        PendingMovementTutorial = false;
        return true;
    }

    public static bool RestoreLocalSession()
    {
        var accounts = GetAccounts();
        if (accounts.Count == 0) return false;
        return SelectAccount(accounts.Any(a => a.Slot == ActiveAccountSlot)
            ? ActiveAccountSlot : accounts[0].Slot);
    }

    public static bool RegisterNewAccount(int slot)
    {
        if (slot is < 1 or > 3 || PlayerId <= 0 || RefreshToken.Length == 0)
            return false;
        var accounts = GetAccounts();
        if (accounts.Any(a => a.Slot == slot)) return false;
        accounts.Add(new StoredSession { Slot = slot, PlayerId = PlayerId,
            PlayerName = PlayerName, Gender = Gender, RefreshToken = RefreshToken });
        if (!SaveRegistry(accounts)) return false;
        ActiveAccountSlot = slot;
        PendingNewAccountSlot = 0;
        return true;
    }

    public static bool RemoveAccount(int slot)
    {
        var accounts = GetAccounts();
        if (accounts.RemoveAll(a => a.Slot == slot) == 0 || !SaveRegistry(accounts))
            return false;
        if (ActiveAccountSlot == slot)
        {
            ActiveAccountSlot = 0;
            PlayerId = 0;
            PlayerName = Gender = AccessToken = RefreshToken = "";
            HasLoadedPosition = false;
            GameProgress.StartNewGame();
        }
        return true;
    }

    public static bool RememberLocalSession()
    {
        if (ActiveAccountSlot == 0 || PlayerId <= 0 || RefreshToken.Length == 0)
            return false;
        var accounts = GetAccounts();
        var a = accounts.FirstOrDefault(account => account.Slot == ActiveAccountSlot);
        if (a == null || a.PlayerId != PlayerId) return false;
        a.PlayerName = PlayerName;
        a.Gender = Gender;
        a.RefreshToken = RefreshToken;
        return SaveRegistry(accounts);
    }

    public static bool CanUpdateAccountSlots()
    {
        var accounts = GetAccounts();
        return !_registryReadFailed && SaveRegistry(accounts);
    }

    private static StoredSession ReadLegacy()
    {
        if (!FileAccess.FileExists(OldFile)) return null;
        try
        {
            using var file = FileAccess.Open(OldFile, FileAccess.ModeFlags.Read);
            var a = file == null ? null : JsonSerializer.Deserialize<StoredSession>(file.GetAsText());
            return a != null && a.PlayerId > 0 && !string.IsNullOrEmpty(a.RefreshToken) ? a : null;
        }
        catch (Exception e)
        {
            GD.PushWarning("Could not migrate old login: " + e.Message);
            return null;
        }
    }

    private static bool SaveRegistry(List<StoredSession> accounts)
    {
        if (_registryReadFailed) return false; // Never overwrite a corrupt registry.
        try
        {
            // Atomic replace protects all three accounts during token rotation.
            string temporary = ProfilesFile + ".tmp";
            using (var file = FileAccess.Open(temporary, FileAccess.ModeFlags.Write))
            {
                if (file == null) return false;
                file.StoreString(JsonSerializer.Serialize(new Registry { Accounts = accounts }));
                file.Flush();
            }
            Error result = DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(temporary),
                ProjectSettings.GlobalizePath(ProfilesFile));
            if (result != Error.Ok) throw new InvalidOperationException(result.ToString());
            return true;
        }
        catch (Exception e)
        {
            GD.PushWarning("Could not save player accounts: " + e.Message);
            return false;
        }
    }
}
