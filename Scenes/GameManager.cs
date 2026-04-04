using Godot;
using System;

public partial class GameManager : Node
{

    public static GameManager instance;
    public GameManager GDInstance { get { return instance; } }


    public MainGame ActiveMainGame;


    public override void _EnterTree()
    {
        //singleton
        if( instance == null )
        {
            instance = this;
        }
        else
        {
            GD.PushWarning("Second Game Manager Detected, Singleton Activated! Deleting " + Name);
            QueueFree();
        }
    }

    public override void _Ready()
    {
        
    }
}
