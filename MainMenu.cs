
using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("Exit").Pressed += OnExitPressed;
		GetNode<Button>("Collections").Pressed += OnCollectionsPressed;
	}

	private void OnNewGamePressed()
	{
		GetTree().ChangeSceneToFile("res://world.tscn");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
	
	private void OnCollectionsPressed()
	{
		GetTree().ChangeSceneToFile("res://Collection.tscn");
	}
}
