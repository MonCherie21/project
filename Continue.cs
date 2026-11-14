using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Three PLAYER accounts, each with its own saved game and progress.
// All controls sit inside the original LOAD MENU artwork.
public partial class Continue : Control
{
    private readonly Dictionary<int, Label> _names = new();
    private readonly Dictionary<int, Button> _play = new();
    private readonly Dictionary<int, Button> _delete = new();
    private Label _status;
    private bool _busy;

    public override void _Ready()
    {
        GetNode<Button>("Button").Pressed += () =>
        {
            if (!_busy) GetTree().ChangeSceneToFile("res://main_menu.tscn");
        };

        for (int slot = 1; slot <= 3; slot++)
            CreateAccountRow(slot);

        _status = new Label
        {
            Position = new Vector2(317, 561),
            Size = new Vector2(646, 52),
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _status.AddThemeColorOverride("font_color", new Color("f2dbab"));
        AddChild(_status);
        RefreshRows();
    }

    private void CreateAccountRow(int slot)
    {
        var row = new PanelContainer
        {
            Position = new Vector2(300, 286 + (slot - 1) * 81),
            Size = new Vector2(680, 68)
        };
        row.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("161f30ee"),
            BorderColor = new Color("78899e"),
            BorderWidthTop = 2, BorderWidthBottom = 2,
            BorderWidthLeft = 2, BorderWidthRight = 2,
            CornerRadiusTopLeft = 5, CornerRadiusTopRight = 5,
            CornerRadiusBottomLeft = 5, CornerRadiusBottomRight = 5
        });
        AddChild(row);

        var margin = new MarginContainer();
        margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_bottom", 8);
        row.AddChild(margin);

        var line = new HBoxContainer();
        line.AddThemeConstantOverride("separation", 12);
        margin.AddChild(line);

        var label = new Label
        {
            Text = "SLOT " + slot + " — EMPTY",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            VerticalAlignment = VerticalAlignment.Center
        };
        label.AddThemeColorOverride("font_color", new Color("f1e5c9"));
        label.AddThemeFontSizeOverride("font_size", 19);
        _names[slot] = label;
        line.AddChild(label);

        var play = MakeButton("PLAY");
        play.Pressed += () => _ = PlayAsync(slot);
        _play[slot] = play;
        line.AddChild(play);

        var delete = MakeButton("DELETE");
        delete.Pressed += () => ConfirmDelete(slot);
        _delete[slot] = delete;
        line.AddChild(delete);
    }

    private static Button MakeButton(string text)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(102, 44) };
        var style = new StyleBoxFlat
        {
            BgColor = new Color("303d51"),
            BorderColor = new Color("8e9dad"),
            BorderWidthTop = 2, BorderWidthBottom = 2,
            BorderWidthLeft = 2, BorderWidthRight = 2
        };
        var hover = (StyleBoxFlat)style.Duplicate();
        hover.BgColor = new Color("53647b");
        button.AddThemeStyleboxOverride("normal", style);
        button.AddThemeStyleboxOverride("hover", hover);
        button.AddThemeColorOverride("font_color", new Color("fff0ca"));
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        return button;
    }

    private void RefreshRows()
    {
        var accounts = PlayerData.GetAccounts();
        for (int slot = 1; slot <= 3; slot++)
        {
            var account = accounts.FirstOrDefault(a => a.Slot == slot);
            _names[slot].Text = account == null
                ? "SLOT " + slot + "   —   EMPTY"
                : "SLOT " + slot + "   —   " + account.PlayerName;
            _play[slot].Disabled = _busy || account == null;
            _delete[slot].Disabled = _busy || account == null;
        }
        if (!_busy && string.IsNullOrEmpty(_status.Text))
            _status.Text = accounts.Count == 0
                ? "No adventures yet. Select NEW GAME to begin."
                : "Select PLAY to continue, or DELETE to free an account slot.";
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        RefreshRows();
    }

    private async Task PlayAsync(int slot)
    {
        if (_busy || !PlayerData.SelectAccount(slot)) return;
        SetBusy(true);
        _status.Text = "Loading account " + slot + "...";
        try
        {
            var saves = await SaveLoadService.ListAsync(this);
            if (!GodotObject.IsInstanceValid(this)) return;
            // Old accounts may have used Save 2 or Save 3. Never strand them.
            var latest = saves.OrderByDescending(s => s.SavedAt).FirstOrDefault();
            if (latest == null)
            {
                GameProgress.StartNewGame();
                GameProgress.QueueAutoSave();
                PlayerData.HasLoadedPosition = false;
            }
            else
            {
                SaveLoadService.PrepareLoad(latest);
            }
            Error changed = GetTree().ChangeSceneToFile("res://world.tscn");
            if (changed != Error.Ok)
                throw new InvalidOperationException("Could not open the world: " + changed);
        }
        catch (Exception e)
        {
            if (GodotObject.IsInstanceValid(this))
                _status.Text = "Unable to load this adventure: " + e.Message;
        }
        finally
        {
            if (GodotObject.IsInstanceValid(this)) SetBusy(false);
        }
    }

    private void ConfirmDelete(int slot)
    {
        if (_busy) return;
        var account = PlayerData.GetAccounts().FirstOrDefault(a => a.Slot == slot);
        if (account == null) return;
        TutorialOverlay.Confirm(this, "DELETE ACCOUNT SLOT " + slot + "?",
            "Delete " + account.PlayerName + "'s saved adventure?\n\n" +
            "All progress in this slot will be lost. This cannot be undone.",
            () => _ = DeleteAsync(slot), "DELETE FOREVER");
    }

    private async Task DeleteAsync(int slot)
    {
        if (_busy || !PlayerData.SelectAccount(slot)) return;
        SetBusy(true);
        _status.Text = "Deleting Account Slot " + slot + "...";
        bool canOfferLocalOnly = false;
        try
        {
            // Do not forget the login until server progress is actually removed.
            if (!PlayerData.CanUpdateAccountSlots())
                throw new InvalidOperationException("Cannot update local account slots.");
            canOfferLocalOnly = true;
            await SaveLoadService.DeleteAllSavesAsync(this);
            if (!GodotObject.IsInstanceValid(this)) return;
            if (!PlayerData.RemoveAccount(slot))
                throw new InvalidOperationException("Unable to update the local account slots.");
            _status.Text = "Slot " + slot + " is now free. You can start another game.";
        }
        catch (Exception e)
        {
            if (GodotObject.IsInstanceValid(this))
            {
                _status.Text = "Cloud delete failed. Your account is still safe.";
                if (canOfferLocalOnly)
                    OfferLocalOnlyDelete(slot, e.Message);
                else
                    _status.Text = "Could not update account slots: " + e.Message;
            }
        }
        finally
        {
            if (GodotObject.IsInstanceValid(this)) SetBusy(false);
        }
    }

    private void OfferLocalOnlyDelete(int slot, string reason)
    {
        // If the project's DELETE policy has not been configured, the player
        // can still free a DEVICE slot, but only after a second clear warning.
        TutorialOverlay.Confirm(this, "CLOUD SAVE NOT REMOVED",
            "The online save could not be deleted.\n\n" +
            "Free this slot on this device anyway? The old online progress " +
            "may remain, but this game will no longer be able to access it.",
            () =>
            {
                if (PlayerData.RemoveAccount(slot))
                {
                    _status.Text = "Local Slot " + slot + " is free. Online data may still exist.";
                    RefreshRows();
                }
                else
                    _status.Text = "Could not remove the local account.";
            }, "FREE DEVICE SLOT");
        GD.PushWarning("Remote save deletion was refused: " + reason);
    }
}
