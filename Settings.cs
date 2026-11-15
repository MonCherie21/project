using Godot;

public partial class Settings : Node2D
{
	private TextureButton _manualButton;
	private Control _manualPopup;
	private TextureButton _closeButton;
	private TextureButton _closeSettingsButton;

	public override void _Ready()
	{
		_manualButton = GetNode<TextureButton>("ManualButton");
		_manualPopup = GetNode<Control>("ManualPopup");
		_closeButton = GetNode<TextureButton>("ManualPopup/CloseButton");
		_closeSettingsButton = GetNode<TextureButton>("CloseSettingsButton");

		_manualPopup.Hide();

		_manualButton.Pressed += OnManualButtonPressed;
		_closeButton.Pressed += OnCloseButtonPressed;
		_closeSettingsButton.Pressed += OnCloseSettingsButtonPressed;
	}

	private void OnManualButtonPressed()
	{
		_manualPopup.Show();
	}

	private void OnCloseButtonPressed()
	{
		_manualPopup.Hide();
	}

	private void OnCloseSettingsButtonPressed()
	{
		// Closes the entire Settings overlay
		QueueFree();

		// If you opened Settings via ChangeSceneToFile instead of AddChild,
		// replace QueueFree() above with:
		// GetTree().ChangeSceneToFile("res://MainMenu.tscn");
	}
}
