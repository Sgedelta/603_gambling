using Godot;
using System;

public partial class Drinks : StaticBody2D
{
	[Export] public bool IsOpen = false;
	
	[Export] public float DrinkCost = 5;
	[Export] public float DrinkHopeStr = .1f;
	[Export] public float DrinkAddictionStr = .025f;
	[Export] public float DrinkTime = 1; //how long it takes to drink a drink

	[Export] private Area2D DrinkArea;
	[Export] private Shape2D DrinkShape;
	private RandomNumberGenerator _rng;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	public void BuyDrink(Customer c)
	{
		//take casino money
		GameManager.instance.ActiveMainGame.UpdateCasinoMoney(-DrinkCost);

		GetTree().CreateTimer(DrinkTime).Timeout += () => { GiveCustomerDrinkBenefits(c); };
	}

	public void GiveCustomerDrinkBenefits(Customer c)
	{
        //give customer drink
        c.HopeAmount += DrinkHopeStr;
        c.AddictionStrength += DrinkAddictionStr;
		c.ReevaluateGoal();
    }

	public Vector2 GetRandomLocForCust()
	{
		Vector2 shapeSize = DrinkShape.GetRect().Size;

		return DrinkArea.GlobalPosition + new Vector2(_rng.RandfRange(-shapeSize.X, shapeSize.X), _rng.RandfRange(-shapeSize.Y, shapeSize.Y));

    }

}
