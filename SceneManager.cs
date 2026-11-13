using Godot;

public partial class SceneManager : Node
{
	private CanvasLayer _pauseOverlay;

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
