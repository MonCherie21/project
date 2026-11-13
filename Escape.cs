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
		saveButton.Pressed += OnSaveButtonPressed;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Ito ang gagana kapag pinindot ang Exit button
	private void OnExitButtonPressed()
	{
		GetNode<SceneManager>("/root/SceneManager").ExitToMainMenu();
	}

	// Ito ang gagana kapag pinindot ang Load button
	private void OnLoadButtonPressed()
	{
		SaveSlotPanel.Open(this, SaveSlotPanel.SlotMode.Load);
	}

	private void OnSaveButtonPressed()
	{
		// Battle state is not yet part of the save snapshot. Don't imply that
		// players can safely resume a half-finished fight from a save slot.
		if (GetTree().CurrentScene is BattleScene)
		{
			var message = new AcceptDialog
			{
				Title = "Save game",
				DialogText = "Finish or leave the battle before saving your progress."
			};
			AddChild(message);
			message.Confirmed += message.QueueFree;
			message.CloseRequested += message.QueueFree;
			message.PopupCentered();
			return;
		}

		SaveSlotPanel.Open(this, SaveSlotPanel.SlotMode.Save);
	}
}
