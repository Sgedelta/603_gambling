using Godot;
using System;

public partial class Onboarding : Node2D
{

	[Export] public PackedScene MainGameScene;

    public void PlayGame()
	{
		GetTree().ChangeSceneToPacked(MainGameScene);
	}
}