using Godot;
using System;

public partial class Machine : Node2D
{
	//hii juliaaaa this is now some basic shit needed from customer side lmk if there's anything else ya need me for kthxbye -Sam
	public bool IsAvailable = true;
	public Node2D PlayPosition;
	[Export] public float Cost = 1;
	[Export] public float Profit = 10;

	[Export] public float PlayTime;

	[Export] public float PlayDistance = 100;
	[Signal] public delegate void OnGamePlayedEventHandler(bool won); //signal for when the game is over

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PlayPosition = GetNode<Node2D>("PlayPosition");
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

		EmitSignal(SignalName.OnGamePlayed, false); //TEMP TRUE MEANS ALWAYS WIN!! this casino sucks ASS 

	}
}
