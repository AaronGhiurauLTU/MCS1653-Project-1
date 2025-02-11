using Godot;
using System;

public partial class FlyingEnemy : CharacterBody2D
{
	[Export] private float dashMoveSpeed = 200f,
		wanderMoveSpeed = 100f,
		flyingUpMoveSpeed = 50f;
	[Export] private float maxWanderDistance = 200f;
	private Node2D player;
	private Vector2 originalPosition;
	private Vector2 dashVector;
	private Health health;
	private bool isDashing = false,
		isWandering = true,
		isFlyingUp = false,
		playerDetected = false;
	
	private int wanderDirection = 1;

	private AnimatedSprite2D animatedSprite;
	public override void _Ready()
	{
		player = GetNode<Node2D>("%Player");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite");
		originalPosition = Position;

		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
	}

	private void OnHealthDepleted()
	{
		// destroy the enemy node, uses call deferred to delete after the physics frame to avoid issues
		CallDeferred(Node.MethodName.QueueFree);
	}

	private void TargetPlayer()
	{
		Vector2 difference = player.Position - Position;

		if (difference.Y > 0 && !isFlyingUp)
		{
			dashVector = difference;
			isDashing = true;
			isWandering = false;
			playerDetected = true;
		}
	}

	private void OnBodyEnteredDetectionArea(Node2D body)
	{
		TargetPlayer();
	}

	private void OnBodyExitedDetectionArea(Node2D body)
	{
		playerDetected = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		// if the player never exited the detection radius, attack them again after making it to the wander state
		if (isWandering && playerDetected)
		{
			TargetPlayer();
		}

		if (isDashing)
		{
			Velocity = dashVector.Normalized() * dashMoveSpeed;

			if (IsOnFloor())
			{
				isDashing = false;
				isFlyingUp = true;
			}
		}
		else if (isWandering)
		{
			if ((originalPosition - Position).Length() > maxWanderDistance)
			{
				wanderDirection *= -1;
			}
			
			Velocity = Vector2.Right * wanderDirection * wanderMoveSpeed;
		}
		else if (isFlyingUp)
		{
			Velocity = (originalPosition - Position).Normalized() * flyingUpMoveSpeed;

			if ((originalPosition - Position).Length() <= 1)
			{
				isFlyingUp = false;
				isWandering = true;
			}
		}

		MoveAndSlide();

		if (Velocity.X > 0)
		{
			animatedSprite.FlipH = false;
		}
		else if (Velocity.X < 0)
		{
			animatedSprite.FlipH = true;
		}
	}
}
