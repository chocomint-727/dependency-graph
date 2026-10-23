using Godot;
using System;

public partial class BaseModal : Panel
{
    [Export] public AnimationPlayer a;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public virtual void AppearAnimation()
    {
        Visible = true;
    }

    public virtual void DisappearAnimation()
    {
        Visible = false;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
        {
            AcceptEvent(); // consume it so it doesn't also trigger native context stuff
            DisappearAnimation();
        }
    }
}
