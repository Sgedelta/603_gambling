using Godot;
using System;

public partial class Customer : CharacterBody2D
{
	private NavigationAgent2D _navAgent;

	[Export] public float Speed = 300.0f;

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

		//set Vel, an internal Character2D thing for MoveAndSlide
		Velocity = currentPos.DirectionTo(nextPathPos) * Speed;

		MoveAndSlide();
	}

}
