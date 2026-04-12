using Godot;
using System;

public partial class Shop : StaticBody2D
{

	private Panel _control;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_control = GetNode<Panel>("ControlUI");
	}

	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		base._InputEvent(viewport, @event, shapeIdx);

		if (@event.IsActionReleased("MachineUIInteract"))
		{
			if (!_control.Visible)
			{
				SetUIControlVis(true);
			}
		}
	}

	public void SetUIControlVis(bool visible)
	{
		_control.Visible = visible;
	}
	
	public void Drinks()
	{
		GD.Print("Buy everyone a round of drinks");
	}
	
	public void Bouncer()
	{
		GD.Print("Protect Exit");
	}
	
	public void Upgrade3()
	{
		GD.Print("yo");
	}
	
	public void NewMachine()
	{
		GD.Print("new machine");
	}
}
