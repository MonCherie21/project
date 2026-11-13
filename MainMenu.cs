
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
		if (!PlayerData.HasLocalSession)
		{
			GetTree().ChangeSceneToFile("res://character_creation.tscn");
			return;
		}

		// A guest account is tied to its locally saved refresh token. Starting
		// over would replace that token and make previous save slots inaccessible.
		var confirmation = new ConfirmationDialog
		{
			Title = "Start a new game?",
			DialogText = "This creates a new guest player and replaces the remembered login on this PC. " +
				"You will no longer be able to load the previous player's saved games. Continue?"
		};
		AddChild(confirmation);
		confirmation.Confirmed += () => GetTree().ChangeSceneToFile("res://character_creation.tscn");
		confirmation.CloseRequested += confirmation.QueueFree;
		confirmation.PopupCentered();
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
