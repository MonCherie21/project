
using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 150.0f;
	[Export] public Texture2D FrontTexture;
	[Export] public Texture2D BackTexture;

	private Sprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNode<Sprite2D>("Sprite2D");

		if (FrontTexture != null)
			_sprite.Texture = FrontTexture;
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

		if (direction.Y < 0 && BackTexture != null)
		{
			_sprite.Texture = BackTexture;
		}
		else if (direction.Y > 0 && FrontTexture != null)
		{
			_sprite.Texture = FrontTexture;
		}
	}
}
