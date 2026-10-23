using Godot;
using System;

public partial class UneditableTextWithTitle : VBoxContainer
{
    public Label title;
    public Label field;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        title = GetChild(0) as Label;
        field = GetChild(1) as Label;
    }

    public void SetTitleText(string text)
    {
        title.Text = text;
    }

    public void SetFieldText(string text)
    {
        field.Text = text;
    }
}
