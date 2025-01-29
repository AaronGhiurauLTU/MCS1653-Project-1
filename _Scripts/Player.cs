using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -400.0f;
	private Health health;
	private PlayerHud playerHud;
	public override void _Ready()
	{
		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
		health.HealthChanged += OnHealthChanged;

		playerHud = GetNode<PlayerHud>("Player-HUD");
	}

	private void OnHealthChanged(int newHealth)
	{
		playerHud.UpdateHealthUI(newHealth);
	}

	private void OnHealthDepleted()
	{
		// destroy the player node
		QueueFree();
	}
	public override void _PhysicsProcess(double delta)
	{
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("left", "right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (Input.IsActionJustPressed("attack"))
		{
			var scene = GD.Load<PackedScene>("res://Scenes/slash_attack.tscn");

			var newAttack = scene.Instantiate() as Node2D;
			AddChild(newAttack);
			//newAttack.Position = Position;
			GD.Print("attack!");
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
