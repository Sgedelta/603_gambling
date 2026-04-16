using Godot;
using System;
using System.Diagnostics;

public partial class PermDisplay : Label
{
    float amount = 0;

    public override void _Ready()
    {

    }

    public void UpdateMachinePermDisplay(float money)
    {
        amount += money;
        string neg = amount < 0 ? "-" : "";
        this.Text = $"Net Profit\n{neg}{Mathf.Abs(amount).ToString("F0")}";
    }

    public void ResetAmount(float fuckingWhatever)
    {
        amount = 0;
        string neg = amount < 0 ? "-" : "";
        this.Text = $"Net Profit\n{neg}{Mathf.Abs(amount).ToString("F0")}";
    }
}
