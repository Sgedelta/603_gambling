using Godot;
using System;

public partial class Machine : Node2D
{
	//hii juliaaaa this is now some basic shit needed from customer side lmk if there's anything else ya need me for kthxbye -Sam
	public bool IsAvailable = true;
	public Node2D PlayPosition;
	[Export] public float Cost = 1;
	[Export] public float Profit = 10;
	[Export] public float WinChance = 0.1f; //Should be between 0 and 1, can adjust as needed

	[Export] public float PlayTime;

	[Export] public float PlayDistance = 100;
	[Signal] public delegate void OnGamePlayedEventHandler(bool won); //signal for when the game is over

    private RandomNumberGenerator rng;
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		PlayPosition = GetNode<Node2D>("PlayPosition");
        rng = new RandomNumberGenerator();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public async void Play(Customer c)
	{
		//trigger an animation or someother visual state change here...

		//wait for game to play (could be DIRECTLY tied to animation or something, if we wanted? but this is easier for now)
		await ToSignal(GetTree().CreateTimer(PlayTime), SceneTreeTimer.SignalName.Timeout);

		//decide if we win
		bool win = rng.Randf() < WinChance;

		//Adjust customer's money and casino's money
		//customer has to spend to even play, always reduce their money by cost
		c.CurrentMoney -= Cost;

		//Add money
		if (win)
		{
			c.CurrentMoney += Profit;
		}

		GD.Print(c.CurrentMoney);

		//Casino no longer sucks ass, returns result to player
		EmitSignal(SignalName.OnGamePlayed, win); 

	}
}
