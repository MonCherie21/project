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
		_manualPopup.AddThemeStyleboxOverride("panel", YokaiTheme.Panel(5));
		_manualPopup.AddChild(new YokaiRuneSeal
		{
			Position = new Vector2(849, 23),
			Size = new Vector2(44, 44)
		});
		GetNode<Label>("ManualPopup/ManualTitle").AddThemeColorOverride("font_color", YokaiTheme.BrassBright);
		GetNode<RichTextLabel>("ManualPopup/ManualText").AddThemeColorOverride("default_color", YokaiTheme.Parchment);
		YokaiTheme.StyleClose(GetNode<Button>("ManualPopup/CloseButton"));

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
		var box = YokaiTheme.Box(selected ? new Color("2c493b") : YokaiTheme.Surface,
			selected ? YokaiTheme.BrassBright : YokaiTheme.FrameEdge, selected ? 3 : 2);
		button.AddThemeStyleboxOverride("normal", box);
		button.AddThemeStyleboxOverride("hover", YokaiTheme.Box(new Color("385e49"), YokaiTheme.BrassBright));
		button.AddThemeColorOverride("font_color", selected ? YokaiTheme.BrassBright : YokaiTheme.Parchment);
	}

	private void Close()
	{
		GetNode<SceneManager>("/root/SceneManager").CloseSettings();
	}
}
