using Godot;
using System;

public partial class ExitZone : Area2D
{
	[Signal] public delegate void CustomerMarkedKillEventHandler(Customer c);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void CheckForKill(Node2D body)
	{
		//we only care about customers
		if(body is not Customer)
		{
			return;
		}

		Customer c = (Customer)body;

		//safety check, only kill a Leaving or Fleeing Customer
		if(c.CurrentGoal != CustomerGoal.LEAVE || c.CurrentGoal != CustomerGoal.FLEE)
		{
			return;
		}

		c.QueueFree();

		if(c.CurrentGoal == CustomerGoal.FLEE)
		{
			EmitSignal(SignalName.CustomerMarkedKill, c);
		}

	}
	
}
