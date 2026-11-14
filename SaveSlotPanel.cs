using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// A small reusable overlay. It doesn't replace the current scene or reset gameplay.
public partial class SaveSlotPanel : CanvasLayer
{
	public enum SlotMode { Save, Load }

	private SlotMode _mode;
	private readonly Dictionary<int, GameSaveSlot> _existing = new Dictionary<int, GameSaveSlot>();
	private readonly Dictionary<int, Button> _buttons = new Dictionary<int, Button>();
	private Label _status = null!;
	private Button _close = null!;
	private bool _working;

	public static void Open(Node caller, SlotMode mode)
	{
		var root = caller.GetTree().Root;
		if (root.GetNodeOrNull<SaveSlotPanel>("SaveSlotPanel") != null)
			return;

		var menu = new SaveSlotPanel
		{
			Name = "SaveSlotPanel",
			Layer = 100,
			ProcessMode = ProcessModeEnum.Always,
			_mode = mode
		};
		root.AddChild(menu);
	}

	public override async void _Ready()
	{
		CreateInterface();
		await RefreshAsync();
	}

	private void CreateInterface()
	{
		var backdrop = new ColorRect
		{
			Color = new Color(0.03f, 0.04f, 0.09f, 0.85f),
			MouseFilter = Control.MouseFilterEnum.Stop,
			AnchorRight = 1,
			AnchorBottom = 1
		};
		AddChild(backdrop);

		var panel = new PanelContainer
		{
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			AnchorTop = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -290,
			OffsetRight = 290,
			OffsetTop = -210,
			OffsetBottom = 210
		};
		backdrop.AddChild(panel);

		var margin = new MarginContainer();
		margin.AddThemeConstantOverride("margin_left", 22);
		margin.AddThemeConstantOverride("margin_right", 22);
		margin.AddThemeConstantOverride("margin_top", 22);
		margin.AddThemeConstantOverride("margin_bottom", 22);
		panel.AddChild(margin);

		var content = new VBoxContainer();
		content.AddThemeConstantOverride("separation", 12);
		margin.AddChild(content);

		content.AddChild(new Label
		{
			Text = _mode == SlotMode.Save ? "SAVE YOUR GAME" : "LOAD YOUR GAME",
			HorizontalAlignment = HorizontalAlignment.Center
		});

		for (int i = 1; i <= 3; i++)
		{
			int slot = i;
			var button = new Button
			{
				Text = "Slot " + slot + " — Checking...",
				CustomMinimumSize = new Vector2(480, 54),
				Disabled = true
			};
			button.Pressed += () => _ = OnSlotPressedAsync(slot);
			_buttons[slot] = button;
			content.AddChild(button);
		}

		_status = new Label
		{
			Text = "Checking your saved games...",
			AutowrapMode = TextServer.AutowrapMode.WordSmart
		};
		content.AddChild(_status);

		_close = new Button { Text = "BACK", CustomMinimumSize = new Vector2(120, 42) };
		_close.Pressed += () => { if (!_working) QueueFree(); };
		content.AddChild(_close);
	}

	private void SetWorking(bool busy)
	{
		_working = busy;
		_close.Disabled = busy;
		foreach (var button in _buttons.Values)
			button.Disabled = busy;
	}

	private async Task RefreshAsync()
	{
		SetWorking(true);
		try
		{
			_existing.Clear();
			foreach (var save in await SaveLoadService.ListAsync(this))
				_existing[save.Number] = save;

			foreach (var entry in _buttons)
			{
				int slot = entry.Key;
				Button button = entry.Value;
				if (_existing.TryGetValue(slot, out GameSaveSlot existing))
				{
					button.Text = "SLOT " + slot + "  —  " +
						existing.Data.CompletedLevels + "/7 cleared — " +
						existing.SavedAt.ToLocalTime().ToString("MMM d, yyyy  h:mm tt");
					button.Disabled = false;
				}
				else
				{
					button.Text = "SLOT " + slot + "  —  EMPTY";
					button.Disabled = _mode == SlotMode.Load;
				}
			}
			_status.Text = "Player: " + PlayerData.PlayerName;
		}
		catch (Exception e)
		{
			_status.Text = e.Message;
			foreach (var button in _buttons.Values)
				button.Disabled = true;
		}
		finally
		{
			_working = false;
			_close.Disabled = false;
		}
	}

	private async Task OnSlotPressedAsync(int slot)
	{
		if (_working)
			return;

		if (_mode == SlotMode.Load)
		{
			try
			{
				if (!_existing.TryGetValue(slot, out GameSaveSlot selected))
					return;

				SaveLoadService.PrepareLoad(selected);
				GetNode<SceneManager>("/root/SceneManager").ResumeFromPause();
				Error error = GetTree().ChangeSceneToFile("res://world.tscn");
				if (error != Error.Ok)
				{
					_status.Text = "Could not open the world: " + error;
					return;
				}
				QueueFree();
			}
			catch (Exception e)
			{
				_status.Text = e.Message;
			}
			return;
		}

		if (_existing.ContainsKey(slot))
		{
			var dialog = new ConfirmationDialog
			{
				Title = "Overwrite save?",
				DialogText = "Replace the progress in Slot " + slot + "?"
			};
			AddChild(dialog);
			dialog.Confirmed += () => _ = SaveAsync(slot);
			dialog.CloseRequested += dialog.QueueFree;
			dialog.Confirmed += dialog.QueueFree;
			dialog.PopupCentered();
			return;
		}

		await SaveAsync(slot);
	}

	private async Task SaveAsync(int slot)
	{
		SetWorking(true);
		_status.Text = "Saving slot " + slot + "...";
		try
		{
			GameSaveData snapshot = GameSaveData.Capture();
			await SaveLoadService.SaveAsync(this, slot, snapshot);
			GameProgress.SetActiveSlot(slot);
			GameProgress.MarkSaved(snapshot.CompletedLevels);
			await RefreshAsync();
			_status.Text = "Saved slot " + slot + " successfully!";
			_ = ClearSuccessMessageAsync(_status.Text);
		}
		catch (Exception e)
		{
			_status.Text = e.Message;
		}
		finally
		{
			SetWorking(false);
		}
	}

	private async Task ClearSuccessMessageAsync(string successText)
	{
		await ToSignal(GetTree().CreateTimer(3.0, processAlways: true),
			SceneTreeTimer.SignalName.Timeout);
		if (GodotObject.IsInstanceValid(this) && _status.Text == successText)
			_status.Text = "Player: " + PlayerData.PlayerName;
	}
}
