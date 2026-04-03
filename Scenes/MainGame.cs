using Godot;
using Godot.Collections;
using System;

public partial class MainGame : Node2D
{
	private RandomNumberGenerator _rng;

	private Array<Customer> LivingCustomers = new Array<Customer>();
	private Array<Machine> ActiveMachines = new Array<Machine>();

	[Export] private PackedScene _customerPrefab;
	[Export] private PackedScene _machinePrefab;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();

		PopulateRandomCustomersAndMachines(3, 5);

		Tween MoveCustomers = CreateTween();
		MoveCustomers.SetLoops(); //loop forever
		MoveCustomers.TweenCallback(Callable.From(MoveAllCustomersToRandomOpenMachine)).SetDelay(10f);
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
		}

        for (int i = 0; i < customers; i++)
        {
            Customer newCustomer = _customerPrefab.Instantiate<Customer>();
            newCustomer.GlobalPosition = new Vector2(_rng.RandiRange(200, 3640), _rng.RandiRange(200, 1960)); //pick a random point within a smaller area of the screen
            AddChild(newCustomer);
			LivingCustomers.Add(newCustomer);
        }
    }

	//for testing - we'll break this up into 
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
