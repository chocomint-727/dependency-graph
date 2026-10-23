using Godot;
using System;

public partial class TaskFrame : GraphFrame
{

    public TaskGroup groupProps;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        GetTitlebarHBox().GetChild(0).QueueFree();
        LineEdit titleBar = new();
        GetTitlebarHBox().AddChild(titleBar);

        titleBar.TextChanged += EditTitle;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void EditTitle(string newText)
    {
        groupProps.Title = newText;
    }
}
