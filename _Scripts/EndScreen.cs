using Godot;
using System;

public partial class EndScreen : Control
{
	private void OnMainMenuPressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}
}
