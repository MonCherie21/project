using Godot;

public partial class BackButton : Button
{
	public override void _Ready()
	{
		Pressed += OnBackButtonPressed;
	}

	private void OnBackButtonPressed()
	{
		SceneManager sceneManager = GetNode<SceneManager>("/root/SceneManager");
		sceneManager.ResumeFromPause();
	}
}
