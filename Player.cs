using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 150.0f;
	[Export] public Texture2D FrontTexture;
	[Export] public Texture2D BackTexture;
	[Export] public Texture2D SideTexture;

	// Eight stepped visual poses per walk cycle. These adjust only the art;
	// physics, collision, camera, and saved world coordinates stay unchanged.
	private static readonly float[] StepLift = { 0f, 0.8f, 2.1f, 0.9f, 0f, 0.8f, 2.1f, 0.9f };
	private static readonly float[] StepLean = { 0f, 0.012f, 0.020f, 0.010f,
		0f, -0.012f, -0.020f, -0.010f };
	private static readonly float[] StepSway = { 0f, 0.45f, 0.65f, 0.25f,
		0f, -0.45f, -0.65f, -0.25f };

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
		if (direction.LengthSquared() > 0.01f)
		{
			// Eight distinct movement poses per loop, at retro RPG cadence.
			// No synthetic image frames or changes to collision geometry.
			_walkTime += seconds * 13.0f;
			int frame = Mathf.PosMod((int)_walkTime, StepLift.Length);
			float lift = StepLift[frame];
			float lean = StepLean[frame];
			float sway = StepSway[frame];
			_sprite.Position = _restPosition + new Vector2(sway, -lift);
			_sprite.Rotation = lean;
			_sprite.Scale = new Vector2(
				_restScale.X * (1.0f + Mathf.Abs(lean) * 0.4f),
				_restScale.Y * (1.0f - lift * 0.008f));
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
