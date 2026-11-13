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

        // Added programmatically, so the existing world.tscn is untouched.
        var overlay = new CanvasLayer();
        AddChild(overlay);
        var nameLabel = new Label { Position = new Vector2(15, 15),
                                    Text = "Loading player profile..." };
        overlay.AddChild(nameLabel);

		// Keep the quick-save entry point clear of the player's return/pause arrow.
        var saveButton = new Button
        {
            Text = "SAVE GAME",
			Position = new Vector2(175, 48),
            Size = new Vector2(145, 42)
        };
        saveButton.Pressed += () => SaveSlotPanel.Open(this, SaveSlotPanel.SlotMode.Save);
        overlay.AddChild(saveButton);

        // Keep progression below the return arrow and make the next objective
        // easy to find without covering the map controls.
        var progressBackground = new ColorRect
        {
            Position = new Vector2(12, 151),
            Size = new Vector2(600, 49),
            Color = new Color(0.03f, 0.06f, 0.10f, 0.70f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        overlay.AddChild(progressBackground);

        var objectiveMarker = new Label
        {
            Position = new Vector2(13, 4),
            Text = GameProgress.CompletedLevels == GameProgress.TotalLevels ? "★" : "!",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        objectiveMarker.AddThemeColorOverride("font_color", Colors.Gold);
        objectiveMarker.AddThemeFontSizeOverride("font_size", 28);
        progressBackground.AddChild(objectiveMarker);

        var progressLabel = new Label
        {
            Position = new Vector2(49, 15),
            Size = new Vector2(535, 30),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Text = "CLEARED: " + GameProgress.CompletedLevels + "/" +
                   GameProgress.TotalLevels + "   NEXT: " + GameProgress.NextBuilding
        };
        progressBackground.AddChild(progressLabel);

        if (GameProgress.PendingAutoSave)
            _ = AutoSaveProgressAsync(progressLabel);

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

            flag.Visible = i >= GameProgress.CompletedLevels;
            flag.MouseFilter = Control.MouseFilterEnum.Ignore;
        }
    }

    private async Task AutoSaveProgressAsync(Label progressLabel)
    {
        GameSaveData snapshot = GameSaveData.Capture();
        int slot = GameProgress.ActiveSlot;
        try
        {
            await SaveLoadService.SaveAsync(this, slot, snapshot);
            GameProgress.MarkSaved(snapshot.CompletedLevels);
            if (GodotObject.IsInstanceValid(progressLabel))
                progressLabel.Text += "  (saved to Slot " + slot + ")";
            GD.Print("Level progress automatically saved to Slot " + slot + ".");
        }
        catch (Exception ex)
        {
            GD.PushWarning("Progress autosave failed. Save manually to retry: " + ex.Message);
            if (GodotObject.IsInstanceValid(progressLabel))
                progressLabel.Text += "  (save pending)";
        }
    }
}
