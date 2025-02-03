using Godot;
using System;

public partial class MainMenu : Control
{
	private void OnPlayPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/level.tscn");
	}

	private void OnQuitPressed()
	{
		GetTree().Quit();
	}
}
