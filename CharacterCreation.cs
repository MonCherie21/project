
using Godot;

public partial class CharacterCreation : Control
{
	private string _gender = "";

	public override void _Ready()
	{
		GetNode<Button>("Male").Pressed += () =>
		{
			_gender = "Male";
			GD.Print("Male selected!");
		};

		GetNode<Button>("Female").Pressed += () =>
		{
			_gender = "Female";
			GD.Print("Female selected!");
		};

		GetNode<Button>("Start").Pressed += OnStartPressed;
	}

	private void OnStartPressed()
	{
		string playerName = GetNode<LineEdit>("NameInput").Text.Trim();

		if (string.IsNullOrWhiteSpace(playerName))
		{
			GD.Print("Please enter a player name!");
			return;
		}

		if (string.IsNullOrEmpty(_gender))
		{
			GD.Print("Please select a character!");
			return;
		}

		GD.Print($"Name: {playerName}");
		GD.Print($"Gender: {_gender}");

		PlayerData.PlayerName = playerName;
		PlayerData.Gender = _gender;

		GetTree().ChangeSceneToFile("res://world.tscn");
	}
}
