using Godot;
using System;

public partial class MoneyDisplay : Node
{
	[Export] private bool _useDollar = false;
	private Label _label;

	public override void _Ready()
	{
		_label = GetNode<Label>("MoneyAmount");
	}

	public void Display(float money)
	{
		string neg = money < 0 ? "-" : "";
		string dollar = _useDollar ? "$" : "";
		_label.Text = $"{neg}{dollar}{Mathf.Abs(money).ToString("F0")}";
	}


}
