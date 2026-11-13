using Godot;

public partial class BackButton : Button
{
	public override void _Ready()
	{
		Pressed += OnBackButtonPressed;
	}

	private void OnBackButtonPressed()
	{
		// Awtomatiko nitong ibabalik ka sa huling scene na binisita mo
		SceneManager sceneManager = GetNode<SceneManager>("/root/SceneManager");
		sceneManager.GoBack();
	}
}
