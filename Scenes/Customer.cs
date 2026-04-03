using Godot;
using Godot.Collections;
using System;

public partial class Customer : CharacterBody2D
{
	//======BEHAVIOUR CONTROLS======
	public CustomerGoal CurrentGoal = CustomerGoal.WANDER;
	private float _wanderTime = 5f; //number of seconds to wander

	//How "Hopeful"/satisfied this character is - factors into decision making. ranges 0-1
	private float _hopeAmount = 1;
	public float HopeAmount { get { return _hopeAmount; } set { _hopeAmount = Mathf.Clamp(value, 0, 1); } }

	//how addicted to gambling this character is. The more addicted to gambling someone is, the less rationally they act!
	private float _addictionStrength = 0.05f;
	public float AddictionStrength { get { return _addictionStrength; } set { _addictionStrength += Mathf.Clamp(value, 0, 1); } }

    //Hope controls represent an offset/scalar (depending) applied to _hopeAmount when used in calculations
    [ExportGroup("Hope Controls")]
	[Export] public float HopeWinRateStrength = .1f; //how much hope shifts percieved win rate when deciding if we can make a profit
	[Export] public float HopeBorrowWillingness = .75f; //the maximum chance the customer is willing to put themselves in debt (when hope = 1)
	[Export] public float HopeLeaveRateStrength = .3f;

    [ExportGroup("")]
	


	//======GAMBLING CONTROLS======
	public float CurrentMoney = 100; //100 temp value
	public float StartingMoney = 100; //100 temp value
	//represents how much this character has "made" - 1 is no profit but no loss, 0-1 is some amount of loss but no debt, >1 is profiting, <0 is in debt 
	public float CurrentEarningPercent { get { return CurrentMoney / StartingMoney; } }
	private Dictionary<Machine, Array<bool>> _machineWinRates = new Dictionary<Machine, Array<bool>>();

	public Machine ActiveMachine;
	private const int MIN_PLAYS_TO_GUESS_RATE = 5;


    //======NAV CONTROLS======
    private NavigationAgent2D _navAgent;
    [Export] public float Speed = 300.0f;
	private float _movementDelta;

	private Vector2 _targetPos = Vector2.Zero;

	public Vector2 TargetPos
	{
		get { return _navAgent.TargetPosition; }
		set { _navAgent.TargetPosition = value;}
	}

	//======EXTRA VARS======
	private RandomNumberGenerator _rng;

    public override void _Ready()
    {
		base._Ready();

		_navAgent = GetNode<NavigationAgent2D>("NavAgent");

		//change based on speed & layout
		_navAgent.PathDesiredDistance = 5f;
		_navAgent.TargetDesiredDistance = 5f;

		_navAgent.VelocityComputed += OnVelocityComputed;

		_rng = new RandomNumberGenerator();


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

	//checks current states and sets CurrentGoal to the appropriate Goal
	public void ReevaluateGoal()
	{

		switch(CurrentGoal)
		{
			case CustomerGoal.GAMBLE:
                //if we are gambling and think we can stand to make a profit at this machine, KEEP GOING BABY MORE GAMBLING!! (maybe)
                if (GetPercievedMachineProfit(ActiveMachine) > 0)
                {
                    //check if using this machine even once will put us into debt
                    float debtChance = (1 - GetPercievedWinRate(ActiveMachine)) * (_hopeAmount * HopeBorrowWillingness);
                    if ((ActiveMachine.Cost >= CurrentMoney && _rng.Randf() >= Mathf.Min(debtChance, 1 - _addictionStrength)) || ActiveMachine.Cost < CurrentMoney)
                    {
                        //it either WON'T put us into debt OR we think that it's an acceptable risk (rolled lower than debtChance or we're addicted)
                        CurrentGoal = CustomerGoal.GAMBLE;
                        return;
                    }

					//decided it was too risky, fall through to wander
					break;

                }
				else
				{

                    //if we're gambling but we don't think we can make a profit, maybe we're wrong?
					if(_rng.Randf() < _addictionStrength)
					{
						CurrentGoal = CustomerGoal.GAMBLE;
						return;
					}
                }


                break;

			case CustomerGoal.WANDER:
				//if we're wandering, we either just entered or we left a machine for some reason. We need to analyze what we want to do.

				//NOTE: fleeing from "max debt" is NOT included in this logic, when we lose money so that CurrentEarningPercent is <= -1, the character should ALWAYS flee, otherwise addicts might never leave
				//case "0" -> gambling addicition. No matter what, if we're addicted to gambling, there's a chance we gamble again
				if (_rng.Randi() < _addictionStrength)
				{
					CurrentGoal = CustomerGoal.GAMBLE;
					return;
				}

				//case 1: we've made profit
					//consider leaving? if our hope is low, we should leave, otherwise we should stay

				//case 2: we're at our break even
					//again consider leaving, but if our hope is low we should REALLY consider leaving. With high hope we should REALLY consider staying

				//case 3: we're at a loss
					//again consider leaving, but if our hope is low we should ABSOLUTELY consider leaving. with high hope we should ABSOLUTELY stay!! we can make it back!!

				//case 4: we're in debt
					//oh shit. oh fuck. either we can make it back or we need to FLEE.

				break;
		}


		//safety check, we didn't reach any real conclusion, in which case we should just wander...
		CurrentGoal = CustomerGoal.WANDER;
		GetTree().CreateTimer(_wanderTime).Timeout += ReevaluateGoal;
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

	/// <summary>
	/// given a machine, returns how much this Customer thinks that machine will make them
	/// </summary>
	/// <param name="m"></param>
	/// <returns></returns>
	public float GetPercievedMachineProfit(Machine m)
	{
		float rate = GetPercievedWinRate(m);

		return (m.Profit * Mathf.Clamp(rate +  _hopeAmount * HopeWinRateStrength, 0, 1)) - (m.Cost * (1 - rate));

	}

}

public enum CustomerGoal
{
	GAMBLE, //find an active machine and use it
	WANDER, //walk around for a little
	LEAVE,  //Leave normally, without potentially dying (not in debt)
	FLEE    //get the FUCK out because you're in debt and don't think you can make it back
}