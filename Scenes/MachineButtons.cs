using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class MachineButtons : Button
{
    public void OnHover()
    {
        GetNode<HBoxContainer>("HBoxContainer").Visible = true;
    }

    public void OnHoverExit()
    {
        GetNode<HBoxContainer>("HBoxContainer").Visible = false;
    }
}
