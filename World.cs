
using Godot;

public partial class World : Node2D
{
	public override void _Ready()
	{
		GD.Print("World loaded!");
		GD.Print("Selected gender: " + PlayerData.Gender);

		string scenePath = "";

		if (PlayerData.Gender == "Male")
		{
			scenePath = "res://player_2.tscn";
		}
		else if (PlayerData.Gender == "Female")
		{
			scenePath = "res://player.tscn";
		}
		else
		{
			GD.PrintErr("No valid gender selected!");
			return;
		}

		PackedScene playerScene =
			GD.Load<PackedScene>(scenePath);

		if (playerScene == null)
		{
			GD.PrintErr("Could not load: " + scenePath);
			return;
		}

		Node2D player = playerScene.Instantiate<Node2D>();

		GetNode<Node2D>("../MapBackground").AddChild(player);

		player.Position = new Vector2(1, -3);

		GD.Print("Player spawned successfully!");
	}
}
