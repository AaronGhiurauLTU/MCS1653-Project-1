using Godot;
using System;

public partial class PlayerHud : CanvasLayer
{
	private Sprite2D heartsSprite;
	private Control pauseMenu;

	private Rect2 heartsSpriteRect;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		heartsSprite = GetNode<Sprite2D>("Hearts");
		heartsSpriteRect = heartsSprite.GetRect();
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
		heartsSprite.RegionRect = new Rect2(0, 0, heartsSpriteRect.Size.Y * newHealth, heartsSpriteRect.Size.Y);
	}
}
