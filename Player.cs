using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 150.0f;
	[Export] public Texture2D FrontTexture;
	[Export] public Texture2D BackTexture;

	private Sprite2D _sprite;
	private Button _myButton;
	private Vector2 _restPosition;
	private Vector2 _restScale;
	private float _walkTime;
	private float _idleTime;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");
		_restPosition = _sprite.Position;
		_restScale = _sprite.Scale;

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

		// The artwork currently has front/back poses only. Keep the facing
		// consistent when walking sideways, and mirror the front pose for left.
		if (Mathf.Abs(direction.Y) > Mathf.Abs(direction.X))
		{
			if (direction.Y < 0 && BackTexture != null)
				_sprite.Texture = BackTexture;
			else if (direction.Y > 0 && FrontTexture != null)
				_sprite.Texture = FrontTexture;
			_sprite.FlipH = false;
		}
		else if (Mathf.Abs(direction.X) > 0.05f)
		{
			if (FrontTexture != null)
				_sprite.Texture = FrontTexture;
			_sprite.FlipH = direction.X < 0;
		}

		float seconds = (float)delta;
		if (direction.LengthSquared() > 0.01f)
		{
			// Alternating step/lean and a tiny hop suggest a walk cycle without
			// inventing frames or changing the player's physics footprint.
			_walkTime += seconds * 11.0f;
			float step = Mathf.Sin(_walkTime);
			float hop = Mathf.Abs(step);
			_sprite.Position = _restPosition + new Vector2(0, -2.6f * hop);
			_sprite.Rotation = step * 0.035f;
			_sprite.Scale = new Vector2(
				_restScale.X * (1.0f + 0.025f * step),
				_restScale.Y * (1.0f - 0.018f * hop));
		}
		else
		{
			// Ease smoothly back into a quiet breathing idle pose.
			_idleTime += seconds * 2.2f;
			float easing = Mathf.Min(1.0f, seconds * 12.0f);
			_sprite.Position = _sprite.Position.Lerp(
				_restPosition + new Vector2(0, Mathf.Sin(_idleTime) * 0.4f), easing);
			_sprite.Rotation = Mathf.Lerp(_sprite.Rotation, 0.0f, easing);
			_sprite.Scale = _sprite.Scale.Lerp(
				_restScale * (1.0f + 0.008f * Mathf.Sin(_idleTime)), easing);
		}
	}

	private void OnButtonPressed()
	{
		GetNode<SceneManager>("/root/SceneManager").ShowPause();
	}
}
