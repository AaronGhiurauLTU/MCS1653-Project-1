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
	private Node2D slashAttack;
	private AnimationPlayer animationPlayer;
	private bool isAttacking = false;
	public override void _Ready()
	{
		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
		health.HealthChanged += OnHealthChanged;

		playerHud = GetNode<PlayerHud>("Player-HUD");

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		attackCooldownTimer = GetNode<Timer>("AttackCooldownTimer");
		slashAttack = GetNode<Node2D>("SlashAttack");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		slashAttack.Visible = false;
	}

	private void OnHealthChanged(int newHealth)
	{
		playerHud.UpdateHealthUI(newHealth);
	}

	private void OnHealthDepleted()
	{
		Velocity = Vector2.Zero;

		// play the character death animation
		animatedSprite.Play("death");

		// this is to reload the scene and perform any other effects after death
		animationPlayer.Play("death");
	}
	
	public void ReloadScene()
	{
		// reload the level, call deferred used to reload scene after physics frame to avoid errors
		GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
	}

	private void OnAttackCooldownTimeout()
	{
		canAttack = true;
	}

	private void OnAttackAnimationFinished(StringName animationName)
	{
		if (animationName == "player_attack")
		{
			isAttacking = false;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.TimeScale == 0 || health.CurrentHealth <= 0)
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

		if (Input.IsActionJustPressed("attack") && canAttack && !isAttacking)
		{
			isAttacking = true;
			canAttack = false;
			attackCooldownTimer.Start();
			animatedSprite.Play("attack");

			slashAttack.Visible = true;
			slashAttack.SetProcess(true);

			Vector2 attackDirection = direction;
			

			if (attackDirection.Length() == 0)
			{
				attackDirection = directionXFacing * Vector2.Right;
			}

			// angle that the attack is rotated by, so that the attack goes in the direction the player is facing
			float angleDeg = (float)Mathf.RadToDeg(attackDirection.Angle());

			slashAttack.RotationDegrees = angleDeg;

			slashAttack.Position = attackDirection * attackOffset;

			AnimationPlayer attackAnimationPlayer = slashAttack.GetNode<AnimationPlayer>("AnimationPlayer");
			attackAnimationPlayer.Play("player_attack");
		}

		Velocity = velocity;
		MoveAndSlide();

		// update animations
		if (!isAttacking)
		{
			if (IsOnFloor())
			{
				if (velocity.X == 0)
				{
					animatedSprite.Play("idle");
				}
				else
				{
					animatedSprite.Play("walk");
				}
			}
			else
			{
				animatedSprite.Play("jump");
			}
		}

	}
}
