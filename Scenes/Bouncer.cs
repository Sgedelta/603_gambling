using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Bouncer : Node2D
{
	private Queue<int> bouncerCost = new Queue<int>(new[] {500, 100, 200, 350, 500, 750});
	public Array<Texture2D> sprites = new Array<Texture2D>();
	int index = 1;
	public float StopTime = 1.0f;
	public bool IsHired = false;

	void Start()
	{
		sprites[0] = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer.png");
		sprites[1] = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer2.png");
        sprites[2] = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer3.png");
        sprites[3] = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer4.png");
        sprites[4] = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer5.png");
        sprites[5] = ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer6.png");
    }
	
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
		int cost = bouncerCost.Peek();
		if (cost <= mainGame.CasinoMoney)
		{
			cost = bouncerCost.Dequeue();
			mainGame.UpdateCasinoMoney(-cost);
			
			if(!IsHired)
			{
				IsHired = true;
				Show();
			}
			else
			{
				GetNode<Sprite2D>("Display/Sprite").Texture = sprites[index];
				index++;
				IncreaseStopTime();
			}
		}
		if (bouncerCost.Count <= 0) { return -1; }

		return bouncerCost.Peek();
	}
}
