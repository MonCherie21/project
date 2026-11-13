using Godot;
using System;

// Seven story battles in order. Progress is saved separately inside each slot.
public static class GameProgress
{
    public const int TotalLevels = 7;

    private static readonly string[] BuildingNames =
    {
        "House 2", "Sari-Sari Store", "Gas Station", "Puregold",
        "McDonald's", "School", "Haunted House"
    };

    public static int CompletedLevels { get; private set; }
    public static int ActiveSlot { get; private set; } = 1;
    public static bool PendingAutoSave { get; private set; }
    public static bool HasSeenBattleTutorial { get; private set; }

    public static string NextBuilding => CompletedLevels >= TotalLevels
        ? "All levels completed"
        : BuildingNames[CompletedLevels];

    public static void StartNewGame()
    {
        CompletedLevels = 0;
        ActiveSlot = 1;
        PendingAutoSave = false;
        HasSeenBattleTutorial = false;
    }

    public static void LoadProgress(int completed, int slot)
    {
        CompletedLevels = Math.Clamp(completed, 0, TotalLevels);
        SetActiveSlot(slot);
        PendingAutoSave = false;
    }

    public static void SetActiveSlot(int slot)
    {
        if (slot is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(slot));
        ActiveSlot = slot;
    }

    public static void MarkSaved(int capturedLevel)
    {
        if (capturedLevel == CompletedLevels)
            PendingAutoSave = false;
    }

    public static bool CompleteLevel(int level)
    {
        // Prevent replaying or skipping levels from changing progress.
        if (level != CompletedLevels + 1 || level > TotalLevels)
            return false;

        CompletedLevels = level;
        PendingAutoSave = true;
        return true;
    }

    public static void MarkBattleTutorialSeen() => HasSeenBattleTutorial = true;

    public static int LevelFromBattleScene(string path)
    {
        if (string.Equals(path, "res://BattleScene.tscn", StringComparison.OrdinalIgnoreCase))
            return 1;

        for (int level = 2; level <= TotalLevels; level++)
        {
            if (string.Equals(path, $"res://BattleScene{level}.tscn",
                StringComparison.OrdinalIgnoreCase))
                return level;
        }
        return 0;
    }

    public static void EnterBuilding(Node caller, int level, string dialogueScene)
    {
        if (level == CompletedLevels + 1)
        {
            Error error = caller.GetTree().ChangeSceneToFile(dialogueScene);
            if (error != Error.Ok)
                GD.PushError("Could not open level " + level + ": " + error);
            return;
        }

        string message = level <= CompletedLevels
            ? $"Level {level} is already cleared. You cannot replay this building."
            : $"Level {level} is locked. Clear Level {CompletedLevels + 1} " +
              $"at {NextBuilding} first.";

        if (CompletedLevels == TotalLevels)
            message = "You have already completed every Yokai battle in Wawa!";

        var root = caller.GetTree().Root;
        if (root.GetNodeOrNull<AcceptDialog>("LevelProgressNotice") != null)
            return;

        var dialog = new AcceptDialog
        {
            Name = "LevelProgressNotice",
            Title = "Yokai Wave Progress",
            DialogText = message
        };
        root.AddChild(dialog);
        dialog.Confirmed += dialog.QueueFree;
        dialog.CloseRequested += dialog.QueueFree;
        dialog.PopupCentered();
    }
}
