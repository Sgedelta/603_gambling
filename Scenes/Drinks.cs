using Godot;
using System;

public partial class Drinks : StaticBody2D
{
	[Export] public float DrinkCost = 5;
	[Export] public float DrinkHopeStr = .1f;
	[Export] public float DrinkAddictionStr = .025f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	public void BuyDrink(Customer c)
	{

	}

}
