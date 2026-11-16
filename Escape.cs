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
		GetNode<Button>("Button Settings").Pressed += () =>
			GetNode<SceneManager>("/root/SceneManager").ShowSettings();
		YokaiTheme.StyleButton(GetNode<Button>("Button Settings"));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	// Ito ang gagana kapag pinindot ang Exit button
	private void OnExitButtonPressed()
	{
		TutorialOverlay.Confirm(this, "LEAVE YOUR ADVENTURE?",
			"Return to the main menu?\n\n" +
			"Any progress since your last successful save will be lost. " +
			"Choose CANCEL if you need to save first.",
			() => GetNode<SceneManager>("/root/SceneManager").ExitToMainMenu(),
			"LEAVE GAME");
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
			TutorialOverlay.Show(this, "SAVE UNAVAILABLE",
				"Finish or leave the battle before saving your progress.");
			return;
		}

		SaveSlotPanel.Open(this, SaveSlotPanel.SlotMode.Save);
	}
}
