using Godot;
using System;

public partial class Player : CharacterBody2D
{
	[Export] public float Speed = 300.0f;
	[Export] public float JumpVelocity = -400.0f;
	[Export] public float attackOffset = 1f;
	[Export] public float maxSlideDistance = 100f,
		normalAttackScale = 1.25f,
		slideAttackScale = 1.5f;
	private bool canAttack = true;
	private int directionXFacing = 1;
	private Health health;
	private PlayerHud playerHud;
	private AnimatedSprite2D animatedSprite;
	private Timer attackCooldownTimer;
	private Node2D slashAttack;
	private AnimationPlayer animationPlayer;
	private bool isAttacking = false,
		isSliding = false;
	
	private Vector2 originalSlidePosition;

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

		if (newHealth > 0)
		{
			animationPlayer.Play("damaged");
		}
	}

	private void OnHealthDepleted()
	{
		Velocity = Vector2.Zero;

		// fix rotation if player died while sliding
		StopSlide();

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

	private void StopSlide()
	{
		if (isSliding)
		{
			isSliding = false;
			RotationDegrees = 0;
			Position = new Vector2(Position.X, Position.Y - 6);
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
		
		float horizontalInputs = Input.GetAxis("left", "right");

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		} 
		else if (Input.IsActionJustPressed("slide") && !isAttacking && IsOnFloor() && !isSliding && horizontalInputs != 0)
		{
			isSliding = true;
			RotationDegrees = -90 * directionXFacing;
			Position = new Vector2(Position.X, Position.Y + 10);
			originalSlidePosition = Position;
		}

		// Get the input direction and handle the movement/deceleration.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		
		if (horizontalInputs != 0 && !isSliding)
		{
			velocity.X = horizontalInputs * Speed;

			if (horizontalInputs > 0)
			{
				directionXFacing = 1;
				animatedSprite.FlipH = false;
			}
			else if (horizontalInputs < 0)
			{
				directionXFacing = -1;
				animatedSprite.FlipH = true;
			}
		}
		else if (!isSliding)
		{
			// decelerate speed to 0
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}


		if (isSliding)
		{
			if (!IsOnFloor() || (Position - originalSlidePosition).Length() > maxSlideDistance || IsOnWall())
			{
				StopSlide();

				// decelerate speed to 0
				velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			}
			else
			{
				//animatedSprite.FlipH = true;
				velocity.X = directionXFacing * Speed * 2;
			}
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



			AnimationPlayer attackAnimationPlayer = slashAttack.GetNode<AnimationPlayer>("AnimationPlayer");
			AnimatedSprite2D attackAnimationSprite = slashAttack.GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			DamageArea attackDamageArea = slashAttack.GetNode<DamageArea>("DamageArea");
			AudioStreamPlayer2D attackSound = slashAttack.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

			attackAnimationPlayer.Play("player_attack");

			if (!isSliding)
			{
				slashAttack.Scale = new(normalAttackScale, normalAttackScale);
				attackAnimationSprite.Play("default");
				attackDamageArea.damage = 1;
				attackAnimationPlayer.SpeedScale = 0.5f;
				attackSound.PitchScale = 1;
				attackSound.VolumeDb = 0;
			}
			else // do a stronger attack while sliding
			{
				slashAttack.Scale = new(slideAttackScale, slideAttackScale);
				attackAnimationSprite.Play("inverted");
				attackDamageArea.damage = 2;
				attackAnimationPlayer.SpeedScale = 1f;
				attackSound.PitchScale = 2;
				attackSound.VolumeDb = 1;
			}
			slashAttack.RotationDegrees = angleDeg;

			slashAttack.Position = attackDirection * attackOffset;
		}

		Velocity = velocity;
		MoveAndSlide();

		// update animations
		if (!isAttacking)
		{
			if (IsOnFloor())
			{
				if (isSliding)
				{
					animatedSprite.Play("slide");
				}
				else if (velocity.X == 0)
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
