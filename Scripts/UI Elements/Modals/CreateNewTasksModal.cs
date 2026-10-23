using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class CreateNewTasksModal : BaseModal
{
    [Export] public TextEdit titleField;
    [Export] public TextEdit descriptionField;
    [Export] public OptionButton severitySelector;
    [Export] public Button createButton;
    [Export] public Button cancelButton;

    private Dictionary curDate;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Visible = false;
        curDate = Time.GetDateDictFromSystem();
        cancelButton.Pressed += () => DisappearAnimation();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetFieldsForEditing(TaskDescriptor task)
    {
        titleField.Text = task.Title;
        descriptionField.Text = task.Description;
    }

    public override void AppearAnimation()
    {
        GD.Print("appear called");
        severitySelector.Selected = 2;
        titleField.Text = "";
        descriptionField.Text = "";
        base.AppearAnimation();
    }

    public override void DisappearAnimation()
    {
        GD.Print("disappear called");
        base.DisappearAnimation();
    }
}
