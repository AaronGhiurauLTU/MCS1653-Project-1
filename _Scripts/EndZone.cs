using Godot;
using System;

public partial class EndZone : Area2D
{
	private void OnBodyEntered(Node2D body)
	{
		GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, "res://Scenes/EndScreen.tscn");
	}
}
