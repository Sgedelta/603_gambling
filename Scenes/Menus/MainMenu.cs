using Godot;
using System;

public partial class MainMenu : MarginContainer
{

    [Export] public PackedScene Onboarding;

    public void OnboardingTransition()
    {
        GetTree().ChangeSceneToPacked(Onboarding);
    }
}