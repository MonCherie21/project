using System;
using System.Collections.Generic;
using System.Linq;

// One source of truth for candy and friendship across all seven EFeed scenes,
// the overworld party inventory, battles, and the existing Supabase save JSON.
public static class CandyInventory
{
    public const int PartnerCount = 7;
    public const int StarterCandies = 5;
    public const int MaxFriendshipLevel = 3;
    public const int FeedsPerLevel = 2;

    private static int _candies = StarterCandies;
    private static int[] _feeds = new int[PartnerCount];
    private static int _activePartnerIndex;
    private static HashSet<string> _pickedUp = new(StringComparer.Ordinal);
    private static bool _dirty;

    public static event Action Changed;
    public static int Candies => _candies;
    public static bool HasUnsavedChanges => _dirty;

    // Whisper is the starter. Every other Yokai joins only AFTER its own
    // level is won: escaping a battle must never unlock the opponent.
    public static int AvailablePartners => Math.Clamp(GameProgress.CompletedLevels, 1, PartnerCount);
    public static int ActivePartnerIndex => Math.Clamp(_activePartnerIndex, 0, AvailablePartners - 1);

    public static string Name(int index) => YokaiRoster.Name(index);

    public static bool TrySelectActivePartner(int index)
    {
        if (index < 0 || index >= AvailablePartners || index == ActivePartnerIndex)
            return false;

        _activePartnerIndex = index;
        NotifyChanged();
        return true;
    }

    public static int FeedCount(int index) =>
        index >= 0 && index < PartnerCount ? _feeds[index] : 0;

    public static int FriendshipLevel(int index) =>
        Math.Min(MaxFriendshipLevel, FeedCount(index) / FeedsPerLevel);

    public static int FriendshipProgress(int index) =>
        FriendshipLevel(index) >= MaxFriendshipLevel
            ? 100 : (FeedCount(index) % FeedsPerLevel) * 50;

    public static bool TryFeed(int index)
    {
        if (index < 0 || index >= AvailablePartners || _candies <= 0 ||
            FriendshipLevel(index) >= MaxFriendshipLevel)
            return false;

        _candies--;
        _feeds[index]++;
        NotifyChanged();
        return true;
    }

    public static bool IsPickedUp(string pickupId) => _pickedUp.Contains(pickupId);

    public static bool Collect(string pickupId, int amount)
    {
        if (string.IsNullOrWhiteSpace(pickupId) || amount <= 0 || !_pickedUp.Add(pickupId))
            return false;
        _candies = Math.Min(9999, _candies + amount);
        NotifyChanged();
        return true;
    }

    public static void StartNewGame()
    {
        _candies = StarterCandies;
        _feeds = new int[PartnerCount];
        _activePartnerIndex = 0;
        _pickedUp = new HashSet<string>(StringComparer.Ordinal);
        _dirty = false;
        Changed?.Invoke();
    }

    public static void Load(GameSaveData save)
    {
        // V1/V2 saves predate candy persistence. They retain their battle
        // progress and begin with the five candies formerly local to EFeed1.
        if (save == null || save.Version < 3)
        {
            StartNewGame();
            return;
        }

        _candies = Math.Clamp(save.CandyCount, 0, 9999);
        _feeds = new int[PartnerCount];
        for (int i = 0; i < _feeds.Length && i < (save.FriendshipFeeds?.Length ?? 0); i++)
            _feeds[i] = Math.Clamp(save.FriendshipFeeds[i], 0,
                FeedsPerLevel * MaxFriendshipLevel);
        _pickedUp = new HashSet<string>(
            (save.CollectedCandyIds ?? Array.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id)), StringComparer.Ordinal);
        _activePartnerIndex = save.Version >= 4
            ? Math.Clamp(save.ActiveYokaiIndex, 0, AvailablePartners - 1) : 0;
        _dirty = false;
        Changed?.Invoke();
    }

    public static int[] SnapshotFeeds() => (int[])_feeds.Clone();
    public static string[] SnapshotPickups() => _pickedUp.OrderBy(id => id, StringComparer.Ordinal).ToArray();

    public static void MarkSaved(GameSaveData snapshot)
    {
        // An async save could finish after another pickup; don't mark newer
        // progress as safely saved if it wasn't in that snapshot.
        if (snapshot != null && snapshot.CandyCount == _candies &&
            snapshot.ActiveYokaiIndex == ActivePartnerIndex &&
            (snapshot.FriendshipFeeds ?? Array.Empty<int>()).SequenceEqual(_feeds) &&
            (snapshot.CollectedCandyIds ?? Array.Empty<string>()).SequenceEqual(SnapshotPickups()))
            _dirty = false;
    }

    private static void NotifyChanged()
    {
        _dirty = true;
        Changed?.Invoke();
    }
}
