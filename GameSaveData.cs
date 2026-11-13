using System;
using System.Text.Json.Serialization;

// Versioned JSON snapshot. Other systems (battles, candy, quests) can be added later.
public sealed class GameSaveData
{
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    [JsonPropertyName("scene")]
    public string Scene { get; set; } = "res://world.tscn";

    [JsonPropertyName("position_x")]
    public float PositionX { get; set; }

    [JsonPropertyName("position_y")]
    public float PositionY { get; set; }

    public static GameSaveData Capture() => new GameSaveData
    {
        PositionX = PlayerData.WorldPosition.X,
        PositionY = PlayerData.WorldPosition.Y
    };
}

public sealed class GameSaveSlot
{
    public int Number { get; set; }
    public DateTimeOffset SavedAt { get; set; }
    public GameSaveData Data { get; set; } = new GameSaveData();
}
