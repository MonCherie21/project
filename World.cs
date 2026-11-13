using Godot;
using System;
using System.Text.Json;

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
        player.Position = new Vector2(-193, -405);

        // Added programmatically, so the existing world.tscn is untouched.
        var overlay = new CanvasLayer();
        AddChild(overlay);
        var nameLabel = new Label { Position = new Vector2(15, 15),
                                    Text = "Loading player profile..." };
        overlay.AddChild(nameLabel);

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
}
