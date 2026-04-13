using Godot;
using Godot.Collections;
using System;
using System.Reflection.PortableExecutable;

public partial class Customer : CharacterBody2D
{
	//======BEHAVIOR CONTROLS======
	private CustomerGoal _currentGoal = CustomerGoal.WANDER;
	public CustomerGoal CurrentGoal { 
		get { return _currentGoal;  } 
		set { 
			if(DEBUG)
			{
				GD.Print($"[C] {Name}: old goal was {_currentGoal}, new goal is {value}");
			}
			_currentGoal = value;
		} }
	private float _wanderTime = 5f; //number of seconds to wander
	private float _wanderRecalcTime = 3f;

	//How "Hopeful"/satisfied this character is - factors into decision making. ranges 0-1
	private float _hopeAmount = 1;
	public float HopeAmount { get { return _hopeAmount; } set { _hopeAmount = Mathf.Clamp(value, 0, 1); } }

	//how addicted to gambling this character is. The more addicted to gambling someone is, the less rationally they act!
	private float _addictionStrength = 0.05f;
	public float AddictionStrength { get { return _addictionStrength; } set { _addictionStrength += Mathf.Clamp(value, 0, 1); } }
	[ExportGroup("Behavior Controls")]

	//Hope controls represent an offset/scalar (depending) applied to _hopeAmount when used in calculations
	[ExportSubgroup("Hope Modifier Controls")]
	[Export] public float HopeWinRateStrength = .1f; //how much hope shifts percieved win rate when deciding if we can make a profit
	[Export] public float HopeBorrowWillingness = .75f; //the maximum chance the customer is willing to put themselves in debt / more in debt (when hope = 1)
	[Export] public float HopeLeaveRateStrength = .25f; //The predisposition to leave. What the Hope function is "centered" around - when hope is at 50%, this will be the chance to leave

	[ExportSubgroup("Hope Adjusting Controls")]
	[Export] private float _baseWinHopeGain = .05f;
	[Export] private float _baseLossHopeLoss = .01f;
	[Export] private float _machineSucksHopeLoss = .05f;

    private int _winLossStreak = 0;

	[ExportSubgroup("Machine Pick Controls")]
	[Export] private float _rateStr = 1;
	[Export] private float _profitStr = 1;
	[Export] private float _distUnit = 200;
	[Export] private float _distStr = 1;


	[ExportSubgroup("")]

	[Export] public float Indecisiveness = .2f; //chance of a gambler to wander again when leaving wander state


	//======GAMBLING CONTROLS======
	[ExportGroup("Gambling Controls")]
	[Export] private float _currentMoney = 100; //100 temp value
	public float CurrentMoney { get { return _currentMoney; } set { _currentMoney = value; _moneyDisplay.Display(value); } }
	public float StartingMoney;


	//represents how much this character has "made" - 1 is no profit but no loss, 0-1 is some amount of loss but no debt, >1 is profiting, <0 is in debt 
	public float CurrentEarningPercent { get { return _currentMoney / StartingMoney; } }
	private Dictionary<Machine, Array<bool>> _machineWinRates = new Dictionary<Machine, Array<bool>>();

	public Machine ActiveMachine;
	private const int MIN_PLAYS_TO_GUESS_RATE = 5;
	private bool _isWaitingForGame = false;

	private int _playCount = 0; //how many times we've gambled...

	private bool _customerLocked = false;
	private Vector2 _lockPosition;

	[Export] public MachinePickConsiderations MachineConsiderations = 0;

	[ExportGroup("")]

	//======NAV CONTROLS======
	private NavigationAgent2D _navAgent;
	[Export] public float Speed = 300.0f;
	[Export] public float FleeSpeed = 1500f;
	private float _movementDelta;
	private Sprite2D _sprite;

	private Vector2 _targetPos = Vector2.Zero;

	public Vector2 TargetPos
	{
		get { return _navAgent.TargetPosition; }
		set { _navAgent.TargetPosition = value;}
	}

	//======EXTRA VARS======
	[Export] private GradientTexture1D _hopeGradient;
	[Export] private bool DEBUG = false;
	private RandomNumberGenerator _rng;
	private Tween _rewanderTween;
	private MoneyDisplay _moneyDisplay;
    [Export] private Texture2D killCursor;
	[Export] private int soulValue = 1; //do we want some customers to have varying soul values? idk

    [Signal] public delegate void OnCustomerKillEventHandler(int value);
	private Sprite2D alertSprite;

    public override void _Ready()
	{
		base._Ready();

		_navAgent = GetNode<NavigationAgent2D>("NavAgent");

		//change based on speed & layout
		_navAgent.PathDesiredDistance = 5f;
		_navAgent.TargetDesiredDistance = 5f;

		_navAgent.VelocityComputed += OnVelocityComputed;
		_navAgent.NavigationFinished += CheckNavLock;

		_rng = new RandomNumberGenerator();
		_sprite = GetNode<Sprite2D>("Display");
		_moneyDisplay = GetNode<MoneyDisplay>("MoneyDisplay");
		_moneyDisplay.Display(_currentMoney);

		this.InputEvent += OnClick;
		this.MouseEntered += OnMouseEntered;
		this.MouseExited += OnMouseExit;
		alertSprite = GetNode<Sprite2D>("FleeAlert");

		Callable.From(DelayedSetup).CallDeferred();

    }

	public void DelayedSetup()
	{

		_rewanderTween = CreateTween().SetLoops();
		_rewanderTween.TweenCallback(Callable.From(() => {

			if (CurrentGoal == CustomerGoal.WANDER)
			{
				TargetPos = GetNewWanderLoc();
				if (DEBUG)
				{
					GD.Print($"[C] {Name}: Picked new wander location -> {TargetPos}");
				}
			}
			else if(DEBUG)
			{
				GD.Print($"[C] {Name}: did NOT pick a new loc, because my current goal is {CurrentGoal}");

			}
		
		})).SetDelay(_wanderRecalcTime);
		TargetPos = GetNewWanderLoc();

		//BEGINS CONTROL LOGIC
		ReevaluateGoal();

	}

	//Runs when the character is clicked
	private void OnClick(Node viewport, InputEvent clickEvent, long shapeIdx)
	{
		if(Input.IsMouseButtonPressed(MouseButton.Left))
		{
			CheckKill();
        }
	}

	//Checks customer state and if they should uh. explode when clicked
	private void CheckKill()
	{
		//If the current state isn't flee, end 
		if (CurrentGoal != CustomerGoal.FLEE)
		{
			return;
		}

		//Change cursor back to normal
		//Not entirely sure this'd count as mouse exit since thing is being destroyed
		Input.SetCustomMouseCursor(null);

		//Send signal to maingame for soul change
		EmitSignal(SignalName.OnCustomerKill, soulValue);

		//Destroy object
		QueueFree();
	}

	private void OnMouseEntered()
	{
		//Can we kill this guy
		if (CurrentGoal == CustomerGoal.FLEE)
		{
            //Change and center crosshair
            Input.SetCustomMouseCursor(killCursor, Input.CursorShape.Arrow, new Vector2(25, 25));
        }
    }

	private void OnMouseExit()
	{
		//Resets crosshair if needed
		Input.SetCustomMouseCursor(null);
	}

	//set values, from CasinoEntrance. INDEX MATTERS, IF YOU CHANGE INDEX, CHANGE GetCustVals()
	public void SetupCustomerValues(Array<float> vals)
	{
		_currentMoney = vals[0];
		StartingMoney = vals[0];
		_hopeAmount = vals[1];
		_addictionStrength = vals[2];
		HopeWinRateStrength = vals[3];
		HopeBorrowWillingness = vals[4];
		HopeLeaveRateStrength = vals[5];
		_baseWinHopeGain = vals[6];
		_baseLossHopeLoss = vals[7];
		_machineSucksHopeLoss = vals[8];
    }

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		//Manually changing color in fleecasino, don't want it to be overwritten here
		if (CurrentGoal != CustomerGoal.FLEE)
		{
            _sprite.Modulate = _hopeGradient.Gradient.Sample(_hopeAmount);
        }

        //handle our gambling!
        if (CurrentGoal == CustomerGoal.GAMBLE)
		{
			//see if we have an active machine
			//if we don't have an active machine, try to find the "best" open machine, based on our own considerations
			if(ActiveMachine == null)
			{
				ActiveMachine = GameManager.instance.ActiveMainGame.GetBestMachineForCustomer(this);

				if (ActiveMachine != null) //sometimes, no machines are open.
				{
                    float noDistanceGoodness = GetMachinePercievedGoodness(ActiveMachine, true);

					//double check we actually make profit...
					if (noDistanceGoodness > 0 || (noDistanceGoodness < 0 && RandomAddictionCheck())) 
					{
                        ActiveMachine.IsAvailable = false; //mark this machine as taken
                        TargetPos = ActiveMachine.PlayPosition;
                        if (DEBUG)
                        {
                            GD.Print($"[C] {Name}: Going to Gamble at my new machine, {ActiveMachine.Name}");
                        }
                    }
					else
					{
                        if (DEBUG)
                        {
                            GD.Print($"[C] {Name}: Going to Wander because my new machine, {ActiveMachine.Name}, sucks ({noDistanceGoodness})");
                        }
						_hopeAmount -= _machineSucksHopeLoss;
                        BeginWander();
						return;
					}

				}
				else
				{
					BeginWander(); //welp. just wander if nothing's op
					return;
				}

			}

			//safety check because I think it's breaking some?
			if(ActiveMachine != null)
			{
				if(TargetPos != ActiveMachine.PlayPosition)
				{
					TargetPos = ActiveMachine.PlayPosition;
				}
			}

			//see if we are AT our active machine
			//GlobalPosition.DistanceSquaredTo(ActiveMachine.GlobalPosition) <= Mathf.Pow(ActiveMachine.PlayDistance, 2)
			if (ActiveMachine.IsCustomerInPlayArea(this))
			{
				//if we are, we can play if we aren't already
				if(!_isWaitingForGame)
				{
					Gamble();
				}
			}

		}


		//move

		if(_customerLocked)
		{
			GlobalPosition = _lockPosition;
			return;
		}

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
					if(DEBUG)
					{
						GD.Print($"[C] {Name}: Fleeing because CurrentEarningPercent ({CurrentEarningPercent} is less than -1");
					}
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
					if (CurrentMoney < 0 && !RandomAddictionCheck(debtAcceptanceAdjusted))
					{
						if (DEBUG)
						{
							GD.Print($"[C] {Name}: Fleeing because CurrentEarningPercent ({CurrentEarningPercent} is too risky!");
						}
						FleeCasino();
						return;
					}

					//if we accept the risk that this puts us into debt, or it won't put us into debt, HIT IT AGAIN BABEYYY
					if ((ActiveMachine.Cost >= _currentMoney && RandomAddictionCheck(debtAcceptance)) || ActiveMachine.Cost < _currentMoney)
					{
						if (DEBUG)
						{
							GD.Print($"[C] {Name}: Gambling because we decided the risk was okay or there was none!");
						}
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
					if(RandomAddictionCheck())
					{
						if (DEBUG)
						{
							GD.Print($"[C] {Name}: Gambling because we're ADDICTED to it! Wahoo!!");
						}
						CurrentGoal = CustomerGoal.GAMBLE;
						return;
					}

					//we're not wrong
					//if we're in debt we need to consider fleeing RIGHT NOW
					if (_currentMoney < 0 && !RandomAddictionCheck(debtAcceptanceAdjusted))
					{
						if (DEBUG)
						{
							GD.Print($"[C] {Name}: Fleeing because we are in debt and this machine won't make us money.");
						}
						
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
					if (DEBUG)
					{
						GD.Print($"[C] {Name}: Can't Decide, Wandering Again.");
					}
					//wander around again lol
					break;
				}

				//NOTE: fleeing from "max debt" is NOT included in this logic, when we lose money so that CurrentEarningPercent is <= -1, the character should ALWAYS flee, otherwise addicts might never leave
				//case "0" -> gambling addicition. No matter what, if we're addicted to gambling, there's a chance we gamble again
				if (RandomAddictionCheck())
				{
					if (DEBUG)
					{
						GD.Print($"[C] {Name}: Gambling again because we're addicted to it anyway");
					}
					CurrentGoal = CustomerGoal.GAMBLE;
					return;
				}

				//If we haven't played enough games, go gamble..
				if(_playCount < GameManager.instance.ActiveMainGame.NumMinGames) 
				{
					if (DEBUG)
					{
						GD.Print($"[C] {Name}: Gambling because we just got here! we've only played {_playCount}");
					}
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
				float EarnAbs = Mathf.Abs(CurrentEarningPercent);
				float decisionStrength = Mathf.Pow(Mathf.Abs(Mathf.Log(EarnAbs + 1)), EarnAbs + 1); //don't ask, it just works...
				//the chance of leaving
				float leaveChance = Mathf.Clamp((-1.0f * (Mathf.Log(_hopeAmount / (1 - _hopeAmount)) * decisionStrength)) + HopeLeaveRateStrength, 0, 1);

				//if we're going to leave, do it
				if(_rng.Randf() <= leaveChance)
				{
					if (DEBUG)
					{
						GD.Print($"[C] {Name}: Leaving because we rolled below our leave chance of {leaveChance}");
					}
					
					LeaveCasino();
					return;
				}
				else
				{
					if (DEBUG)
					{
						GD.Print($"[C] {Name}: Gambling because there's nothing else to do! leave chance was {leaveChance} | {decisionStrength}");
					}
					CurrentGoal = CustomerGoal.GAMBLE;
					return;
				}

				break; //yes, i know, unreachable. just preventing in case there's ever further states.
		}


		//we didn't reach any real conclusion, in which case we should just wander...
		if (DEBUG)
		{
			GD.Print($"[C] {Name}: Wandering because I don't want to do what I was doing before, which was {CurrentGoal}");
		}
		BeginWander();
	}

	public Vector2 GetNewWanderLoc()
	{
		Vector2 size = GameManager.instance.ActiveMainGame.CasinoBounds.Size;
		return GameManager.instance.ActiveMainGame.CasinoBounds.GetCenter() + new Vector2(_rng.RandfRange(-size.X/2, size.X/2), _rng.RandfRange(-size.Y/2, size.Y/2));	
	}

	public void LeaveCasino()
	{
		//there is no leaving when in debt...
		if(CurrentEarningPercent < 0)
		{
			FleeCasino();
			return;
		}
		CurrentGoal = CustomerGoal.LEAVE;
		LeaveMachine();
		TargetPos = GameManager.instance.ActiveMainGame.CasinoExit;
	}

	public async void FleeCasino()
	{
		CurrentGoal = CustomerGoal.FLEE;
		LeaveMachine();
		TargetPos = GameManager.instance.ActiveMainGame.CasinoExit;

		//Color switches to purple
		_sprite.SelfModulate = new Color(1, 0, 1);

		//Enable flee alert sprite
		alertSprite.Visible = true;

		//Wait for a sec then increase speed
		await ToSignal(GetTree().CreateTimer(1f), SceneTreeTimer.SignalName.Timeout);

		Speed = FleeSpeed;
		_navAgent.PathDesiredDistance *= 10;
		_navAgent.TargetDesiredDistance *= 10;
	}

	public void BeginWander()
	{
		LeaveMachine();
		CurrentGoal = CustomerGoal.WANDER;
		GetTree().CreateTimer(_wanderTime).Timeout += ReevaluateGoal;
	}

	private bool RandomAddictionCheck(float otherConsideration = 0)
	{
		return _rng.Randf() <= Mathf.Max(_addictionStrength, otherConsideration);
	}

	private void LeaveMachine()
	{
		if(ActiveMachine != null)
		{
			if (DEBUG)
			{
				GD.Print($"[C] {Name}: Leaving machine {ActiveMachine.Name}");
			}
			ActiveMachine.IsAvailable = true;
			ActiveMachine = null;
			UnlockCustomer();

		}  
	}

	public async void Gamble()
	{
		//SAFETY, just in case, don't gamble while we're already gambling. this shouldn't be called ANYWAY but still.
		if(_isWaitingForGame)
		{
			return;
		}
		_isWaitingForGame = true;

		if (DEBUG)
		{
			GD.Print($"[C] {Name}: LETS GO GAMBLING!!");
		}

		//subscribe our listener(s)
		ActiveMachine.OnGamePlayed += RegisterGame;

		//play the game, then wait for it to finish
		ActiveMachine.Play(this);
		await ToSignal(ActiveMachine, Machine.SignalName.OnGamePlayed);
		//count that we played!
		_playCount += 1; 


		//unsubscribe our listener(s)
		//we could do this when we pick and leave a machine... but this is a bit cleaner, imo. we might leave after ANY game and we only care about it WHEN we play. so. safer! one place!
		ActiveMachine.OnGamePlayed -= RegisterGame;
        

        //rethink life choices
        ReevaluateGoal();

		_isWaitingForGame = false;
	}

	private void CheckNavLock()
	{
		if(CurrentGoal == CustomerGoal.GAMBLE)
		{
			LockCustomer(GlobalPosition);
		}
	}

	private void LockCustomer(Vector2 pos)
	{
		_lockPosition = pos;
		_customerLocked = true;
	}

	private void UnlockCustomer(bool typeFix = false)
	{
		_customerLocked = false;
	}

	public void OnVelocityComputed(Vector2 safeVelocity)
	{
		GlobalPosition = GlobalPosition.MoveToward(GlobalPosition + safeVelocity, _movementDelta);
	}

	public void RegisterGame(bool win)
	{
		if (DEBUG)
		{
			GD.Print($"[C] {Name}: Registering game at {ActiveMachine.Name}, win state was {win}");
		}

		//register in win/loss dictionary
		if (!_machineWinRates.ContainsKey(ActiveMachine))
		{
			_machineWinRates.Add(ActiveMachine, new Array<bool>());
		}

		//adjust hope and do other win/loss things
		if (win)
		{
			_winLossStreak = Mathf.Max(1, _winLossStreak + 1);
			_hopeAmount += _baseWinHopeGain * _winLossStreak;
		}
		else
		{
			_winLossStreak = Mathf.Min(-1, _winLossStreak - 1);
			_hopeAmount += _baseLossHopeLoss * _winLossStreak;
		}
		//hope must be from 0-1
		_hopeAmount = Mathf.Clamp(_hopeAmount, 0, 1);

		if(DEBUG)
		{
			GD.Print($"[C] {Name}: Hope is now {_hopeAmount}");
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

		if(DEBUG)
		{
			GD.Print($"[C] {Name}: Percieved Wins and Rate from machine {m.Name}: {percievedWins} / {potentialMaxWins} = {percievedWins / potentialMaxWins}");
		}

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

		float profit = (m.Profit * Mathf.Clamp(rate + _hopeAmount * HopeWinRateStrength, 0, 1)) - m.Cost;


        if (DEBUG) 
		{
			GD.Print($"[C] {Name}: Percieved Profit from {m.Name} is {profit}: win {m.Profit} x {Mathf.Clamp(rate + _hopeAmount * HopeWinRateStrength, 0, 1)}, lose {m.Cost}"); 
		}


        return profit;

	}

	public float GetMachinePercievedGoodness(Machine m, bool ignoreDistance = false)
	{
		//do not consider busy machines
		if (!m.IsAvailable)
		{
			return float.MinValue;
		}

		//using bitflag in customer to see what we consider here
		float rating = 0;
		if ((MachineConsiderations & MachinePickConsiderations.WIN_RATE) != 0)
		{
			rating += GetPercievedWinRate(m) * _rateStr;
		}
		if ((MachineConsiderations & MachinePickConsiderations.PROFIT) != 0)
		{
			rating += GetPercievedMachineProfit(m) * _profitStr;
		}
		if(!ignoreDistance && (MachineConsiderations & MachinePickConsiderations.DISTANCE) != 0)
		{
			rating -= (GlobalPosition.DistanceTo(m.GlobalPosition) / _distUnit) * _distStr;
		}

		return rating;
	}

}

public enum CustomerGoal
{
	GAMBLE, //find an active machine and use it
	WANDER, //walk around for a little
	LEAVE,  //Leave normally, without potentially dying (not in debt)
	FLEE    //get the FUCK out because you're in debt and don't think you can make it back
}

[Flags]
public enum MachinePickConsiderations
{
	WIN_RATE = 1 << 1,
	PROFIT = 1 << 2,
	DISTANCE = 1 << 3,


	RATE_AND_PROFIT = WIN_RATE | PROFIT,
	RATE_AND_DISTANCE = WIN_RATE | DISTANCE,
	PROFIT_AND_DISTANCE = PROFIT | DISTANCE,

	ALL = WIN_RATE | PROFIT | DISTANCE,

}
