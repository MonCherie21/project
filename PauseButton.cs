using Godot;

public partial class PauseButton : Button
{
	public override void _Ready()
	{
		Pressed += OnPauseButtonPressed;
	}

	private void OnPauseButtonPressed()
	{
		SceneManager sceneManager = GetNode<SceneManager>("/root/SceneManager");
		sceneManager.ShowPause();
	}
}
