using Godot;
using Godot.Collections;
using System;

public partial class MainGame : Node2D
{
	private RandomNumberGenerator _rng;

	[Export] private Array<Customer> LivingCustomers = new Array<Customer>();
	[Export] private Array<Machine> ActiveMachines = new Array<Machine>();

	[Export] private PackedScene _customerPrefab;
	[Export] private PackedScene _machinePrefab;

	[Export] public Rect2 CasinoBounds;
	[Export] public Vector2 CasinoExit = Vector2.Zero;

	//Casino starting money, can adjust this if needed
	[Export] public float CasinoMoney = 100;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();

		//PopulateRandomCustomersAndMachines(10, 15);

		//Tween MoveCustomers = CreateTween();
		//MoveCustomers.SetLoops(); //loop forever
		//MoveCustomers.TweenCallback(Callable.From(MoveAllCustomersToRandomOpenMachine)).SetDelay(10f);

		GameManager.instance.ActiveMainGame = this;


		//DEBUG TESTING PERCIEVED WINRATE
		//LivingCustomers[0].ActiveMachine = ActiveMachines[0];
		//for(int i = 0; i < 500; i++)
		//{
		//	bool win = _rng.Randf() > .5f;
		//	GD.Print(win);
        //    LivingCustomers[0].RegisterGame(win);
		//	LivingCustomers[0].GetPercievedWinRate(ActiveMachines[0]);
		//}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//for testing, creates and places randomly a number of customers and machines
	public void PopulateRandomCustomersAndMachines(int customers, int machines)
	{

		for(int i = 0; i < machines; i ++)
		{
			Machine newMachine = _machinePrefab.Instantiate<Machine>();
			newMachine.GlobalPosition = new Vector2(_rng.RandiRange(200, 3640), _rng.RandiRange(200, 1960)); //pick a random point within a smaller area of the screen
			AddChild(newMachine);
			ActiveMachines.Add(newMachine);

			//Subsrible to money change signal
			//This wasn't working so it's assinged in editor now, ideally should fix later
			//newMachine.OnCasinoMoneyChange += UpdateCasinoMoney;
		}

        for (int i = 0; i < customers; i++)
        {
            Customer newCustomer = _customerPrefab.Instantiate<Customer>();
            newCustomer.GlobalPosition = new Vector2(_rng.RandiRange(200, 3640), _rng.RandiRange(200, 1960)); //pick a random point within a smaller area of the screen
            AddChild(newCustomer);
			LivingCustomers.Add(newCustomer);
        }
    }

	//Updates total money casino has
	public void UpdateCasinoMoney(float amount)
	{
		//Adds amount (can be negative)
		CasinoMoney += amount;

		//If it's under 0, even out to 0 (unless we're allowed to go into debt...that's a design question)
		if (CasinoMoney < 0)
		{
			CasinoMoney = 0;
		}

		GD.Print("Casino gets: " + amount);
	}

	//Returns the "best" machine for a given customer. Decided by the game because the customer doesn't know about all the machines
	//Note: is this particularly effecient? No! but it doesn't run that often, so it's alright...
	//to make this better, we could cache this data in the machine or customer and then we can do this faster. I think?
	public Machine GetBestMachineForCustomer(Customer customer)
	{
		//the best rating found by the customer, based on if the consider win rate or profit
		float bestRating = 0;

		//loop through all machines, log the best score
		foreach(Machine m in ActiveMachines)
		{
			//do not consider busy machines
			if(!m.IsAvailable)
			{
				continue;
			}

			//using bitflag in customer to see what we consider here
			float rating = 0;
			if((customer.MachineConsiderations & MachinePickConsiderations.WIN_RATE) != 0)
			{
				rating += customer.GetPercievedWinRate(m);
			}
			if((customer.MachineConsiderations & MachinePickConsiderations.PROFIT) != 0)
			{
				rating += customer.GetPercievedMachineProfit(m);
			}

			//track if actually better
			if(rating > bestRating)
			{
				bestRating = rating;
			}

		}

		//loop through all machines and gather all with a score equal to the best score
		Array<Machine> bestMachines = new Array<Machine> ();
        foreach (var m in ActiveMachines)
        {
            //using bitflag in customer to see what we consider here
            float rating = 0;
            if ((customer.MachineConsiderations & MachinePickConsiderations.WIN_RATE) != 0)
            {
                rating += customer.GetPercievedWinRate(m);
            }
            if ((customer.MachineConsiderations & MachinePickConsiderations.PROFIT) != 0)
            {
                rating += customer.GetPercievedMachineProfit(m);
            }

			if(rating >= bestRating)
			{
				bestMachines.Add(m);
			}
        }

        //pick one of those randomly and return it
        return bestMachines[_rng.RandiRange(0, bestMachines.Count-1)];
	}

	//for testing - we'll break this up into customer logic
	public void MoveAllCustomersToRandomOpenMachine()
	{
		//clear all machines
		foreach(var machine in ActiveMachines)
		{
			machine.IsAvailable = true;
		}

		//send customers to machines
		for(int i = 0; i < LivingCustomers.Count; i++)
		{
			Customer c = LivingCustomers[i];
			Machine m = ActiveMachines[_rng.RandiRange(0, ActiveMachines.Count-1)];
			int weHateInfiniteLoops = 500;
			while (!m.IsAvailable && weHateInfiniteLoops > 0)
			{
                m = ActiveMachines[_rng.RandiRange(0, ActiveMachines.Count - 1)];
				weHateInfiniteLoops--;
            }
			if(weHateInfiniteLoops <= 0)
			{
				GD.PrintErr("[MG] We don't have enough machines to send ALL of the customers!!!");
				return;
			}
			c.TargetPos = m.GlobalPosition;
			m.IsAvailable = false;
		}

	}


}
