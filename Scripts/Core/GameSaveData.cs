using System;
using System.Text.Json.Serialization;

// Versioned JSON snapshot. V3 adds candy/friendship; V4 adds active Yokai.
public sealed class GameSaveData
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 4;

    [JsonPropertyName("scene")]
    public string Scene { get; set; } = "res://Scenes/World/world.tscn";

    [JsonPropertyName("position_x")]
    public float PositionX { get; set; }

    [JsonPropertyName("position_y")]
    public float PositionY { get; set; }

    // Older version-1 saves automatically default to zero cleared levels.
    [JsonPropertyName("completed_levels")]
    public int CompletedLevels { get; set; }

    [JsonPropertyName("candy_count")]
    public int CandyCount { get; set; } = CandyInventory.StarterCandies;

    [JsonPropertyName("friendship_feeds")]
    public int[] FriendshipFeeds { get; set; } = new int[CandyInventory.PartnerCount];

    [JsonPropertyName("active_yokai_index")]
    public int ActiveYokaiIndex { get; set; }

    [JsonPropertyName("collected_candy_ids")]
    public string[] CollectedCandyIds { get; set; } = System.Array.Empty<string>();

    public static GameSaveData Capture() => new GameSaveData
    {
        PositionX = PlayerData.WorldPosition.X,
        PositionY = PlayerData.WorldPosition.Y,
        CompletedLevels = GameProgress.CompletedLevels,
        CandyCount = CandyInventory.Candies,
        ActiveYokaiIndex = CandyInventory.ActivePartnerIndex,
        FriendshipFeeds = CandyInventory.SnapshotFeeds(),
        CollectedCandyIds = CandyInventory.SnapshotPickups()
    };
}

public sealed class GameSaveSlot
{
    public int Number { get; set; }
    public DateTimeOffset SavedAt { get; set; }
    public GameSaveData Data { get; set; } = new GameSaveData();
}
