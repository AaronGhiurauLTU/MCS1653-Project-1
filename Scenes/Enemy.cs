using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	private Health health;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		health = GetNode<Health>("Health");

		// connect health's custom HealthDepleted signal to OnHealthDepleted
		health.HealthDepleted += OnHealthDepleted;
	}

	private void OnHealthDepleted()
	{
		// destroy the enemy node
		QueueFree();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
