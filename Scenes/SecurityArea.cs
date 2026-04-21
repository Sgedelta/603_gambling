using Godot;
using System;

public partial class SecurityArea : Area2D
{	
	public float StopTime = 1.0f;
	
	private void IncreaseStopTime()
	{
		StopTime += 0.2f;
	}
	
	private async void OnBodyEntered(Node2D body)
	{
		GD.Print("i workin");
		if(body is Customer customer)
		{
			if(customer.CurrentGoal == CustomerGoal.FLEE)
			{
				float originalSpeed = customer.Speed;
				customer.Speed = 0;
				await ToSignal(GetTree().CreateTimer(StopTime), SceneTreeTimer.SignalName.Timeout);
				customer.Speed = originalSpeed;
			}
		GD.Print(body);
		}
	}
}
