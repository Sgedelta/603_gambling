using Godot;
using Godot.Collections;
using System;

public partial class MainGame : Node2D
{
	private RandomNumberGenerator _rng;

	[Signal] public delegate void GameStartedEventHandler();

	[Export] private Array<Customer> LivingCustomers = new Array<Customer>();
	[Export] private Array<Machine> ActiveMachines = new Array<Machine>();

	[Export] private PackedScene _customerPrefab;
	[Export] private PackedScene _machinePrefab;

	[Export] public Rect2 CasinoBounds;
	[Export] public Vector2 CasinoExit = Vector2.Zero;

	//Casino starting money, can adjust this if needed
	[Export] public float CasinoMoney = 100;

	[Export] private MoneyDisplay _mDisplay;


	//EVERY customer will play this amount of games before they consider leaving (NOT Fleeing)
	[Export] public int NumMinGames = 0;


	[Export] private bool DEBUG = false;
	
	[Export] private PackedScene _adScene;
	[Export] private float _adMinWait = 30f;
	[Export] private float _adMaxWait = 60f;

	[Export] private bool _allowAds = true;
	private Timer _adTimer;
	private bool _adPlaying = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();
		_mDisplay.Display(CasinoMoney);
		
		//hook up our inserted machines
		foreach(Machine m in ActiveMachines)
		{
			m.OnCasinoMoneyChange += UpdateCasinoMoney;
		}

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
		
		_adTimer = new Timer();
		_adTimer.OneShot = true;
		AddChild(_adTimer);
		_adTimer.Timeout += ShowAd;
		ScheduleNextAd();

		EmitSignal(SignalName.GameStarted);
	}
	
	private void ScheduleNextAd()
	{
		float waitTime = _rng.RandfRange(_adMinWait, _adMaxWait);
		_adTimer.WaitTime = waitTime;
		_adTimer.Start();
	}
	
	private void ShowAd()
	{
		if (!_allowAds) return;
		if (_adPlaying) return;
		_adPlaying = true;
		
		GetTree().Paused = true;
		
		var ad = _adScene.Instantiate<CanvasLayer>();
		ad.ProcessMode = ProcessModeEnum.Always;
		AddChild(ad);		
		ad.Connect("ad_closed", Callable.From(() =>
		{
			GetTree().Paused = false;
			_adPlaying = false;
			ad.QueueFree();
			ScheduleNextAd();
		}));
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		//some debug shit
		if(DEBUG)
		{
			int openMachines = 0;
			foreach (Machine m in ActiveMachines)
			{
				if (m.IsAvailable) { openMachines++; }
			}
			GD.Print($"[MG] Open Machines: {openMachines}");
		}
		
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

		_mDisplay.Display(CasinoMoney);
	}

	//Returns the "best" machine for a given customer. Decided by the game because the customer doesn't know about all the machines
	//Note: is this particularly effecient? No! but it doesn't run that often, so it's alright...
	//to make this better, we could cache this data in the machine or customer and then we can do this faster. I think?
	public Machine GetBestMachineForCustomer(Customer customer)
	{
		//the best rating found by the customer, based on if the consider win rate or profit
		float bestRating = float.MinValue;

		//loop through all machines, log the best score
		foreach(Machine m in ActiveMachines)
		{

			float rating = customer.GetMachinePercievedGoodness(m);

			//track if actually better
			if(rating > bestRating)
			{
				bestRating = rating;
			}

		}

		//will only happen if NO machines are free
		if(bestRating == float.MinValue)
		{
			return null;
		}

		//loop through all machines and gather all with a score equal to the best score
		Array<Machine> bestMachines = new Array<Machine> ();
		foreach (var m in ActiveMachines)
		{
			float rating = customer.GetMachinePercievedGoodness(m);

			if(rating >= bestRating)
			{
				bestMachines.Add(m);
			}
		}

		//pick one of those randomly and return it
		return bestMachines[_rng.RandiRange(0, bestMachines.Count-1)];
	}

	private void RegisterNewCustomer(Customer customer)
	{
		LivingCustomers.Add(customer);
	}

	private void UnregisterCustomer(Customer customer)
	{
		if(LivingCustomers.Contains(customer))
		{
            LivingCustomers.Remove(customer);
        }
	}

	private void RegisterNewMachine(Machine machine)
	{
		ActiveMachines.Add(machine);
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
