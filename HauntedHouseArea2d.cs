using Godot;
using System;

public partial class HauntedHouseArea2d : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// I-connect ang input_event signal mula sa Area2D na ito
		Connect("input_event", new Callable(this, nameof(OnAreaInput)));
	}

	private void OnAreaInput(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
		{
			// Palitan ang "res://Dialogue2.tscn" ng gusto mong puntahang scene para sa School
			GetTree().ChangeSceneToFile("res://Dialogue7.tscn");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
