using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Bouncer : Node2D
{
	private Queue<int> bouncerCost = new Queue<int>(new[] {500, 100, 200, 350, 500, 750});
	public float StopTime = 1.0f;
	public bool IsHired = false;
	
	private void IncreaseStopTime()
	{
		StopTime += 0.2f;
	}
	
	private async void OnBodyEntered(Node2D body)
	{
		if(!IsHired) return;
		
		if(body is Customer customer)
		{
			if(customer.CurrentGoal == CustomerGoal.FLEE)
			{
				float originalSpeed = customer.Speed;
				customer.Speed = 0;
				await ToSignal(GetTree().CreateTimer(StopTime), SceneTreeTimer.SignalName.Timeout);
				customer.Speed = originalSpeed;
			}
		}
	}

	public int Purchase()
	{
		var mainGame = GetNode<MainGame>("/root/MainGame");
		
		if (bouncerCost.Count == 0)
			return -1;
		
		int cost = bouncerCost.Peek();
		if (cost > mainGame.CasinoMoney)
		{
			return cost;
		}
		cost = bouncerCost.Dequeue();
		mainGame.UpdateCasinoMoney(-cost);
			
		if(!IsHired)
		{
			IsHired = true;
			Show();
		}
		else
		{
			IncreaseStopTime();
		}
				
		GD.Print(StopTime);

		return bouncerCost.Count == 0 ? -1 : bouncerCost.Peek();
	}
}
