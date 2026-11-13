using Godot;

public partial class Area2d : Area2D
{
	public override void _Ready()
	{
		Connect("input_event", new Callable(this, nameof(OnAreaInput)));
	}
	private void OnAreaInput(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			GameProgress.EnterBuilding(this, 1, "res://Dialogue1.tscn");
		}
	}
}
