using Godot;
using System;

public partial class DeathScreen : CanvasLayer
{
	[Export] Sprite2D Greyscale;
	[Export] Sprite2D YouDied;
	[Export] Sprite2D HealthBar;
	[Export] Sprite2D Thumbnail;
	[Export] Button ReturnToMainMenu;

	private float _time = 1.3f;

	private Tween _animTween;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        GetTree().Paused = true;

		Greyscale.Modulate = new Color(1, 1, 1, 0);
		YouDied.Modulate = new Color(1, 1, 1, 0);
		HealthBar.Modulate = new Color(1, 1, 1, 0);
		Thumbnail.Modulate = new Color(1, 1, 1, 0);
		ReturnToMainMenu.Modulate = new Color(1, 1, 1, 0);

		ReturnToMainMenu.Pressed += () => {
			
			//GameManager.instance.QueueFree(); //"unsafe", but needed


            Callable.From(() =>
			{
                //lambdas inside lambdas 
                //GameManager newGM = (GameManager)ResourceLoader.Load<PackedScene>("res://Scenes/game_manager.tscn").Instantiate();
                //GetTree().Root.AddChild(newGM);
                GetTree().ChangeSceneToFile("res://Scenes/Menus/main_menu.tscn");
                GetTree().Paused = false;
                QueueFree();

            }).CallDeferred();

			

		
		};


        _animTween = CreateTween();

		_animTween.TweenProperty(Greyscale, "modulate", new Color(.2f, .2f, .2f, .75f), 2*_time).From(new Color(.3f, .3f, .3f, 0));
		_animTween.Parallel().TweenProperty(YouDied, "modulate", new Color(1, 1, 1, 1), 2*_time).From(new Color(1, 1, 1, 0)).SetDelay(.3 * _time);

		_animTween.TweenProperty(HealthBar, "modulate", new Color(1, 1, 1, 1), 2.5f * _time).From(new Color(1, 1, 1, 0)).SetDelay(.5f * _time);

		_animTween.TweenProperty(Thumbnail, "modulate", new Color(1, 1, 1, 1), 5f * _time).From(new Color(1, 1, 1, 0)).SetDelay(_time);

		_animTween.Parallel().TweenProperty(ReturnToMainMenu, "modulate", new Color(1, 1, 1, 1), 1.5 * _time).From(new Color(1, 1, 1, 0)).SetDelay(4.5f * _time);
	}
	
}
