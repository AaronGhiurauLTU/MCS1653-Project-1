using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] private float speed = 5;
	private Health health;
	private float directionXFacing = 1;

	private RayCast2D rayCastRight, rayCastLeft;
	private AnimatedSprite2D animatedSprite;
	private AnimationPlayer animationPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
		health.HealthChanged += OnHealthChanged;

		rayCastRight = GetNode<RayCast2D>("RayCastRight");
		rayCastLeft = GetNode<RayCast2D>("RayCastLeft");

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}

	private void OnHealthChanged(int newHealth)
	{
		if (newHealth > 0)
		{
			animationPlayer.Play("damaged");
		}
	}

	private void OnHealthDepleted()
	{
		animationPlayer.Play("death");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.TimeScale == 0 || health.CurrentHealth <= 0)
			return;
		
		// flip is inverted to account for the sprite being faced in the opposite direction
		if (rayCastRight.IsColliding())
		{
			directionXFacing = -1;
			animatedSprite.FlipH = false;
		}
		else if (rayCastLeft.IsColliding())
		{
			directionXFacing = 1;
			animatedSprite.FlipH = true;
		}

		Velocity = Vector2.Right * directionXFacing * speed;
		MoveAndSlide();
	}
}
