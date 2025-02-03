using Godot;
using System;

public partial class PlayerHud : CanvasLayer
{
	private Label healthLabel;
	private Control pauseMenu;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		healthLabel = GetNode<Label>("HealthLabel");
		pauseMenu = GetNode<Control>("PauseMenu");
		pauseMenu.Visible = false;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("pause"))
		{
			pauseMenu.Visible = !pauseMenu.Visible;

			if (pauseMenu.Visible)
			{
				Engine.TimeScale = 0;		
			}
			else
			{
				Engine.TimeScale = 1;
			}
		}
	}

	public void UpdateHealthUI(int newHealth)
	{
		healthLabel.Text = newHealth + "";
	}
}
