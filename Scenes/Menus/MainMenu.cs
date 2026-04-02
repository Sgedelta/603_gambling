using Godot;
using System;

public partial class MainMenu : MarginContainer
{

	[Export] public PackedScene MainGameScene;

	public void PlayGame()
	{
		GetTree().ChangeSceneToPacked(MainGameScene);
	}
}
