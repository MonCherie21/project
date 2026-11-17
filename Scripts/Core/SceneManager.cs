using Godot;

public partial class SceneManager : Node
{
	private CanvasLayer _pauseOverlay;
	private CanvasLayer _settingsOverlay;
	private bool _transitioning;
	private CanvasLayer _transitionOverlay;

	// All ordinary scene changes use one fade, on a persistent CanvasLayer.
	// The overlay runs even when the pause menu temporarily pauses the tree.
	public void TransitionTo(string path)
	{
		if (_transitioning) return;
		if (string.IsNullOrWhiteSpace(path) || !ResourceLoader.Exists(path))
		{
			GD.PushError("Scene transition destination is missing: " + path);
			return;
		}

		_transitioning = true;
		GetTree().Paused = false;
		_transitionOverlay = new CanvasLayer
		{
			Name = "SceneFade",
			Layer = 240,
			ProcessMode = ProcessModeEnum.Always
		};
		GetTree().Root.AddChild(_transitionOverlay);
		var shade = new ColorRect
		{
			Color = new Color(0.018f, 0.034f, 0.03f, 0),
			MouseFilter = Control.MouseFilterEnum.Stop,
			AnchorRight = 1,
			AnchorBottom = 1
		};
		_transitionOverlay.AddChild(shade);
		var outTween = _transitionOverlay.CreateTween().SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.InOut);
		outTween.TweenProperty(shade, "color:a", 1f, 0.29);
		outTween.Finished += () =>
		{
			Error result = GetTree().ChangeSceneToFile(path);
			if (result != Error.Ok)
				GD.PushError("Cannot change scene to " + path + ": " + result);
			var inTween = _transitionOverlay.CreateTween().SetTrans(Tween.TransitionType.Sine)
				.SetEase(Tween.EaseType.InOut);
			inTween.TweenInterval(0.08);
			inTween.TweenProperty(shade, "color:a", 0f, 0.36);
			inTween.Finished += () =>
			{
				if (GodotObject.IsInstanceValid(_transitionOverlay))
					_transitionOverlay.QueueFree();
				_transitionOverlay = null;
				_transitioning = false;
			};
		};
	}

	// Open a persistent overlay; do not change scenes or unpause the world.
	public void ShowSettings()
	{
		if (GodotObject.IsInstanceValid(_settingsOverlay))
			return;

		PackedScene scene = GD.Load<PackedScene>("res://Scenes/Menus/Settings.tscn");
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

		PackedScene scene = GD.Load<PackedScene>("res://Scenes/Menus/Escape.tscn");
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
		TransitionTo("res://Scenes/Menus/main_menu.tscn");
	}

	// Dito natin ise-save ang path ng huling scene bago mag-pause
	public string LastScenePath { get; private set; } = "";

	public void LoadScene(string path)
	{
		LastScenePath = GetTree().CurrentScene?.SceneFilePath ?? "";
		TransitionTo(path);
	}

	public void GoBack()
	{
		if (!string.IsNullOrEmpty(LastScenePath))
		{
			TransitionTo(LastScenePath);
		}
		else
		{
			TransitionTo("res://Scenes/Menus/main_menu.tscn");
		}
	}
}
