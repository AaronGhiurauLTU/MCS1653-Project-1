using Godot;
using System;

public partial class PlayerHud : CanvasLayer
{
	// reference to the sprite of the hearts
	private Sprite2D heartsSprite;

	// store the original rect of the sprite
	private Rect2 heartsSpriteRect;
	private Control pauseMenu;

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
		// simply scale the heart sprite to represent the new health since the texture was marked as repeatable
		heartsSprite.RegionRect = new Rect2(0, 0, heartsSpriteRect.Size.Y * newHealth, heartsSpriteRect.Size.Y);
	}
}
