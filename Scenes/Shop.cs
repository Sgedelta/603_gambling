using Godot;
using Godot.Collections;
using System;

public partial class Shop : StaticBody2D
{

	private Panel _control;
	private Button adFreeButton;

	/// <summary>
	/// Bar Upgrade Values. Is always Cost, new Drink Cost, new Drink Hope, new Drink Addiction. First entry is unlock.
	/// </summary>
	[Export] public Array<Array<float>> BarUpgradeVals = new Array<Array<float>>();
	private int _barUpgrade = 0;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_control = GetNode<Panel>("ControlUI");
		adFreeButton = GetNode<Button>("ControlUI/VBoxContainer/AdFreeButton");
		GameManager.instance.AdFreePurchased += OnAdFreePurchased;

        //set initial upgrade vals
        Label upgradeLabel = GetNodeOrNull<Label>("ControlUI/VBoxContainer/Drinks/UpgradeButton/HBoxContainer/Label");
        if (IsInstanceValid(upgradeLabel))
        {
            upgradeLabel.Text = BarUpgradeVals[0][0].ToString();
        }
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

	public void UpgradeBar()
	{
		MainGame mg = GameManager.instance.ActiveMainGame;
		Array<float> upgradeInfo = BarUpgradeVals[_barUpgrade];

		//check money
		if(mg.CasinoMoney < upgradeInfo[0])
		{
			//failed
			return;
		}

		//take money
		mg.UpdateCasinoMoney(-upgradeInfo[0]);

		//actually upgrade
		_barUpgrade += 1;

		//update button
		if(_barUpgrade != BarUpgradeVals.Count)
		{
            Label buttonLabel = GetNodeOrNull<Label>("ControlUI/VBoxContainer/Drinks/UpgradeButton/HBoxContainer/Label");
            if (IsInstanceValid(buttonLabel))
            {
				buttonLabel.Text = BarUpgradeVals[_barUpgrade][0].ToString();
            }
        } 
		else
		{
			//last upgrade, disable the button
			Button upgradeButton = GetNodeOrNull<Button>("ControlUI/VBoxContainer/Drinks/UpgradeButton");
			if (IsInstanceValid(upgradeButton))
			{
				upgradeButton.Disabled = true;
			}
            Label buttonLabel = GetNodeOrNull<Label>("ControlUI/VBoxContainer/Drinks/UpgradeButton/HBoxContainer/Label");
            if (IsInstanceValid(buttonLabel))
            {
                buttonLabel.Text = "Max Upgrades Reached!";
            }
        }


		//open bar if first upgrade
		if(_barUpgrade == 0)
		{
			mg.Bar.IsOpen = true;
			_barUpgrade += 1;
			return;
		}

		//upgrade bar if any other upgrade
		mg.Bar.DrinkCost = upgradeInfo[1];
		mg.Bar.DrinkHopeStr = upgradeInfo[2];
		mg.Bar.DrinkAddictionStr = upgradeInfo[3];

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
