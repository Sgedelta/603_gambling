using Godot;
using System;

public partial class Shop : StaticBody2D
{
	private Panel _control;
	private Button adFreeButton;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_control = GetNode<Panel>("ControlUI");
		adFreeButton = GetNode<Button>("ControlUI/VBoxContainer/AdFreeButton");
		GameManager.instance.AdFreePurchased += OnAdFreePurchased;
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
	
	public void Upgrade1()
	{
		GD.Print("called");
		var mainGame = GetNode<MainGame>("/root/MainGame");
		mainGame.UpdateCasinoMoney(-10);
		GD.Print("worked");
	}
	
	public void Upgrade2()
	{
		var bouncer = GetNode<Bouncer>("/root/MainGame/Bouncer");
		int nextCost = bouncer.Purchase();
		Label description = GetNode<Label>("ControlUI/VBoxContainer/Bouncer/Label");
			
		if(nextCost > 0)
		{
			description.Text = "Increase to " + (bouncer.StopTime + 0.2f).ToString("F1");
			
			Label costLabel = GetNode<Label>("ControlUI/VBoxContainer/Bouncer/Upgrade2/HBoxContainer/Label");
			costLabel.Text = nextCost.ToString();
		}
		else
		{
			description.Text = "Maxed Out";
			Button button = GetNode<Button>("ControlUI/VBoxContainer/Bouncer/Upgrade2");
			button.Disabled = true;
		}
	}
	
	public void AdFree()
	{
		var payment = GD.Load<PackedScene>("res://Scenes/AD_Microtransaction.tscn").Instantiate();
		payment.ProcessMode = Node.ProcessModeEnum.Always;
		GetTree().Root.AddChild(payment);
	}
	
	private void OnAdFreePurchased()
	{
		adFreeButton.Disabled = true;
	}
}
