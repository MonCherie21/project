using Godot;
using System;
using System.Text.Json;

public partial class CharacterCreation : Control
{
    private string _gender = "";
    private string _pendingName = "";
    private string _pendingGender = "";
    private string _accessToken = "";
    private string _refreshToken = "";
    private Button _startButton;
    private Button _maleButton;
    private Button _femaleButton;
    private AcceptDialog _message;

    public override void _Ready()
    {
        _maleButton = GetNode<Button>("Male");
        _femaleButton = GetNode<Button>("Female");
        _maleButton.Pressed += () => SelectGender("Male");
        _femaleButton.Pressed += () => SelectGender("Female");
        GetNode<Button>("Button").Pressed += OnBackPressed; // Ronan's Back button

        _startButton = GetNode<Button>("Start");
        _startButton.Pressed += OnStartPressed;
        _message = new AcceptDialog { Title = "Character Creation" };
        AddChild(_message);
    }

    private void SelectGender(string gender)
    {
        _gender = gender;
        UpdateGenderButton(_maleButton, gender == "Male", "Male");
        UpdateGenderButton(_femaleButton, gender == "Female", "Female");
    }

    private static void UpdateGenderButton(Button button, bool selected, string caption)
    {
        // The unselected artwork/text is baked into MainMenu.png. Only draw
        // a new button label when covering that artwork with the gray state.
        button.Text = selected ? caption : "";
        StyleBox style = selected
            ? new StyleBoxFlat
            {
                BgColor = new Color(0.47f, 0.51f, 0.53f, 0.96f),
                BorderColor = Colors.Black,
                BorderWidthLeft = 3,
                BorderWidthTop = 3,
                BorderWidthRight = 3,
                BorderWidthBottom = 3,
                CornerRadiusTopLeft = 10,
                CornerRadiusTopRight = 10,
                CornerRadiusBottomLeft = 10,
                CornerRadiusBottomRight = 10
            }
            : new StyleBoxEmpty();
        foreach (string state in new[] { "normal", "hover", "pressed", "focus" })
            button.AddThemeStyleboxOverride(state, style);
        if (selected)
        {
            button.AddThemeFontOverride("font", GD.Load<Font>("res://Assets/PressStart2P-Regular.ttf"));
            button.AddThemeFontSizeOverride("font_size", 21);
            button.AddThemeColorOverride("font_color", new Color("191d25"));
            button.AddThemeColorOverride("font_hover_color", new Color("191d25"));
            button.AddThemeColorOverride("font_pressed_color", new Color("191d25"));
        }
    }

    private void OnStartPressed()
    {
        if (PlayerData.PendingNewAccountSlot == 0 ||
            PlayerData.GetAccounts().Exists(a => a.Slot == PlayerData.PendingNewAccountSlot))
        {
            ShowError("There is no free account slot. Return to the main menu.");
            return;
        }
        _pendingName = GetNode<LineEdit>("NameInput").Text.Trim();
        _pendingGender = _gender;

        if (_pendingName.Length is < 1 or > 50)
        {
            ShowError("Please enter a name between 1 and 50 characters.");
            return;
        }
        if (_pendingGender != "Male" && _pendingGender != "Female")
        {
            ShowError("Please select Male or Female.");
            return;
        }
        if (!SupabaseConfig.IsConfigured)
        {
            ShowError("The game cannot connect right now. Please try again later.");
            return;
        }

        _startButton.Disabled = true;
        // Every New Game creates a new anonymous identity. Continue/load comes later.
        var request = new HttpRequest { Timeout = 15.0 };
        AddChild(request);
        request.RequestCompleted += (result, code, headers, body) =>
        {
            request.QueueFree();
            if (result != (long)HttpRequest.Result.Success || code < 200 || code >= 300)
            {
                ShowError("Could not start your adventure. Check your connection and try again.");
                return;
            }

            try
            {
                using var json = JsonDocument.Parse(body);
                _accessToken = json.RootElement.GetProperty("access_token").GetString() ?? "";
                _refreshToken = json.RootElement.GetProperty("refresh_token").GetString() ?? "";
                if (_accessToken.Length == 0)
                    throw new Exception("No access token returned");
                RegisterPlayer();
            }
            catch (Exception ex)
            {
                GD.PrintErr("Authentication response error: " + ex.Message);
                ShowError("Could not sign in. Please try again.");
            }
        };

        var error = request.Request(SupabaseConfig.AuthUrl + "/signup",
            new[] { "apikey: " + SupabaseConfig.PublishableKey,
                    "Content-Type: application/json" },
            HttpClient.Method.Post, "{}");
        if (error != Error.Ok)
        {
            request.QueueFree();
            ShowError("Could not start registration: " + error);
        }
    }

    private void RegisterPlayer()
    {
        var request = new HttpRequest { Timeout = 15.0 };
        AddChild(request);
        request.RequestCompleted += (result, code, headers, body) =>
        {
            request.QueueFree();
            if (result != (long)HttpRequest.Result.Success || code < 200 || code >= 300)
            {
                GD.PrintErr("Supabase player registration failed (HTTP " + code + ").");
                ShowError("Couldn't save your character (HTTP " + code + ").");
                return;
            }

            try
            {
                using var json = JsonDocument.Parse(body);
                if (json.RootElement.GetArrayLength() < 1)
                    throw new Exception("No player returned");

                var row = json.RootElement[0];
                PlayerData.PlayerId = row.GetProperty("player_id").GetInt32();
                PlayerData.PlayerName = row.GetProperty("player_name").GetString() ?? "";
                PlayerData.Gender = row.GetProperty("gender").GetString() ?? "";
                PlayerData.AccessToken = _accessToken;
                PlayerData.RefreshToken = _refreshToken;
                PlayerData.WorldPosition = new Vector2(-193, -405);
                PlayerData.HasLoadedPosition = false;
                GameProgress.StartNewGame();
                PlayerData.PendingMovementTutorial = true;
                if (!PlayerData.RegisterNewAccount(PlayerData.PendingNewAccountSlot))
                {
                    ShowError("Could not secure the new account slot on this device. Please try again.");
                    return;
                }
                // Makes a newly created account continuable even if the
                // player hasn't finished the opening cutscene yet.
                GameProgress.QueueAutoSave();
                GetTree().ChangeSceneToFile("res://OpeningCutscene.tscn");
            }
            catch (Exception ex)
            {
                GD.PrintErr("Player response error: " + ex.Message);
                ShowError("The server returned invalid player information.");
            }
        };

        string payload = JsonSerializer.Serialize(new
        {
            player_name = _pendingName,
            gender = _pendingGender
        });
        var error = request.Request(
            SupabaseConfig.DataUrl + "/players?select=player_id,player_name,gender",
            new[] { "apikey: " + SupabaseConfig.PublishableKey,
                    "Authorization: Bearer " + _accessToken,
                    "Content-Type: application/json",
                    "Prefer: return=representation" },
            HttpClient.Method.Post, payload);
        if (error != Error.Ok)
        {
            request.QueueFree();
            ShowError("Could not send player data: " + error);
        }
    }

    private void ShowError(string message)
    {
        _startButton.Disabled = false;
        _message.DialogText = message;
        _message.PopupCentered();
    }

    private void OnBackPressed()
    {
        if (!_startButton.Disabled)
            GetTree().ChangeSceneToFile("res://main_menu.tscn");
    }
}
