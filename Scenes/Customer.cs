using Godot;
using Godot.Collections;
using System;

public partial class Customer : CharacterBody2D
{
	//======BEHAVIOUR CONTROLS======
	public CustomerGoal CurrentGoal = CustomerGoal.WANDER;
	private float _wanderTime = 5f; //number of seconds to wander
	private float _wanderRecalcTime = 3f;

	//How "Hopeful"/satisfied this character is - factors into decision making. ranges 0-1
	private float _hopeAmount = 1;
	public float HopeAmount { get { return _hopeAmount; } set { _hopeAmount = Mathf.Clamp(value, 0, 1); } }

	//how addicted to gambling this character is. The more addicted to gambling someone is, the less rationally they act!
	private float _addictionStrength = 0.05f;
	public float AddictionStrength { get { return _addictionStrength; } set { _addictionStrength += Mathf.Clamp(value, 0, 1); } }

    //Hope controls represent an offset/scalar (depending) applied to _hopeAmount when used in calculations
    [ExportGroup("Hope Controls")]
	[Export] public float HopeWinRateStrength = .1f; //how much hope shifts percieved win rate when deciding if we can make a profit
	[Export] public float HopeBorrowWillingness = .75f; //the maximum chance the customer is willing to put themselves in debt / more in debt (when hope = 1)
	[Export] public float HopeLeaveRateStrength = .25f; //The predisposition to leave. What the Hope function is "centered" around - when hope is at 50%, this will be the chance to leave

	[ExportGroup("")]

	[Export] public float Indecisiveness = .2f; //chance of a gambler to wander again when leaving wander state
	


	//======GAMBLING CONTROLS======
	public float CurrentMoney = 100; //100 temp value
	public float StartingMoney = 100; //100 temp value
	//represents how much this character has "made" - 1 is no profit but no loss, 0-1 is some amount of loss but no debt, >1 is profiting, <0 is in debt 
	public float CurrentEarningPercent { get { return CurrentMoney / StartingMoney; } }
	private Dictionary<Machine, Array<bool>> _machineWinRates = new Dictionary<Machine, Array<bool>>();

	public Machine ActiveMachine;
	private const int MIN_PLAYS_TO_GUESS_RATE = 5;

	private int _playCount = 0; //how many times we've gambled...


    //======NAV CONTROLS======
    private NavigationAgent2D _navAgent;
    [Export] public float Speed = 300.0f;
	[Export] public float FleeSpeed = 1500f;
	private float _movementDelta;

	private Vector2 _targetPos = Vector2.Zero;

	public Vector2 TargetPos
	{
		get { return _navAgent.TargetPosition; }
		set { _navAgent.TargetPosition = value;}
	}

	//======EXTRA VARS======
	private RandomNumberGenerator _rng;
	private Tween ReWanderTween;

    public override void _Ready()
    {
		base._Ready();

		_navAgent = GetNode<NavigationAgent2D>("NavAgent");

		//change based on speed & layout
		_navAgent.PathDesiredDistance = 5f;
		_navAgent.TargetDesiredDistance = 5f;

		_navAgent.VelocityComputed += OnVelocityComputed;

		_rng = new RandomNumberGenerator();

		Callable.From(DelayedSetup).CallDeferred();

    }

	public void DelayedSetup()
	{

        ReWanderTween = CreateTween().SetLoops();
        ReWanderTween.TweenCallback(Callable.From(() => { if (CurrentGoal == CustomerGoal.WANDER) TargetPos = GetNewWanderLoc(); })).SetDelay(_wanderRecalcTime);
        TargetPos = GetNewWanderLoc();

    }

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if(CurrentGoal == CustomerGoal.GAMBLE)
		{
			//see if we have an active machine

			//if we don't have an active machine, try to find an open machine

			//if we do have an active machine, wait a time, gamble, then reconsider our life choices. 
		}


		//move

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

				//first priority, have we gambled *too* much? (this is no matter what, no one can gamble this much!)
				if(CurrentEarningPercent <= -1)
				{
					CurrentGoal = CustomerGoal.FLEE;
					FleeCasino();
					return;
				}

                //debtAcceptance represents how willing I am to risk debt. It is the WIN rate, multiplied by how willing I am to lose money
                float debtAcceptance = GetPercievedWinRate(ActiveMachine) * (_hopeAmount * HopeBorrowWillingness);

                //a level of debt Acceptance, adjusted by how in debt I already am, compared to my "max limit" of -1
                float debtAcceptanceAdjusted = Mathf.Lerp(debtAcceptance, 0, -CurrentEarningPercent);

                //if we are gambling and think we can stand to make a profit at this machine, KEEP GOING BABY MORE GAMBLING!! (maybe)
                if (GetPercievedMachineProfit(ActiveMachine) > 0)
                {

                    //check if we're already in debt - if we are, we might decide to flee instead of just wandering to another machine
                    if (CurrentMoney < 0 && _rng.Randf() > Mathf.Max(debtAcceptanceAdjusted, _addictionStrength))
					{
						CurrentGoal = CustomerGoal.FLEE;
						FleeCasino();
						return;
					}

                    //if we accept the risk that this puts us into debt, or it won't put us into debt, HIT IT AGAIN BABEYYY
                    if ((ActiveMachine.Cost >= CurrentMoney && _rng.Randf() <= Mathf.Max(debtAcceptance, _addictionStrength)) || ActiveMachine.Cost < CurrentMoney)
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

                    //we're not wrong
					//if we're in debt we need to consider fleeing RIGHT NOW
                    if (CurrentMoney < 0 && _rng.Randf() > Mathf.Max(debtAcceptanceAdjusted, _addictionStrength))
                    {
                        CurrentGoal = CustomerGoal.FLEE;
						FleeCasino();
                        return;
                    }

					//if we're not in debt OR we're confident enough that we don't need to flee, go to another machine / reconsider life (fall through)
                }


                break;

			case CustomerGoal.WANDER:
				//if we're wandering, we either just entered or we left a machine for some reason. We need to analyze what we want to do.

				//maybe we're indecisive...
				if(_rng.Randf() < Indecisiveness)
				{
					//wander around again lol
					break;
				}

				//NOTE: fleeing from "max debt" is NOT included in this logic, when we lose money so that CurrentEarningPercent is <= -1, the character should ALWAYS flee, otherwise addicts might never leave
				//case "0" -> gambling addicition. No matter what, if we're addicted to gambling, there's a chance we gamble again
				if (_rng.Randf() < _addictionStrength)
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
				//THIS CASE IS HANDLED IN GAMBLE. We should ALWAYS decide if we flee from where we make/lose money.

				//cases 1-3 are similar enough to have one calculation, which is then scaled based on how we're doing.
				//people who make profit are at their break even are the most "indecisve" - hope does not affect them greatly either way
				//people in deep loss or great profit are more "impulsive" - with low hope, they are more likely to leave. with high hope, they are more likely to stay

				//this falls on a sigmoid curve, which is then clamped
				//how strong the decision is. A value closer to 0 is "flatter", higher values result in a "steeper" version
				float decisionStrength = Mathf.Pow(Mathf.Abs(Mathf.Log(CurrentEarningPercent)), CurrentEarningPercent); //don't ask, it just works...
				//the chance of leaving
				float leaveChance = Mathf.Clamp((-1.0f * (Mathf.Log(_hopeAmount / (1 - _hopeAmount)) * decisionStrength)) + HopeLeaveRateStrength, 0, 1);

				//if we're going to leave, do it
				if(_rng.Randf() <= leaveChance)
				{
					CurrentGoal = CustomerGoal.LEAVE;
					LeaveCasino();
					return;
				}
				else
				{
					CurrentGoal = CustomerGoal.GAMBLE;
					return;
				}

				break; //yes, i know, unreachable. just preventing in case there's ever further states.
		}


		//we didn't reach any real conclusion, in which case we should just wander...
		ActiveMachine = null;
		CurrentGoal = CustomerGoal.WANDER;
		GetTree().CreateTimer(_wanderTime).Timeout += ReevaluateGoal;
	}

	public Vector2 GetNewWanderLoc()
	{
		Vector2 size = GameManager.instance.ActiveMainGame.CasinoBounds.Size;
		return GameManager.instance.ActiveMainGame.CasinoBounds.GetCenter() + new Vector2(_rng.RandfRange(-size.X/2, size.X/2), _rng.RandfRange(-size.Y/2, size.Y/2));	
	}

	public void LeaveCasino()
	{
		ActiveMachine = null;
		TargetPos = GameManager.instance.ActiveMainGame.CasinoExit;
	}

	public void FleeCasino()
	{
		ActiveMachine = null;
		TargetPos = GameManager.instance.ActiveMainGame.CasinoExit;
		Speed = FleeSpeed;
		_navAgent.PathDesiredDistance *= 10;
		_navAgent.TargetDesiredDistance *= 10;
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