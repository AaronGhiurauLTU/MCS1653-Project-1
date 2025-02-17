using Godot;
using System;

// chat GPT was used for the CalculateInterceptionVelocity method
public partial class AdvancedFlyingEnemy : CharacterBody2D
{
	// the speed while dashing
	[Export] private float dashMoveSpeed = 200f,
		
		// the speed while wandering
		wanderMoveSpeed = 100f,

		// the speed while flying up
		flyingUpMoveSpeed = 50f;

	// the max distance this enemy can wander left or right
	[Export] private float maxWanderDistance = 200f;

	// reference to the player in the scene
	private Player player;

	// the original position to fly back to
	private Vector2 originalPosition;

	// the direction the dash should go in
	private Vector2 dashDirection;
	private Health health;

	// the various states the enemy can be in
	private bool isDashing = false,
		isWandering = true,
		isFlyingUp = false,
		playerDetected = false;
	
	// the direction the character is wandering in, either -1 or 1 for left or right respectively
	private int wanderDirection = 1;

	private AnimatedSprite2D animatedSprite;
	private AnimationPlayer animationPlayer;
	public override void _Ready()
	{
		player = GetNode<Player>("%Player");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite");

		// store the original position
		originalPosition = Position;

		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
		health.HealthChanged += OnHealthChanged;

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

	// this function was obtained from ChatGPT
 	public static Vector2 CalculateInterceptionVelocity(Vector2 playerPos, Vector2 playerVel, Vector2 enemyPos, float enemySpeed)
    {
		playerVel.Y *= 0.25f; // suppress vertical velocity component for jumping significantly
        Vector2 relativePos = playerPos - enemyPos;
        Vector2 relativeVel = playerVel;

        float a = relativeVel.Dot(relativeVel) - enemySpeed * enemySpeed;
        float b = 2 * relativePos.Dot(relativeVel);
        float c = relativePos.Dot(relativePos);

        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return Vector2.Zero; // No real solution, enemy is too slow
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDiscriminant) / (2 * a);
        float t2 = (-b - sqrtDiscriminant) / (2 * a);

        float t = Mathf.Min(t1, t2);
        if (t < 0) t = Mathf.Max(t1, t2);
        if (t < 0) return Vector2.Zero; // No valid interception time

        Vector2 interceptionPoint = playerPos + playerVel * t;
        Vector2 requiredDir = (interceptionPoint - enemyPos).Normalized();
        return requiredDir * enemySpeed;
    }

	// target the player and enter the dash state
	private void TargetPlayer()
	{
		Vector2 difference = player.Position - Position;
	

		// only target the player if it is below this enemy and the enemy was wandering
		if (difference.Y > 0 && isWandering)
		{
			dashDirection = CalculateInterceptionVelocity(player.Position, player.Velocity, Position, dashMoveSpeed);
			isDashing = true;
			isWandering = false;
		}
	}

	private void OnBodyEnteredDetectionArea(Node2D body)
	{
		playerDetected = true;
		TargetPlayer();
	}

	private void OnBodyExitedDetectionArea(Node2D body)
	{
		playerDetected = false;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.TimeScale == 0 || health.CurrentHealth <= 0)
			return;
			
		// if the player never exited the detection radius, attack them again after making it to the wander state
		if (isWandering && playerDetected)
		{
			TargetPlayer();
		}

		if (isDashing)
		{
			animatedSprite.Play("Dash");
			Velocity = dashDirection.Normalized() * dashMoveSpeed;
			
			// stop dashing once it collides or flies too far
			if (IsOnFloor() || IsOnWall() || (originalPosition - Position).Length() > 500)
			{
				isDashing = false;
				isFlyingUp = true;
			}
		}
		else if (isWandering)
		{
			animatedSprite.Play("Flying");

			// change the wandering direction once the max wander distance is exceeded
			if ((originalPosition - Position).Length() > maxWanderDistance)
			{
				wanderDirection *= -1;
			}
			
			Velocity = Vector2.Right * wanderDirection * wanderMoveSpeed;
		}
		else if (isFlyingUp)
		{
			animatedSprite.Play("Flying");
			Velocity = (originalPosition - Position).Normalized() * flyingUpMoveSpeed;

			// stop flying up once the enemy is close enough to the original position
			if ((originalPosition - Position).Length() <= 1)
			{
				isFlyingUp = false;
				isWandering = true;
			}
		}

		MoveAndSlide();

		// flip the sprite depending on its velocity so it faces the correct way
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
