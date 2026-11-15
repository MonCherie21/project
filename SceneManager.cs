using Godot;

public partial class SceneManager : Node
{
	private CanvasLayer _pauseOverlay;
	private CanvasLayer _settingsOverlay;

	// Open a persistent overlay; do not change scenes or unpause the world.
	public void ShowSettings()
	{
		if (GodotObject.IsInstanceValid(_settingsOverlay))
			return;

		PackedScene scene = GD.Load<PackedScene>("res://Settings.tscn");
		if (scene == null)
		{
			GD.PushError("Could not open settings.");
			return;
		}

		_settingsOverlay = scene.Instantiate<CanvasLayer>();
		_settingsOverlay.Name = "ActiveSettings";
		_settingsOverlay.Layer = 120;
		_settingsOverlay.ProcessMode = ProcessModeEnum.Always;
		GetTree().Root.AddChild(_settingsOverlay);
	}

	public void CloseSettings()
	{
		if (GodotObject.IsInstanceValid(_settingsOverlay))
			_settingsOverlay.QueueFree();
		_settingsOverlay = null;
	}

	// Keep the current world/battle scene alive underneath the pause UI.
	public void ShowPause()
	{
		if (GodotObject.IsInstanceValid(_pauseOverlay))
			return;

		PackedScene scene = GD.Load<PackedScene>("res://Escape.tscn");
		if (scene == null)
		{
			GD.PushError("Could not open the pause menu.");
			return;
		}

		_pauseOverlay = scene.Instantiate<CanvasLayer>();
		_pauseOverlay.Name = "ActivePauseMenu";
		_pauseOverlay.Layer = 90;
		_pauseOverlay.ProcessMode = ProcessModeEnum.Always;
		GetTree().Root.AddChild(_pauseOverlay);
		GetTree().Paused = true;
	}

	public void ResumeFromPause()
	{
		GetTree().Paused = false;
		if (GodotObject.IsInstanceValid(_pauseOverlay))
			_pauseOverlay.QueueFree();
		_pauseOverlay = null;
	}

	public void ExitToMainMenu()
	{
		CloseSettings();
		ResumeFromPause();
		GetTree().ChangeSceneToFile("res://main_menu.tscn");
	}

	// Dito natin ise-save ang path ng huling scene bago mag-pause
	public string LastScenePath { get; private set; } = "";

	public void LoadScene(string path)
	{
		LastScenePath = GetTree().CurrentScene.SceneFilePath;
		GetTree().ChangeSceneToFile(path);
	}

	public void GoBack()
	{
		if (!string.IsNullOrEmpty(LastScenePath))
		{
			GetTree().ChangeSceneToFile(LastScenePath);
		}
		else
		{
			// Fallback kung sakaling walang naka-save (hal. nag-start directly sa Pause Menu)
			GetTree().ChangeSceneToFile("res://main.tscn"); 
		}
	}
}
