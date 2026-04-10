using Godot;
using System;

public partial class BeastSpawner : Node2D
{
	private ProgressBar _beastBar;
	[Export] private float _beastInterval = 90f;
	public float BeastInterval { get { return _beastInterval; } set { UpdateInterval(value);  } }

	[Export] private MrBeast _mrBeast;

	Tween BarTween;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_beastBar = GetNode<ProgressBar>("BeastUI/BeastBar");

		_mrBeast.MrBeastLeft += StartBar;

		StartBar();
		
	}

	private void StartBar(float precomputedProgressPercent = 0)
	{
		if(BarTween != null && BarTween.IsRunning())
		{
			BarTween.Kill();
		}
		BarTween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);

		BarTween.TweenProperty(_beastBar, "value", 100, _beastInterval).From(0);

		BarTween.CustomStep(_beastInterval * precomputedProgressPercent);

		BarTween.Finished += _mrBeast.TriggerMrBeast;

	}

	public void StartBar()
	{
		StartBar(0);
	}

	private void UpdateInterval(float newInterval)
	{
		_beastInterval = newInterval;

		float currTime = 0;
		if(BarTween != null && BarTween.IsRunning())
		{
			currTime = (float)BarTween.GetTotalElapsedTime();
		}

		StartBar(currTime / newInterval);
	}
}
