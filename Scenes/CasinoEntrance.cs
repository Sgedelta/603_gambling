using Godot;
using System;

public partial class CasinoEntrance : Node2D
{

	[Export] private PackedScene _customerScene;


	[Export] private float _spawnTickSpeed = 1f;
	public float SpawnTickSpeed { get { return _spawnTickSpeed; } set { _spawnTickSpeed = value; }}
	[Export] private float _spawnChancePerTick = .05f;
	public float SpawnChancePerTick { get { return _spawnChancePerTick; } set { _spawnChancePerTick = value; }}

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

	public void TrySpawnCustomer()
	{
		if(_rng.Randf() <= _spawnChancePerTick)
		{
			//Create customer here

			//emit signal?
		}


		_spawnTimer.Start(_spawnTickSpeed);

	}
}
