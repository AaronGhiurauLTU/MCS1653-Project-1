using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	[Export] private float speed = 5;
	private Health health;
	private float directionXFacing = 1;

	private RayCast2D rayCastRight, rayCastLeft;
	private AnimatedSprite2D animatedSprite;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;

		rayCastRight = GetNode<RayCast2D>("RayCastRight");
		rayCastLeft = GetNode<RayCast2D>("RayCastLeft");

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	}

	private void OnHealthDepleted()
	{
		// destroy the enemy node, uses call deferred to delete after the physics frame to avoid issues
		CallDeferred(Node.MethodName.QueueFree);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Engine.TimeScale == 0)
			return;
			
		if (rayCastRight.IsColliding())
		{
			directionXFacing = -1;
			animatedSprite.FlipH = true;
		}
		else if (rayCastLeft.IsColliding())
		{
			directionXFacing = 1;
			animatedSprite.FlipH = false;
		}

		Velocity = Vector2.Right * directionXFacing * speed;
		MoveAndSlide();
	}
}
