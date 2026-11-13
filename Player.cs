using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 150.0f;
	[Export] public Texture2D FrontTexture;
	[Export] public Texture2D BackTexture;

	private Sprite2D _sprite;
	private Button _myButton;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");

		if (FrontTexture != null)
			_sprite.Texture = FrontTexture;

		// Kunin ang Button node na nasa loob ng Player
		_myButton = GetNode<Button>("Button");
		if (_myButton != null)
		{
			_myButton.Pressed += OnButtonPressed;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 direction = Input.GetVector(
			"move_left",
			"move_right",
			"move_up",
			"move_down"
		);

		Velocity = direction * Speed;
		MoveAndSlide();
		PlayerData.WorldPosition = Position;

		if (direction.Y < 0 && BackTexture != null)
		{
			_sprite.Texture = BackTexture;
		}
		else if (direction.Y > 0 && FrontTexture != null)
		{
			_sprite.Texture = FrontTexture;
		}
	}

	private void OnButtonPressed()
	{
		GetNode<SceneManager>("/root/SceneManager").ShowPause();
	}
}
