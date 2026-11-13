using Godot;
using System;
using System.Collections.Generic;

// The three save slots appear inside the original Continue.tscn LOAD MENU box.
public partial class Continue : Control
{
    private readonly Dictionary<int, GameSaveSlot> _slots = new();
    private readonly Dictionary<int, Button> _buttons = new();
    private Label _playerLabel;
    private Label _statusLabel;
    private bool _loading;

    public override async void _Ready()
    {
        GetNode<Button>("Button").Pressed += () =>
            GetTree().ChangeSceneToFile("res://main_menu.tscn");

        for (int i = 1; i <= 3; i++)
        {
            int slot = i;
            var button = new Button
            {
                Name = "SaveSlot" + slot,
                Position = new Vector2(310, 300 + (slot - 1) * 75),
                Size = new Vector2(660, 59),
                Text = "SLOT " + slot + " — CHECKING...",
                Disabled = true
            };
            button.Pressed += () => LoadSlot(slot);
            _buttons[slot] = button;
            AddChild(button);
        }

        _playerLabel = new Label
        {
            Position = new Vector2(350, 532),
            Size = new Vector2(580, 28),
            HorizontalAlignment = HorizontalAlignment.Center,
            Text = "PLAYER: Loading..."
        };
        AddChild(_playerLabel);

        _statusLabel = new Label
        {
            Position = new Vector2(320, 567),
            Size = new Vector2(640, 40),
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            Text = "Checking your saved games..."
        };
        AddChild(_statusLabel);

        try
        {
            var savedSlots = await SaveLoadService.ListAsync(this);
            if (!GodotObject.IsInstanceValid(this))
                return;

            foreach (var saved in savedSlots)
                _slots[saved.Number] = saved;

            foreach (var entry in _buttons)
            {
                int number = entry.Key;
                Button button = entry.Value;
                if (_slots.TryGetValue(number, out GameSaveSlot saved))
                {
                    button.Text = "SLOT " + number + " — " +
                        saved.SavedAt.ToLocalTime().ToString("MMM d, yyyy  h:mm tt");
                    button.Disabled = false;
                }
                else
                {
                    button.Text = "SLOT " + number + " — EMPTY";
                    button.Disabled = true;
                }
            }

            _playerLabel.Text = "PLAYER: " + PlayerData.PlayerName;
            _statusLabel.Text = savedSlots.Count == 0
                ? "No saves yet. Start a game and save first."
                : "Choose a slot to continue.";
        }
        catch (Exception e)
        {
            if (GodotObject.IsInstanceValid(this))
            {
                _playerLabel.Text = "PLAYER: Not connected";
                _statusLabel.Text = e.Message;
            }
        }
    }

    private void LoadSlot(int number)
    {
        if (_loading || !_slots.TryGetValue(number, out GameSaveSlot selected))
            return;

        try
        {
            _loading = true;
            SaveLoadService.PrepareLoad(selected);
            Error error = GetTree().ChangeSceneToFile("res://world.tscn");
            if (error != Error.Ok)
            {
                _statusLabel.Text = "Unable to load saved game: " + error;
                _loading = false;
            }
        }
        catch (Exception e)
        {
            _statusLabel.Text = e.Message;
            _loading = false;
        }
    }
}
