using Godot;
using System;

public partial class PlayerHud : CanvasLayer
{
	private Label healthLabel;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// make HUD not move with player
	//	TopLevel = true;

		healthLabel = GetNode<Label>("HealthLabel");
	}

	public void UpdateHealthUI(int newHealth)
	{
		healthLabel.Text = newHealth + "";
	}
}
