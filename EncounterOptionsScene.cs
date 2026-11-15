using Godot;

public partial class EncounterOptionsScene : Node
{
	// Set these in the Godot Inspector for each level's encounter options scene
	// (e.g., FeedScenePath = "res://EFeed1.tscn", AttackScenePath = "res://BattleScene1.tscn")
	[Export] public string FeedScenePath { get; set; } = "res://EFeed1.tscn";
	[Export] public string AttackScenePath { get; set; } = "res://BattleScene1.tscn";
	[Export] public string MapScenePath { get; set; } = "res://world.tscn";

	// Node path references to your buttons in encounterOptions scenes
	[Export] public NodePath FeedButtonPath { get; set; } = "CanvasLayer/FeedButton";
	[Export] public NodePath AttackButtonPath { get; set; } = "CanvasLayer/AttackButton";
	[Export] public NodePath RunButtonPath { get; set; } = "CanvasLayer/RunButton";

	public override void _Ready()
	{
		GetTree().Paused = false;

		// Connect Feed Button
		var feedBtn = GetNodeOrNull<BaseButton>(FeedButtonPath);
		if (feedBtn != null)
			feedBtn.Pressed += () => ChangeScene(FeedScenePath);

		// Connect Attack Button
		var attackBtn = GetNodeOrNull<BaseButton>(AttackButtonPath);
		if (attackBtn != null)
			attackBtn.Pressed += () => ChangeScene(AttackScenePath);

		// Connect Run Button
		var runBtn = GetNodeOrNull<BaseButton>(RunButtonPath);
		if (runBtn != null)
			runBtn.Pressed += () => ChangeScene(MapScenePath);
	}

	private void ChangeScene(string path)
	{
		GD.Print($"Switching scene to: {path}");
		Error err = GetTree().ChangeSceneToFile(path);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Failed to change scene to {path}: {err}");
		}
	}
}
