using Godot;
using System;

public partial class MachineControlUI : Panel
{
	public Slider CostSlider;
	public Slider PayoutSlider;
	public Slider RateSlider;

	public Label CostLabel;
    public Label PayoutLabel;
    public Label RateLabel;



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		CostSlider = GetNode<Slider>("ControlMargins/VertFlow/Upgrade1/CostInteracts/HSlider");
		PayoutSlider = GetNode<Slider>("ControlMargins/VertFlow/Upgrade2/PayoutInteracts/HSlider");
		RateSlider = GetNode<Slider>("ControlMargins/VertFlow/Upgrade3/WinrateInteracts/HSlider");


        CostLabel = GetNode<Label>("ControlMargins/VertFlow/Upgrade1/CostUI/Val Label");
        PayoutLabel = GetNode<Label>("ControlMargins/VertFlow/Upgrade2/PayoutUI/Val Label");
        RateLabel = GetNode<Label>("ControlMargins/VertFlow/Upgrade3/WinrateUI/Val Label");
    }

	public void SetAllSlidersToValues(float cost, float payout, float rate)
	{
		CostSlider.Value = cost;
		PayoutSlider.Value = payout;
		RateSlider.Value = rate;

		CostLabel.Call("set_formatted_text", cost);
        PayoutLabel.Call("set_formatted_text", payout);
        RateLabel.Call("set_formatted_text", rate);
    }
}
