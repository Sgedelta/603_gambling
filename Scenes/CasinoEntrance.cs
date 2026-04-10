using Godot;
using Godot.Collections;

public partial class CasinoEntrance : Node2D
{

	[Signal] public delegate void CustomerCreatedEventHandler(Customer customer);

	[Export] private PackedScene _customerScene;


	[Export] private float _spawnTickSpeed = 1f;
	public float SpawnTickSpeed { get { return _spawnTickSpeed; } set { _spawnTickSpeed = value; }}
	[Export] private float _spawnChancePerTick = .05f;
	public float SpawnChancePerTick { get { return _spawnChancePerTick; } set { _spawnChancePerTick = value; }}

	[Export] private float _spawnIncreasePerFail = .025f;
	public float SpawnIncreasePerFail { get { return _spawnIncreasePerFail; } set { _spawnIncreasePerFail = value; }}
	private int _failedSpawnAttempts = 0;

	[Export] private int _gameStartedCustomerCount = 3;
	[Export] private float _gameStartedCustomerInRate = 1;


	[ExportGroup("Debug New Customer Controls")] //only exported for debug purposes - generally, keep these values as they are "starting" values
	[Export] public Vector2 StartingMoney = new Vector2(50, 300);
	[Export] public Vector2 StartingHope = new Vector2(.7f, 1); //represents what customers have "heard" of the casino
	[Export] public Vector2 StartingAddiction = new Vector2(.01f, .075f);
	[Export] public Vector2 WinRateStr = new Vector2(.05f, .15f); //How much hope effects the percieved win rate 
	[Export] public Vector2 BorrowWillingness = new Vector2(.66f, .9f); //the MAX chance a customer willingly goes into debt (or risks it)
	[Export] public Vector2 LeaveRateCenter = new Vector2(.25f, .5f); //the chance to leave when in wander when hope is 50%
	[Export] public Vector2 BaseHopeGain = new Vector2(.025f, .075f);
	[Export] public Vector2 BaseHopeLoss = new Vector2(.01f, .025f);
	[Export] public Vector2 NoMachinesHopeLoss = new Vector2(.01f, .05f); //how much having no available / all poor quality machines upsets this customer 



	private Timer _spawnTimer;
	private RandomNumberGenerator _rng;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_spawnTimer = GetNode<Timer>("Timer");
		_spawnTimer.Timeout += TrySpawnCustomer;
		_spawnTimer.Start(_spawnTickSpeed);

		_rng = new RandomNumberGenerator();
	}

	public void SpawnStartingCustomers()
	{
		Tween startingCustTween = CreateTween();

		startingCustTween.SetLoops(_gameStartedCustomerCount);
		startingCustTween.TweenCallback(Callable.From(SpawnCustomer)).SetDelay(_gameStartedCustomerInRate);
	}

	public void TrySpawnCustomer()
	{
		//don't spawn customers over the max
		if(GameManager.instance.ActiveMainGame.CustomerCount < GameManager.instance.ActiveMainGame.MaxCustomerCount)
		{
            //maybe we spawn?
            if (_rng.Randf() <= _spawnChancePerTick + (_failedSpawnAttempts * _spawnIncreasePerFail))
            {
                SpawnCustomer();
                _failedSpawnAttempts = 0;
            }
            else
            {
                _failedSpawnAttempts++;
            }
        }

		_spawnTimer.Start(_spawnTickSpeed);

	}

	public void SpawnCustomer()
	{
        Customer newCust = (Customer)_customerScene.Instantiate();

        newCust.GlobalPosition = GlobalPosition;

        newCust.SetupCustomerValues(GetCustVals());

        GameManager.instance.ActiveMainGame.AddChild(newCust);

        EmitSignal(SignalName.CustomerCreated, newCust);
    }

	//builds an array and sends it to customer. ORDER IS IMPORTANT.
	//IF YOU CHANGE THE ORDER, CHANGE SETUPCUSTOMERVALUES
	//This could be an enum or string dict but. no <3
	//do that if this is ever touched again
	private Array<float> GetCustVals()
	{
		Array<float> vals = new Array<float>();

        vals.Add(GetRandom(StartingMoney));
		vals.Add(GetRandom(StartingHope));
		vals.Add(GetRandom(StartingAddiction));
		vals.Add(GetRandom(WinRateStr ));
		vals.Add(GetRandom(BorrowWillingness));
		vals.Add(GetRandom(LeaveRateCenter ));
		vals.Add(GetRandom(BaseHopeGain ));
		vals.Add(GetRandom(BaseHopeLoss ));
		vals.Add(GetRandom(NoMachinesHopeLoss));

		return vals;
	}

	//Note: made this a method incase we change this in the future - Sam
	public float GetRandom(Vector2 bounds)
	{
		return _rng.RandfRange(bounds.X, bounds.Y);
	}
}
