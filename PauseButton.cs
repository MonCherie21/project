using Godot;

public partial class PauseButton : Button
{
	public override void _Ready()
	{
		Pressed += OnPauseButtonPressed;
	}

	private void OnPauseButtonPressed()
	{
		// Ise-save nito ang current scene bago lumipat sa Escape.tscn
		SceneManager sceneManager = GetNode<SceneManager>("/root/SceneManager");
		sceneManager.LoadScene("res://Escape.tscn");
	}
}
