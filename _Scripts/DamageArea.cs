using Godot;
using System;

public partial class DamageArea : Area2D
{
	[Export] int damage = 1;
	private void OnBodyEntered(Node2D body)
	{
		DamageTarget(body);
	}

	private void OnAreaEntered(Area2D area)
	{
		GD.Print(area.GetParent().Name);
		DamageTarget(area);
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
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
