using Godot;

// A CanvasLayer overlay works from both the main menu and the paused world.
// Opening settings never changes the scene or destroys the current game state.
public partial class Settings : CanvasLayer
{
	private Control _manualPopup;
	private Button _musicOnButton;
	private Button _musicOffButton;
	private HSlider _volumeSlider;
	private Label _volumeLabel;
	private Label _volumePercent;
	private MusicPlayer _music;

	public override void _Ready()
	{
		_manualPopup = GetNode<Control>("ManualPopup");
		_musicOnButton = GetNode<Button>("MusicOnButton");
		_musicOffButton = GetNode<Button>("MusicOffButton");
		_volumeSlider = GetNode<HSlider>("MusicVolumeSlider");
		_volumeLabel = GetNode<Label>("MusicVolumeLabel");
		_volumePercent = GetNode<Label>("MusicVolumePercent");
		_music = GetNodeOrNull<MusicPlayer>("/root/MusicPlayer");

		GetNode<Button>("ManualButton").Pressed += () => _manualPopup.Show();
		GetNode<Button>("ManualPopup/CloseButton").Pressed += () => _manualPopup.Hide();
		GetNode<Button>("CloseSettingsButton").Pressed += Close;
		_musicOnButton.Pressed += () => ChangeMusic(true);
		_musicOffButton.Pressed += () => ChangeMusic(false);
		_volumeSlider.Value = _music?.VolumePercent ?? 20;
		_volumeSlider.ValueChanged += value =>
		{
			_music?.SetVolumePercent(Mathf.RoundToInt((float)value));
			_volumePercent.Text = Mathf.RoundToInt((float)value) + "%";
		};
		_manualPopup.Hide();
		UpdateMusicButtons();
	}

	public override void _UnhandledKeyInput(InputEvent @event)
	{
		if (@event is not InputEventKey key || !key.Pressed || key.Echo || key.Keycode != Key.Escape)
			return;
		if (_manualPopup.Visible)
			_manualPopup.Hide();
		else
			Close();
		GetViewport().SetInputAsHandled();
	}

	private void ChangeMusic(bool enabled)
	{
		_music?.SetEnabled(enabled);
		UpdateMusicButtons();
	}

	private void UpdateMusicButtons()
	{
		bool enabled = _music?.Enabled ?? false;
		StyleMusicButton(_musicOnButton, enabled);
		StyleMusicButton(_musicOffButton, !enabled);
		_volumeSlider.Visible = enabled;
		_volumeLabel.Visible = enabled;
		_volumePercent.Visible = enabled;
		_volumePercent.Text = (_music?.VolumePercent ?? 20) + "%";
	}

	private static void StyleMusicButton(Button button, bool selected)
	{
		// Opaque buttons cover the two baked "ON" captions in the artwork.
		var box = new StyleBoxFlat
		{
			BgColor = selected ? new Color("3c5945") : new Color("23272e"),
			BorderColor = selected ? new Color("f8dc95") : new Color("939ba5")
		};
		box.SetBorderWidthAll(selected ? 3 : 2);
		box.SetCornerRadiusAll(4);
		button.AddThemeStyleboxOverride("normal", box);
		button.AddThemeColorOverride("font_color", selected ? new Color("fff3cc") : Colors.White);
	}

	private void Close()
	{
		GetNode<SceneManager>("/root/SceneManager").CloseSettings();
	}
}
