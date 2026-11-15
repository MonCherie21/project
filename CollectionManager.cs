using Godot;
using System;
using System.Collections.Generic;

// The seven collection portraits correspond to the seven story battles.
// Finishing level N reveals portraits 1..N; the count is read from the same
// Supabase save-slot JSON used by Continue, so no second progress table is needed.
public partial class CollectionManager : Control
{
    private TextureRect[] _characters;
    private Texture2D[] _originalPortraits;
    private int _currentIndex;
    private int _unlockedCount;
    private Label _counterLabel;
    private Label _nameLabel;
    private Label _hpLabel;
    private Label _attackLabel;
    private Label _levelLabel;
    private Label _rsLevelLabel;
    private Label _hint;

    [Export] private string[] _characterNames =
    {
        "Yo-kai 1", "Yo-kai 2", "Yo-kai 3", "Yo-kai 4",
        "Yo-kai 5", "Yo-kai 6", "Yo-kai 7"
    };
    [Export] private int[] _hpValues = { 100, 120, 90, 150, 110, 130, 200 };
    [Export] private int[] _attackValues = { 20, 25, 30, 15, 22, 28, 35 };
    [Export] private int[] _levelValues = { 1, 1, 1, 1, 1, 1, 1 };
    [Export] private int[] _rsLevelValues = { 1, 2, 1, 3, 2, 1, 4 };

    public override async void _Ready()
    {
        _characters = new TextureRect[]
        {
            GetNode<TextureRect>("Character 1"), GetNode<TextureRect>("Character 2"),
            GetNode<TextureRect>("Character 3"), GetNode<TextureRect>("Character 4"),
            GetNode<TextureRect>("Character 5"), GetNode<TextureRect>("Character 6"),
            GetNode<TextureRect>("Character 7")
        };
        _originalPortraits = new Texture2D[_characters.Length];
        for (int i = 0; i < _characters.Length; i++)
            _originalPortraits[i] = _characters[i].Texture;

        _counterLabel = GetNodeOrNull<Label>("CounterLabel");
        _nameLabel = GetNodeOrNull<Label>("NameLabel");
        _hpLabel = GetNodeOrNull<Label>("HpLabel");
        _attackLabel = GetNodeOrNull<Label>("AttackLabel");
        _levelLabel = GetNodeOrNull<Label>("LevelLabel");
        _rsLevelLabel = GetNodeOrNull<Label>("RsLevelLabel");

        GetNode<Button>("Button2").Pressed += Previous;
        GetNode<Button>("Button").Pressed += Next;
        GetNode<Button>("Button3").Pressed += () =>
            GetTree().ChangeSceneToFile("res://main_menu.tscn");

        _hint = new Label
        {
            Position = new Vector2(868, 367),
            Size = new Vector2(322, 84),
            AutowrapMode = TextServer.AutowrapMode.WordSmart
        };
        _hint.AddThemeFontSizeOverride("font_size", 17);
        AddChild(_hint);

        // A profile switch must never display the previous account's Yokai.
        _unlockedCount = PlayerData.ActiveAccountSlot == 0
            ? 0 : Math.Clamp(GameProgress.CompletedLevels, 0, _characters.Length);
        _hint.Text = "Defeat a Yo-kai in a story battle to reveal it.";
        UpdateDisplay();

        if (!PlayerData.HasLocalSession)
            return;

        // Main-menu Collection can be opened before choosing Continue. Look
        // up the saved slot's progress instead of showing a fresh empty list.
        try
        {
            List<GameSaveSlot> slots = await SaveLoadService.ListAsync(this);
            if (!GodotObject.IsInstanceValid(this))
                return;

            GameSaveSlot chosen = null;
            foreach (GameSaveSlot saved in slots)
            {
                if (saved.Number == GameProgress.ActiveSlot)
                {
                    chosen = saved;
                    break;
                }
                if (chosen == null || saved.SavedAt > chosen.SavedAt)
                    chosen = saved;
            }

            if (chosen != null)
            {
                _unlockedCount = Math.Clamp(chosen.Data.CompletedLevels, 0, _characters.Length);
                _hint.Text = "Collection for Account Slot " + PlayerData.ActiveAccountSlot +
                             " (" + _unlockedCount + "/7 discovered).";
                UpdateDisplay();
            }
            else
            {
                _unlockedCount = 0;
                UpdateDisplay();
            }
        }
        catch (Exception e)
        {
            GD.PushWarning("Collection could not sync with saved progress: " + e.Message);
            if (GodotObject.IsInstanceValid(this))
                _hint.Text = "Offline: showing progress from this session.";
        }
    }

    private void UpdateDisplay()
    {
        bool unlocked = _currentIndex < _unlockedCount;
        for (int i = 0; i < _characters.Length; i++)
        {
            _characters[i].Visible = i == _currentIndex;
            if (i == _currentIndex)
                _characters[i].Texture = unlocked
                    ? _originalPortraits[i] : SilhouetteFactory.Get(_originalPortraits[i]);
        }

        if (_counterLabel != null)
            _counterLabel.Text = (_currentIndex + 1) + " / " + _characters.Length;
        if (_nameLabel != null)
            _nameLabel.Text = unlocked ? GetName(_currentIndex) : "???";
        if (_hpLabel != null)
            _hpLabel.Text = "HP: " + (unlocked ? GetValue(_hpValues).ToString() : "???");
        if (_attackLabel != null)
            _attackLabel.Text = "Attack: " + (unlocked ? GetValue(_attackValues).ToString() : "???");
        if (_levelLabel != null)
            _levelLabel.Text = "Level: " + (unlocked ? GetValue(_levelValues).ToString() : "???");
        if (_rsLevelLabel != null)
            _rsLevelLabel.Text = "RS Level: " + (unlocked ? GetValue(_rsLevelValues).ToString() : "???");
    }

    private string GetName(int index) => index < _characterNames.Length
        ? _characterNames[index] : "Yo-kai " + (index + 1);
    private int GetValue(int[] values) => _currentIndex < values.Length ? values[_currentIndex] : 0;

    private void Previous()
    {
        _currentIndex = (_currentIndex + _characters.Length - 1) % _characters.Length;
        UpdateDisplay();
    }

    private void Next()
    {
        _currentIndex = (_currentIndex + 1) % _characters.Length;
        UpdateDisplay();
    }
}
