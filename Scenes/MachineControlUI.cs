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
		CostSlider = GetNode<Slider>("ControlMargins/VertFlow/CostUI/HSlider");
		PayoutSlider = GetNode<Slider>("ControlMargins/VertFlow/PayoutUI/HSlider");
		RateSlider = GetNode<Slider>("ControlMargins/VertFlow/WinrateUI/HSlider");


        CostLabel = GetNode<Label>("ControlMargins/VertFlow/CostUI/HBoxContainer/Val Label");
        PayoutLabel = GetNode<Label>("ControlMargins/VertFlow/PayoutUI/HBoxContainer/Val Label");
        RateLabel = GetNode<Label>("ControlMargins/VertFlow/WinrateUI/HBoxContainer/Val Label");
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
