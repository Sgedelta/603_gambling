using Godot;
using System;

public partial class MachineControlUI : Panel
{

	public Slider CostSlider;
	public Slider PayoutSlider;
	public Slider RateSlider;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		CostSlider = GetNode<Slider>("ControlMargins/VertFlow/CostUI/HSlider");
        PayoutSlider = GetNode<Slider>("ControlMargins/VertFlow/PayoutUI/HSlider");
        RateSlider = GetNode<Slider>("ControlMargins/VertFlow/WinrateUI/HSlider");

    }

	public void SetAllSlidersToValues(float cost, float payout, float rate)
	{
		CostSlider.Value = cost;
		PayoutSlider.Value = payout;
		RateSlider.Value = rate;
	}
}
