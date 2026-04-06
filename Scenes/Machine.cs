using Godot;
using System;

public partial class Machine : Node2D
{
	//hii juliaaaa this is now some basic shit needed from customer side lmk if there's anything else ya need me for kthxbye -Sam
	public bool IsAvailable = true;
	public Vector2 PlayPosition { get { return _playArea.GlobalPosition; } }
	[Export] public float Cost = 1;
	[Export] public float Profit = 10;
	[Export] public float WinChance = 0.1f; //Should be between 0 and 1, can adjust as needed

	[Export] public float PlayTime;

	[Export] public float PlayDistance = 100;
	[Signal] public delegate void OnGamePlayedEventHandler(bool won); //signal for when the game is over
	[Signal] public delegate void OnCasinoMoneyChangeEventHandler(float amount); //Used to display the amount to adjust casino money by

    private RandomNumberGenerator _rng;
	private Area2D _playArea;
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        _rng = new RandomNumberGenerator();
		_playArea = GetNode<Area2D>("PlayArea");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void Play(Customer c)
	{
        //trigger an animation or someother visual state change here...

        //customer has to spend to even play, always reduce their money by cost
        //Adjust customer's money and casino's money
        c.CurrentMoney -= Cost;
        EmitSignal(SignalName.OnCasinoMoneyChange, Cost);

        //wait for game to play (could be DIRECTLY tied to animation or something, if we wanted? but this is easier for now)
        await ToSignal(GetTree().CreateTimer(PlayTime), SceneTreeTimer.SignalName.Timeout);

		//decide if we win
		bool win = _rng.Randf() < WinChance;

		//Add money
		if (win)
		{
			c.CurrentMoney += Profit;

			//casino loses money
			EmitSignal(SignalName.OnCasinoMoneyChange, -Profit);
		}

		//Casino no longer sucks ass, returns result to player
		EmitSignal(SignalName.OnGamePlayed, win); 

	}

	public bool IsCustomerInPlayArea(Customer c)
	{
		return _playArea.OverlapsBody(c);
	}
}
