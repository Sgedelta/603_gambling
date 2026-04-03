using Godot;
using System;

public partial class Machine : Node2D
{
	//hii juliaaaa I just made this so "Machines" can exist and be "open" that's literally all I need it for kthxby -Sam
	public bool IsAvailable = true;
	public float Cost = 1;
	public float Profit = 10;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
