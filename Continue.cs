using Godot;
using System;

public partial class Continue : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var button = GetNode<Button>("Button");
		
		// I-connect ang pressed signal sa iyong method
		button.Pressed += BackMenu;
	}
	
	private void BackMenu()
	{
		// Palitan ang path ng tscn file na nais mong puntahan
		GetTree().ChangeSceneToFile("res://main_menu.tscn");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	
}
