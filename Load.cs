using Godot;
using System;

public partial class Load : Node2D
{
	public override void _Ready()
	{
		var backButton = GetNode<Button>("Button Back");
		backButton.Pressed += OnBackButtonPressed;
		SaveSlotPanel.Open(this, SaveSlotPanel.SlotMode.Load);
	}
	
	public void OnBackButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://Escape.tscn");
	}
}
