using Godot;

public partial class Dialogue1 : Node
{
	public override void _Ready()
	{
		GetTree().Paused = false;
		GD.Print("Dialogue1 READY!");
		
	
	}

	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Enter))
		{
			GD.Print("ENTER IS BEING PRESSED!");
		}

		if (Input.IsActionJustPressed("ui_accept"))
		{
			GD.Print("UI_ACCEPT DETECTED!");

			GetTree().ChangeSceneToFile(
                "res://BattleScene.tscn"
			);
		}
	}
}
