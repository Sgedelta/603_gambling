using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Bouncer : Node2D
{
	private Queue<int> bouncerCost = new Queue<int>(new[] {500, 100, 200, 350, 500, 750});
	public Array<Texture2D> sprites;
	int index = 1;
	public float StopTime = 0.6f;
	public bool IsHired = false;

	void Start()
	{
	}
	
	private void IncreaseStopTime()
	{
		StopTime += 0.3f;
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

	private void SetSprites()
	{
		sprites = new Array<Texture2D>();
		sprites.Add(ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer.png"));
		sprites.Add(ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer2.png"));
		sprites.Add(ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer3.png"));
		sprites.Add(ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer4.png"));
		sprites.Add(ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer5.png"));
		sprites.Add(ResourceLoader.Load<Texture2D>("res://Resources/Sprites/bouncer6.png"));
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
			SetSprites();
			IsHired = true;
			Show();
		}
		else
		{
			var sprite = GetNode<Sprite2D>("Display/Sprite");
			sprite.Texture = sprites[index];
			float factor = 1.5f * Mathf.Pow(1.05f, index);
			GD.Print("factor", factor);
			sprite.Scale = new Vector2(factor, factor);
			GD.Print(index);
			
			index++;
			IncreaseStopTime();
		}
		GD.Print(StopTime);
		return bouncerCost.Count == 0 ? -1 : bouncerCost.Peek();
	}
}
