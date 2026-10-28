
using Godot;

public partial class MainMenu : Control
{
	public override void _Ready()
	{
		GetNode<Button>("NewGame").Pressed += OnNewGamePressed;
		GetNode<Button>("Exit").Pressed += OnExitPressed;
	}

	private void OnNewGamePressed()
	{
		GetTree().ChangeSceneToFile("res://world.tscn");
	}

	private void OnExitPressed()
	{
		GetTree().Quit();
	}
}
