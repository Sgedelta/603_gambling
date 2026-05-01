using Godot;
using System;

public partial class DeathScreen : Node2D
{
	[Export] Sprite2D Greyscale;
	[Export] Sprite2D YouDied;
	[Export] Sprite2D HealthBar;
	[Export] Sprite2D Thumbnail;
	[Export] Button ReturnToMainMenu;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
