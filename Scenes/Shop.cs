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
		GD.Print("called");
		var mainGame = GetNode<MainGame>("/root/MainGame");
		mainGame.UpdateCasinoMoney(-20);
		GD.Print("worked");
    }

	public void BuyMachine()
	{
		var mainGame = GetNode<MainGame>("/root/MainGame");

		//Call maingame method to unlock
		int nextCost = mainGame.PurchaseMachine();

		//Update display of machine cost
		//If cost is -1, no more machines to buy, disable button
		if (nextCost > 0)
		{
			Label costLabel = GetNode<Label>("ControlUI/VBoxContainer/NewMachine/Upgrade3/HBoxContainer/Label");
			costLabel.Text = nextCost.ToString();
		}
		else
		{
			Button button = GetNode<Button>("ControlUI/VBoxContainer/NewMachine/Upgrade3");
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
