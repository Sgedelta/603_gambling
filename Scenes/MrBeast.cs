using Godot;
using System;

public partial class MrBeast : CharacterBody2D
{
    [Signal] public delegate void MrBeastEnteredEventHandler();
    [Signal] public delegate void MrBeastActionStartedEventHandler();
    [Signal] public delegate void MrBeastActionEndedEventHandler();
    [Signal] public delegate void MrBeastLeftEventHandler();

    private NavigationAgent2D _navAgent;
    [Export] public float Speed = 300.0f;
    private float _movementDelta;

    private Vector2 _targetPos = Vector2.Zero;

    public Vector2 TargetPos
    {
        get { return _navAgent.TargetPosition; }
        set { _navAgent.TargetPosition = value; }
    }

    private GpuParticles2D _particles;
	private bool _MrBeastIsHere = false;
	private Tween _MrBeastTween;

	[Export] private Vector2 _MrBeastEntrancePos = new Vector2(1920, 2300);
	[Export] private Vector2 _MrBeastMoneyThrowingPos = new Vector2(1920, 1700);
	[Export] private Vector2 _MrBeastFinalPos = new Vector2(1920, 700);


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_particles = GetNode<GpuParticles2D>("MoneyParticles");

        _navAgent = GetNode<NavigationAgent2D>("NavAgent");

        //change based on speed & layout
        _navAgent.PathDesiredDistance = 5f;
        _navAgent.TargetDesiredDistance = 5f;

        _navAgent.VelocityComputed += OnVelocityComputed;

        //DEBUG
        TriggerMrBeast();

    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);


        if (_navAgent.IsNavigationFinished())
        {

            return;
        }

        Vector2 currentPos = GlobalTransform.Origin;
        Vector2 nextPathPos = _navAgent.GetNextPathPosition();
        _movementDelta = Speed * (float)delta;

        //set Vel, an internal Character2D thing for MoveAndSlide
        Velocity = currentPos.DirectionTo(nextPathPos) * Speed;
        if (_navAgent.AvoidanceEnabled)
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

    public async void TriggerMrBeast()
	{
        if (_MrBeastIsHere)
        {
            return;
        }
		_MrBeastIsHere = true;

        EmitSignal(SignalName.MrBeastEntered);

		GlobalPosition = _MrBeastEntrancePos;

        TargetPos = _MrBeastMoneyThrowingPos;

        GD.Print("Beast Entered");

        await ToSignal(_navAgent, NavigationAgent2D.SignalName.NavigationFinished);
        await ToSignal(GetTree().CreateTimer(.3f), SceneTreeTimer.SignalName.Timeout);

        _particles.Emitting = true;
        EmitSignal(SignalName.MrBeastActionStarted);

        GD.Print("Beast Throwing");

        await ToSignal(GetTree().CreateTimer(.3f), SceneTreeTimer.SignalName.Timeout);

        TargetPos = _MrBeastFinalPos;

        await ToSignal(_navAgent, NavigationAgent2D.SignalName.NavigationFinished);

        TargetPos = _MrBeastMoneyThrowingPos;

        await ToSignal(_navAgent, NavigationAgent2D.SignalName.NavigationFinished);
        await ToSignal(GetTree().CreateTimer(.3f), SceneTreeTimer.SignalName.Timeout);

        _particles.Emitting = false;
        EmitSignal(SignalName.MrBeastActionEnded);

        GD.Print("Beast Stopped Throwing");

        await ToSignal(GetTree().CreateTimer(.3f), SceneTreeTimer.SignalName.Timeout);

        TargetPos = _MrBeastEntrancePos;

        await ToSignal(_navAgent, NavigationAgent2D.SignalName.NavigationFinished);
        await ToSignal(GetTree().CreateTimer(1.5f), SceneTreeTimer.SignalName.Timeout); //to allow particles to disappear
        EmitSignal(SignalName.MrBeastLeft);

        GD.Print("Beast Left");

        //send to shadow realm to not fuck with other nav agents
        GlobalPosition += new Vector2(5000, 5000);
    }
}
