using Godot;
using System;
using System.Text.Json;
using System.Threading.Tasks;

public partial class World : Node2D
{
    public override void _Ready()
    {
        GD.Print("World loaded!");
        GD.Print("Selected gender: " + PlayerData.Gender);

        string scenePath = PlayerData.Gender == "Male" ? "res://player_2.tscn"
                         : PlayerData.Gender == "Female" ? "res://player.tscn" : "";
        if (scenePath.Length == 0)
        {
            GD.PrintErr("No valid gender selected!");
            return;
        }

        PackedScene playerScene = GD.Load<PackedScene>(scenePath);
        if (playerScene == null)
        {
            GD.PrintErr("Could not load: " + scenePath);
            return;
        }

        Node2D player = playerScene.Instantiate<Node2D>();
        GetNode<Node2D>("../MapBackground").AddChild(player);
        player.Position = PlayerData.HasLoadedPosition
            ? PlayerData.WorldPosition
            : new Vector2(-193, -405);
        PlayerData.WorldPosition = player.Position;
        PlayerData.HasLoadedPosition = false;
        UpdateStoryFlags();
        ApplyBuildingDisturbances();

        // Added programmatically, so the existing world.tscn is untouched.
        var overlay = new CanvasLayer();
        AddChild(overlay);
        var nameLabel = new Label { Position = new Vector2(15, 15),
                                    Text = "Loading player profile..." };
        overlay.AddChild(nameLabel);

        // One quick checkpoint for the selected account (not a new player).
        var saveButton = new Button
        {
            Text = "QUICK SAVE",
            Position = new Vector2(175, 48),
            Size = new Vector2(145, 42),
            TooltipText = "Update the current account's saved progress"
        };
        saveButton.Pressed += () => _ = QuickSaveSlotOneAsync(saveButton, overlay);
        overlay.AddChild(saveButton);

        // The journal replaces the oversized progress banner. The active quest
        // stays one click away, without covering Wawa's roads and buildings.
        var journalButton = new Button
        {
            Position = new Vector2(14, 157),
            Size = new Vector2(66, 66),
            Flat = true,
            TooltipText = "Open the objective clipboard"
        };
        overlay.AddChild(journalButton);
        journalButton.AddChild(new ObjectiveClipboardIcon
        {
            Size = new Vector2(60, 64),
            MouseFilter = Control.MouseFilterEnum.Ignore
        });

        var notice = new Label
        {
            Position = new Vector2(48, -9),
            Text = GameProgress.CompletedLevels == GameProgress.TotalLevels ? "★" : "!",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        notice.AddThemeColorOverride("font_color", Colors.Gold);
        notice.AddThemeColorOverride("font_outline_color", Colors.Black);
        notice.AddThemeConstantOverride("outline_size", 5);
        notice.AddThemeFontSizeOverride("font_size", 23);
        journalButton.AddChild(notice);

        journalButton.Pressed += () =>
        {
            var journal = new AcceptDialog
            {
                Title = "Wawa Field Notes",
                DialogText = GameProgress.CompletedLevels == GameProgress.TotalLevels
                    ? "All seven stages are cleared!\nYou have protected Wawa."
                    : "STORY PROGRESS  " + GameProgress.CompletedLevels + "/" + GameProgress.TotalLevels +
                      "\n\nCURRENT OBJECTIVE\nGo to " + GameProgress.NextBuilding +
                      " and complete the Yo-kai battle.\n\nFollow the white flag on the map."
            };
            overlay.AddChild(journal);
            journal.Confirmed += journal.QueueFree;
            journal.CloseRequested += journal.QueueFree;
            journal.PopupCentered(new Vector2I(430, 235));
        };

        if (GameProgress.PendingAutoSave)
            _ = AutoSaveProgressAsync(overlay);

        if (PlayerData.PendingMovementTutorial)
        {
            PlayerData.PendingMovementTutorial = false;
            TutorialOverlay.Show(this, "WELCOME TO WAWA!",
                "Use W, A, S, D or the arrow keys to walk around.\n\n" +
                "Tap the clipboard with the yellow ! to check your next objective.\n" +
                "Look for the white flag to find your next battle.");
        }

        if (PlayerData.PlayerId <= 0 || PlayerData.AccessToken.Length == 0)
        {
            nameLabel.Text = "No registered player.";
            return;
        }

        var request = new HttpRequest { Timeout = 15.0 };
        AddChild(request);
        request.RequestCompleted += (result, code, headers, body) =>
        {
            request.QueueFree();
            if (result != (long)HttpRequest.Result.Success || code < 200 || code >= 300)
            {
                nameLabel.Text = "Unable to load profile.";
                GD.PrintErr("Supabase profile request failed (HTTP " + code + ").");
                return;
            }

            try
            {
                using var json = JsonDocument.Parse(body);
                if (json.RootElement.GetArrayLength() == 0)
                {
                    nameLabel.Text = "Profile not found.";
                    return;
                }

                var row = json.RootElement[0];
                PlayerData.PlayerName = row.GetProperty("player_name").GetString() ?? "";
                PlayerData.Gender = row.GetProperty("gender").GetString() ?? "";
                nameLabel.Text = "Player: " + PlayerData.PlayerName;
                GD.Print("Retrieved persistent profile: " + PlayerData.PlayerName);
            }
            catch (Exception ex)
            {
                nameLabel.Text = "Invalid profile response.";
                GD.PrintErr("Profile parsing failed: " + ex.Message);
            }
        };

        string url = SupabaseConfig.DataUrl +
            "/players?select=player_id,player_name,gender&player_id=eq." +
            PlayerData.PlayerId + "&limit=1";
        var error = request.Request(url,
            new[] { "apikey: " + SupabaseConfig.PublishableKey,
                    "Authorization: Bearer " + PlayerData.AccessToken },
            HttpClient.Method.Get);
        if (error != Error.Ok)
        {
            request.QueueFree();
            nameLabel.Text = "Unable to request profile.";
            GD.PrintErr("Profile request could not start: " + error);
        }
    }

    private void UpdateStoryFlags()
    {
        string[] flagNames =
        {
            "House Flag", "Sari-Sari Store Flag", "Gas Station Flag",
            "Pure Gold Flag", "McDo Flag", "School Flag", "Abandoned House Flag"
        };

        for (int i = 0; i < flagNames.Length; i++)
        {
            var flag = GetParent().GetNodeOrNull<TextureRect>(flagNames[i]);
            if (flag == null)
                continue;

            // Only the currently assigned building should be marked.
            flag.Visible = i == GameProgress.CompletedLevels;
            flag.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
    }

    private void ApplyBuildingDisturbances()
    {
        string[] places =
        {
            "House2", "SariSariStore", "GasStation",
            "Puregold", "MCDO", "School", "HauntedHouse"
        };
        for (int level = 0; level < places.Length; level++)
        {
            // Completed locations return to their original tidy artwork.
            if (level < GameProgress.CompletedLevels) continue;

            var sprite = GetParent().GetNodeOrNull<Sprite2D>(
                "MapBackground/Buildings/" + places[level]);
            if (sprite == null || sprite.Texture == null) continue;
            sprite.AddChild(new DisturbedBuildingEffect
            {
                Name = "YokaiDisturbance",
                BuildingSize = sprite.RegionEnabled
                    ? sprite.RegionRect.Size : sprite.Texture.GetSize(),
                Variant = level + 1
            });
        }
    }

    private async Task AutoSaveProgressAsync(CanvasLayer overlay)
    {
        GameSaveData snapshot = GameSaveData.Capture();
        const int slot = 1;
        try
        {
            await SaveLoadService.SaveAsync(this, slot, snapshot);
            GameProgress.SetActiveSlot(slot);
            GameProgress.MarkSaved(snapshot.CompletedLevels);
            if (GodotObject.IsInstanceValid(overlay))
                _ = ShowTimedStatusAsync(overlay, "Adventure saved ✓");
            GD.Print("Account " + PlayerData.ActiveAccountSlot + " progress saved.");
        }
        catch (Exception ex)
        {
            GD.PushWarning("Progress autosave failed. Save manually to retry: " + ex.Message);
            if (GodotObject.IsInstanceValid(overlay))
                _ = ShowTimedStatusAsync(overlay, "Save pending — use SAVE GAME to retry");
        }
    }

    private async Task QuickSaveSlotOneAsync(Button button, CanvasLayer overlay)
    {
        if (button.Disabled)
            return;

        button.Disabled = true;
        button.Text = "SAVING...";
        try
        {
            GameSaveData snapshot = GameSaveData.Capture();
            await SaveLoadService.SaveAsync(this, 1, snapshot);
            GameProgress.SetActiveSlot(1);
            GameProgress.MarkSaved(snapshot.CompletedLevels);
            if (GodotObject.IsInstanceValid(overlay))
                _ = ShowTimedStatusAsync(overlay, "Adventure saved ✓", new Vector2(175, 100));
        }
        catch (Exception ex)
        {
            GD.PushWarning("Quick Save failed: " + ex.Message);
            if (GodotObject.IsInstanceValid(overlay))
                _ = ShowTimedStatusAsync(overlay, "Quick save failed — check Output", new Vector2(175, 100));
        }
        finally
        {
            if (GodotObject.IsInstanceValid(button))
            {
                button.Disabled = false;
                button.Text = "QUICK SAVE";
            }
        }
    }

    private async Task ShowTimedStatusAsync(CanvasLayer overlay, string message,
        Vector2? position = null)
    {
        var toast = new Label
        {
            Position = position ?? new Vector2(91, 176),
            Text = message,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        toast.AddThemeFontSizeOverride("font_size", 16);
        toast.AddThemeColorOverride("font_color", Colors.Gold);
        toast.AddThemeColorOverride("font_outline_color", new Color("14202b"));
        toast.AddThemeConstantOverride("outline_size", 5);
        overlay.AddChild(toast);
        await ToSignal(GetTree().CreateTimer(3.2, processAlways: true), SceneTreeTimer.SignalName.Timeout);
        if (GodotObject.IsInstanceValid(toast))
            toast.QueueFree();
    }
}
