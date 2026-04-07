using Godot;
using System;

public partial class MoneyDisplay : Node
{

	private Label _label;

	public override void _Ready()
	{
		_label = GetNode<Label>("MoneyAmount");
	}

	public void Display(float money)
	{
		string neg = money < 0 ? "-" : "";
		_label.Text = $"{neg}${money.ToString("F0")}";
	}


}
