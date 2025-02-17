using Godot;
using System;

public partial class Health : Node
{
	[Export] private int maxHealth = 5;
	private int currentHealth;
	private AudioStreamPlayer2D hitSound;
	// get the current health without allowing the setting of it publicly
	public int CurrentHealth { get { return currentHealth; } }

	// custom signal to fire when health reaches 0
	[Signal] public delegate void HealthDepletedEventHandler();

	[Signal] public delegate void HealthChangedEventHandler(int newHealth);
	public void TakeDamage(int damage)
	{
		currentHealth -= damage;

		// keep health from going below 0
		currentHealth = Math.Max(0, currentHealth);

		EmitSignal(SignalName.HealthChanged, currentHealth);
		hitSound.Play();
		// detect death
		if (currentHealth == 0)
		{
			EmitSignal(SignalName.HealthDepleted);
		}
	}
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		currentHealth = maxHealth;
		hitSound = GetNode<AudioStreamPlayer2D>("HitSound");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
