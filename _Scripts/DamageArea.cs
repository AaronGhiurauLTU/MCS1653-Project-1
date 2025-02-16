using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public partial class DamageArea : Area2D
{
	// the damage this area does
	[Export] public int damage = 1;

	// reference to the timer that counts down the time left for invincibility
	private Timer invincibilityTimer;

	// reference to the last touched body that is still entered in the area
	private Node2D enteredBody = null;

	// the invincibility wait time set in the inspector of the timer
	private float invincibilityTime;
	public override void _Ready()
	{
		invincibilityTimer = GetNode<Timer>("InvincibilityTimer");
		invincibilityTime = (float)invincibilityTimer.WaitTime;
	}

	private void OnBodyEntered(Node2D body)
	{
		DamageTarget(body);
		enteredBody = body;

		// start the timer
		invincibilityTimer.Start(invincibilityTime);
	}

	private void OnBodyExited(Node2D body)
	{
		// set reference to null since the object left
		enteredBody = null;

		// stop the timer for checking for invincibility
		invincibilityTimer.Stop();
	}

	private void OnInvincibilityTimedOut()
	{
		// if the object is still in the area, damage it once more and reset the timer
		if (enteredBody != null)
		{
			DamageTarget(enteredBody);
			invincibilityTimer.Start(invincibilityTime);
		}
	}

	private void DamageTarget(Node2D target)
	{
		// make sure the body is not the same as the parent of the parent of this damage area and there is a health component
		// this basically means that a character cannot attack itself
		if (target != GetParent() && target != GetParent().GetParent() && target.FindChild("Health") is Health)
		{
			Health bodyHealth = target.FindChild("Health") as Health;
			bodyHealth.TakeDamage(damage);
		}
	}
}
