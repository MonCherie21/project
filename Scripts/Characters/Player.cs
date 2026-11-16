using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 150.0f;
	[Export] public Texture2D FrontTexture;
	[Export] public Texture2D BackTexture;
	[Export] public Texture2D SideTexture;

	// Twelve restrained pixel poses (up from eight), stepped instead of
	// interpolated to keep the sprite's outline sharp. No arm-raising gag.
	private static readonly float[] StepLift =
		{ 0f, 0.45f, 1.2f, 2f, 1.65f, 0.7f,
		  0f, 0.45f, 1.2f, 2f, 1.65f, 0.7f };
	private static readonly float[] StepLean =
		{ 0f, 0.005f, 0.011f, 0.014f, 0.009f, 0.003f,
		  0f, -0.005f, -0.011f, -0.014f, -0.009f, -0.003f };
	private static readonly float[] StepSway =
		{ 0f, 0.2f, 0.45f, 0.6f, 0.38f, 0.12f,
		  0f, -0.2f, -0.45f, -0.6f, -0.38f, -0.12f };
	private static readonly float[] StepStretch =
		{ 0f, 0.003f, 0.005f, 0.007f, 0.004f, 0.002f,
		  0f, 0.003f, 0.005f, 0.007f, 0.004f, 0.002f };

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
		_sprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;

		// Kunin ang Button node na nasa loob ng Player
		_myButton = GetNodeOrNull<Button>("Button");
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

		Vector2 beforeMove = GlobalPosition;
		Velocity = direction * Speed;
		MoveAndSlide();
		PlayerData.WorldPosition = Position;

		// Explicit direction priority: ALL up-diagonals use the back pose.
		// Left/right use a genuine profile sprite, mirrored only horizontally.
		if (direction.Y < -0.20f)
		{
			SetFacingTexture(BackTexture, false);
		}
		else if (direction.Y > 0.20f)
		{
			SetFacingTexture(FrontTexture, false);
		}
		else if (Mathf.Abs(direction.X) > 0.05f)
		{
			SetFacingTexture(SideTexture ?? FrontTexture, direction.X < 0);
		}

		float seconds = (float)delta;
		if ((GlobalPosition - beforeMove).LengthSquared() > 0.02f)
		{
			// Twelve discrete poses per loop: a little footstep weight and a
			// subtle body sway, with the character's original art intact.
			_walkTime += seconds * 14.5f;
			int frame = Mathf.PosMod((int)_walkTime, StepLift.Length);
			float lift = StepLift[frame];
			float lean = StepLean[frame];
			float sway = StepSway[frame];
			_sprite.Position = _restPosition + new Vector2(sway, -lift);
			_sprite.Rotation = lean;
			_sprite.Scale = new Vector2(
				_restScale.X * (1f + StepStretch[frame]),
				_restScale.Y * (1f - lift * 0.006f));
		}
		else
		{
			// Keep the last direction when stopping; only relax the walk pose.
			_idleTime += seconds * 2.2f;
			float easing = Mathf.Min(1.0f, seconds * 12.0f);
			_sprite.Position = _sprite.Position.Lerp(
				_restPosition + new Vector2(0, Mathf.Sin(_idleTime) * 0.4f), easing);
			_sprite.Rotation = Mathf.Lerp(_sprite.Rotation, 0.0f, easing);
			_sprite.Scale = _sprite.Scale.Lerp(
				_restScale * (1.0f + 0.008f * Mathf.Sin(_idleTime)), easing);
		}
	}

	private void SetFacingTexture(Texture2D texture, bool mirror)
	{
		if (texture != null && _sprite.Texture != texture)
			_sprite.Texture = texture;
		_sprite.FlipH = mirror;
	}

	private void OnButtonPressed()
	{
		GetNode<SceneManager>("/root/SceneManager").ShowPause();
	}
}
