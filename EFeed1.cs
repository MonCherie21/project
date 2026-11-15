using Godot;
using System;

// All EFeed1..7 scenes use this script and the SAME saved candy/level state
// as the party inventory. Reopening a scene no longer grants five new candies.
public partial class EFeed1 : Node2D
{
    // -1 detects the EFeed scene number; override with 0..6 in the Inspector.
    [Export] public int YokaiIndex = -1;

    private TextureButton _feedButton;
    private Control _candyCount;
    private Control _friendshipText;
    private Range _friendshipBar;
    private CanvasItem _portrait;
    private Tween _flashTween;
    private int _index;

    public override void _Ready()
    {
        _index = ResolveIndex();
        _feedButton = GetNodeOrNull<TextureButton>("CanvasLayer/CandyFeed");
        _candyCount = GetNodeOrNull<Control>("CanvasLayer/CandyCount");
        _friendshipBar = GetNodeOrNull<Range>("friendshipLvl_bar");
        // The old scenes consistently spell this node "frienshipLvl".
        _friendshipText = GetNodeOrNull<Control>("friendshipLvl_bar/frienshipLvl");
        _portrait = GetNodeOrNull<CanvasItem>("M1Front");

        if (_candyCount is TextEdit candyText)
            candyText.Editable = false;
        if (_friendshipText is TextEdit friendshipText)
            friendshipText.Editable = false;
        if (_friendshipBar != null)
        {
            _friendshipBar.MinValue = 0;
            _friendshipBar.MaxValue = 100;
        }
        if (_feedButton != null)
            _feedButton.Pressed += OnFeedPressed;

        CandyInventory.Changed += UpdateUI;
        UpdateUI();
    }

    public override void _ExitTree()
    {
        CandyInventory.Changed -= UpdateUI;
        if (GodotObject.IsInstanceValid(_flashTween))
            _flashTween.Kill();
    }

    private int ResolveIndex()
    {
        if (YokaiIndex >= 0 && YokaiIndex < CandyInventory.PartnerCount)
            return YokaiIndex;
        string path = SceneFilePath;
        if (string.IsNullOrEmpty(path))
            path = GetTree().CurrentScene?.SceneFilePath ?? "";
        string filename = System.IO.Path.GetFileNameWithoutExtension(path);
        if (filename.StartsWith("EFeed", StringComparison.OrdinalIgnoreCase) &&
            int.TryParse(filename.Substring(5), out int number))
            return Math.Clamp(number - 1, 0, CandyInventory.PartnerCount - 1);
        return 0;
    }

    private static void PutText(Control control, string text)
    {
        if (control is Label label)
            label.Text = text;
        else if (control is TextEdit edit)
            edit.Text = text;
    }

    private void UpdateUI()
    {
        PutText(_candyCount, $"x{CandyInventory.Candies:00}");
        PutText(_friendshipText, $"Level {CandyInventory.FriendshipLevel(_index):00}");
        if (_friendshipBar != null)
            _friendshipBar.Value = CandyInventory.FriendshipProgress(_index);
        if (_feedButton != null)
            _feedButton.Disabled = CandyInventory.Candies == 0 ||
                CandyInventory.FriendshipLevel(_index) == CandyInventory.MaxFriendshipLevel ||
                _index >= CandyInventory.AvailablePartners;
    }

    private void OnFeedPressed()
    {
        if (!CandyInventory.TryFeed(_index))
            return;
        if (_portrait == null)
            return;
        if (GodotObject.IsInstanceValid(_flashTween))
            _flashTween.Kill();
        Color original = _portrait.Modulate;
        _portrait.Modulate = new Color("8cff99");
        _flashTween = CreateTween();
        _flashTween.TweenProperty(_portrait, "modulate", original, 0.6);
    }
}
