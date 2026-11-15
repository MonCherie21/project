using Godot;

public partial class Settings : Node2D
{
	private TextureButton _manualButton;
	private Control _manualPopup;
	private TextureButton _closeButton;

	public override void _Ready()
	{
		_manualPopup = GetNode<Control>("ManualPopup");
		_closeButton = GetNode<TextureButton>("CloseButton");
		_manualButton = GetNode<TextureButton>("ManualButton");

		_manualPopup.Hide();
		_closeButton.Hide();

		_manualButton.Pressed += OnManualButtonPressed;
		_closeButton.Pressed += OnCloseButtonPressed;
	}

	private void OnManualButtonPressed()
	{
		_manualPopup.Show();
		_closeButton.Show();
	}

	private void OnCloseButtonPressed()
	{
		_manualPopup.Hide();
		_closeButton.Hide();
	}
}
