using Godot;
using System;

public partial class Escape : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Hanapin ang Exit Button at i-connect ang signal
		var exitButton = GetNode<Button>("Button Exit");
		exitButton.Pressed += OnExitButtonPressed;

		// Hanapin ang Load Button at i-connect ang signal
		// Palitan ang "Button Load" ng eksaktong pangalan ng node mo sa Scene tree kung kinakailangan
		var loadButton = GetNode<Button>("Button Load");
		loadButton.Pressed += OnLoadButtonPressed;

		var saveButton = GetNode<Button>("Button Save");
		saveButton.Pressed += () => SaveSlotPanel.Open(this, SaveSlotPanel.SlotMode.Save);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Ito ang gagana kapag pinindot ang Exit button
	private void OnExitButtonPressed()
	{
		ToScene("res://main_menu.tscn");
	}

	// Ito ang gagana kapag pinindot ang Load button
	private void OnLoadButtonPressed()
	{
		ToScene("res://Load.tscn");
	}

	private void ToScene(string scenePath)
	{
		Error err = GetTree().ChangeSceneToFile(scenePath);
		if (err != Error.Ok)
		{
			GD.PrintErr($"Hindi mabuksan ang scene: {scenePath}");
		}
	}
}
