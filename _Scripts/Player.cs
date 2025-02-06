using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -400.0f;
	[Export] public float attackOffset = 1f;
	private bool canAttack = true;
	private int directionXFacing = 1;
	private Health health;
	private PlayerHud playerHud;
	private AnimatedSprite2D animatedSprite;
	private Timer attackCooldownTimer;
	public override void _Ready()
	{
		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
		health.HealthChanged += OnHealthChanged;

		playerHud = GetNode<PlayerHud>("Player-HUD");

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		attackCooldownTimer = GetNode<Timer>("AttackCooldownTimer");
	}

	private void OnHealthChanged(int newHealth)
	{
		playerHud.UpdateHealthUI(newHealth);
	}

	private void OnHealthDepleted()
	{
		// reload the level, call deferred used to reload scene after physics frame to avoid errors
		GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
	}

	private void OnAttackCooldownTimeout()
	{
		canAttack = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.TimeScale == 0)
			return;
			
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
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
			if (velocity.X > 0)
			{
				directionXFacing = 1;
				animatedSprite.FlipH = false;
			}
			else if (velocity.X < 0)
			{
				directionXFacing = -1;
				animatedSprite.FlipH = true;
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		if (Input.IsActionJustPressed("attack") && canAttack)
		{
			canAttack = false;
			attackCooldownTimer.Start();
			animatedSprite.Play("attack");
			var scene = GD.Load<PackedScene>("res://Scenes/slash_attack.tscn");
			Vector2 attackDirection = direction;
			
			if (attackDirection.Length() == 0)
			{
				attackDirection = directionXFacing * Vector2.Right;
			}

			// angle that the attack is rotated by, so that the attack goes in the direction the player is facing
			float angleDeg = (float)Mathf.RadToDeg(attackDirection.Angle());

			var newAttack = scene.Instantiate() as Node2D;
			AddChild(newAttack);
			newAttack.RotationDegrees = angleDeg;

			newAttack.Position += attackDirection * attackOffset;

			AnimationPlayer attackAnimationPlayer = newAttack.GetNode<AnimationPlayer>("AnimationPlayer");
			attackAnimationPlayer.Play("attack");
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}
