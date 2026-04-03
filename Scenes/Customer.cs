using Godot;
using Godot.Collections;
using System;

public partial class Customer : CharacterBody2D
{
	//===GAMBLING CONTROLS===
	public float CurrentMoney = 100; //100 temp value
	public float StartingMoney = 100; //100 temp value
	private Dictionary<Machine, Array<bool>> _machineWinRates = new Dictionary<Machine, Array<bool>>();

	public Machine ActiveMachine;
	private const int MIN_PLAYS_TO_GUESS_RATE = 5;


    //===NAV CONTROLS===
    private NavigationAgent2D _navAgent;
    [Export] public float Speed = 300.0f;
	private float _movementDelta;

	private Vector2 _targetPos = Vector2.Zero;

	public Vector2 TargetPos
	{
		get { return _navAgent.TargetPosition; }
		set { _navAgent.TargetPosition = value;}
	}

    public override void _Ready()
    {
		base._Ready();

		_navAgent = GetNode<NavigationAgent2D>("NavAgent");

		//change based on speed & layout
		_navAgent.PathDesiredDistance = 5f;
		_navAgent.TargetDesiredDistance = 5f;

		_navAgent.VelocityComputed += OnVelocityComputed;




    }

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);


		if(_navAgent.IsNavigationFinished())
		{
			return;
		}

		Vector2 currentPos = GlobalTransform.Origin;
		Vector2 nextPathPos = _navAgent.GetNextPathPosition();
		_movementDelta = Speed * (float)delta;

		//set Vel, an internal Character2D thing for MoveAndSlide
		Velocity = currentPos.DirectionTo(nextPathPos) * Speed;
		if(_navAgent.AvoidanceEnabled)
		{
			_navAgent.Velocity = Velocity;
		}
		else
		{
			//OnVelocityComputed(Velocity);
			MoveAndSlide();
		}

	}

	public void OnVelocityComputed(Vector2 safeVelocity)
	{
		GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + safeVelocity, _movementDelta);
	}

	public void RegisterGame(bool win)
	{
		//register in win/loss dictionary

		if(!_machineWinRates.ContainsKey(ActiveMachine))
		{
			_machineWinRates.Add(ActiveMachine, new Array<bool>());
		}

		_machineWinRates[ActiveMachine].Add(win);
	}

	/// <summary>
	/// Gets the Percieved Win Rate, from 0 to 1, of the provided machine, based on play history. 
	/// If a machine has not been played, or has not been played enough, it is assumed the machine always pays out (1)
	/// </summary>
	public float GetPercievedWinRate(Machine m)
	{
		if (!_machineWinRates.ContainsKey(m) || _machineWinRates[m].Count < MIN_PLAYS_TO_GUESS_RATE)
		{
			return 1; //assume the machine is good
		}

        float percievedWins = 0;
        float potentialMaxWins = 0;

        //check entire history - scale recent history (at end) to be stronger than old history, because recency bias!
        for (int i = 0; i < _machineWinRates[m].Count; i++)
		{
			float winVal = _machineWinRates[m][_machineWinRates[m].Count - 1 - i] ? 1 : 0;
			//if its within the last MIN_PLAYS, treat it at "full value", otherwise, scale it by some value
			if(i >= MIN_PLAYS_TO_GUESS_RATE)
			{
				winVal /= i - MIN_PLAYS_TO_GUESS_RATE + 1;
				potentialMaxWins += 1.0f / (i - MIN_PLAYS_TO_GUESS_RATE + 1);
			}
			else
			{
				potentialMaxWins += 1;
			}
            percievedWins += winVal;

        }

		GD.Print($"Percieved Wins and Rate: {percievedWins} / {potentialMaxWins} = {percievedWins / potentialMaxWins}");

		//return the percieved rate
		return percievedWins / potentialMaxWins;
	}

}
