
using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("Exit").Pressed += OnExitPressed;
		GetNode<Button>("Collections").Pressed += OnCollectionsPressed;
		GetNode<Button>("Continue").Pressed += OnContinuePressed;
	}

	private void OnNewGamePressed()
{
	GetTree().ChangeSceneToFile("res://character_creation.tscn");
}
	
	private void OnExitPressed()
	{
		GetTree().Quit();
	}
	
	private void OnCollectionsPressed()
	{
		GetTree().ChangeSceneToFile("res://Collection.tscn");
	}
	
	private void OnContinuePressed()
	{
		GetTree().ChangeSceneToFile("res://Continue.tscn");
	}
}
