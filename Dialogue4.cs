using Godot;

public partial class Dialogue4 : Node
{
	// I-export ito para madali mong mapalitan sa Inspector kung anong scene ang susunod (hal. Dialogue2.tscn)
	[Export] public string NextScenePath { get; set; } = "res://BattleScene4.tscn";

	public override void _Ready()
	{
		GetTree().Paused = false;
		GD.Print($"{Name} Ready: Laro ay unpaused na.");
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_accept") || (@event is InputEventKey keyEvent && keyEvent.Pressed && (keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.KpEnter)))
		{
			GD.Print($"Enter key ay natanggap sa {Name}! Lumilipat sa: {NextScenePath}");
			
			Error err = GetTree().ChangeSceneToFile(NextScenePath);
			if (err != Error.Ok)
			{
				GD.PrintErr("May error sa paglipat ng eksena: ", err);
			}
		}
	}
}
