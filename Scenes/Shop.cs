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
			GD.Print(BarUpgradeVals.Count);
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

	public void CloseShop()
	{
		SetUIControlVis(false);
	}

	public void CloseBar()
	{
		GetNode<Panel>("BarOnboard").Visible = false;
	}
	
	public void UpgradeBar()
	{
		MainGame mg = GameManager.instance.ActiveMainGame;
		Array<float> upgradeInfo = BarUpgradeVals[_barUpgrade];

		//check money
		if (mg.CasinoMoney < upgradeInfo[0])
		{
			//failed
			return;
		}

		//take money
		mg.UpdateCasinoMoney(-upgradeInfo[0]);

		//actually upgrade
		_barUpgrade += 1;
		Label buttonLabel = GetNodeOrNull<Label>("ControlUI/VBoxContainer/Drinks/UpgradeButton/HBoxContainer/Label");

		//update button
		if (_barUpgrade != BarUpgradeVals.Count)
		{
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
			if (IsInstanceValid(buttonLabel))
			{
				buttonLabel.Text = "Maxed Out";
			}
		}

		TextureProgressBar drinkBar = GetNodeOrNull<TextureProgressBar>("ControlUI/VBoxContainer/Drinks/HBoxContainer/drinkBar");
		//open bar if first upgrade
		if (_barUpgrade == 1) //1 not 0 because it gets incremented earlier. easier to check conditions up there with it added
		{
			GetNode<Panel>("BarOnboard").Visible = true;
			GD.Print("yo");
			drinkBar.Visible = true;
			mg.Bar.IsOpen = true;
			return;
		}

		//upgrade bar if any other upgrade
		mg.Bar.DrinkCost = upgradeInfo[1];
		mg.Bar.DrinkHopeStr = upgradeInfo[2];
		mg.Bar.DrinkAddictionStr = upgradeInfo[3];
		drinkBar.Value += 20;
	}
	
	public void UpgradeBouncer()
	{
		var bouncer = GetNode<Bouncer>("/root/MainGame/Bouncer");
		int nextCost = bouncer.Purchase();
		
		Label description = GetNode<Label>("ControlUI/VBoxContainer/Bouncer/Label");
		Label costLabel = GetNode<Label>("ControlUI/VBoxContainer/Bouncer/Upgrade2/HBoxContainer/Label");
		
		description.Text = "Detains for " + (bouncer.StopTime).ToString("F1") + "s";
		
		if(nextCost > 0)
		{
			costLabel.Text = nextCost.ToString();
		}
		else
		{
			costLabel.Text = "Maxed Out";
			Button button = GetNode<Button>("ControlUI/VBoxContainer/Bouncer/Upgrade2");
			button.Disabled = true;
		}
	}
	
	public void UpgradeAds()
	{
		var entrance = GetNode<CasinoEntrance>("/root/MainGame/CasinoEntrance");
		bool purchased = entrance.Purchase();
		int nextCost = entrance.GetNextCost();
		
		Label costLabel = GetNode<Label>("ControlUI/VBoxContainer/Advertisement/AdButton/HBoxContainer/Label");
		TextureProgressBar adBar = GetNodeOrNull<TextureProgressBar>("ControlUI/VBoxContainer/Advertisement/HBoxContainer/adBar");
	
		if (purchased)
		{
			adBar.Visible = true;
			adBar.Value += 20;
		}
		if (nextCost > 0)
		{
			costLabel.Text = nextCost.ToString();
		}
		else
		{
			Button button = GetNode<Button>("ControlUI/VBoxContainer/Advertisement/AdButton");
			button.Disabled = true;
			costLabel.Text = "Maxed Out";
			adBar.Value = 100;
		}
	}

	public void BuyMachine()
	{
		var mainGame = GetNode<MainGame>("/root/MainGame");

		//Call maingame method to unlock
		int nextCost = mainGame.PurchaseMachine();
		Label costLabel = GetNode<Label>("ControlUI/VBoxContainer/NewMachine/Upgrade3/HBoxContainer/Label");

		//Update display of machine cost
		//If cost is -1, no more machines to buy, disable button
		if (nextCost > 0)
		{
			costLabel = GetNode<Label>("ControlUI/VBoxContainer/NewMachine/Upgrade3/HBoxContainer/Label");
			costLabel.Text = nextCost.ToString();
		}
		else
		{
			Button button = GetNode<Button>("ControlUI/VBoxContainer/NewMachine/Upgrade3");
			button.Disabled = true;
			costLabel.Text = "Maxed Out";
		}
	}
	
	public void AdFree()
	{
		var payment = GD.Load<PackedScene>("res://Scenes/AD_Microtransaction.tscn").Instantiate();
		payment.ProcessMode = Node.ProcessModeEnum.Always;
		
		GameManager.instance.ActiveMainGame.MicrotransactionOpen = true;
		GetTree().Paused = true;
		GetTree().Root.AddChild(payment);
	}
	
	private void OnAdFreePurchased()
	{
		adFreeButton.Disabled = true;
	}
}
