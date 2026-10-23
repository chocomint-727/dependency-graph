using Godot;
using System;

public partial class SidePanelHolder : Control
{
    [Export] AnimationPlayer a;
    [Export] TextureButton openCloseButton;
    [Export] public Button menuButton;
    [Export] Button settingsButton;
    [Export] public Button saveButton;
    [Export] public TextEdit titleLine;
    [Export] public TextEdit descriptionBox;
    [Export] CheckBox doneCheckBox;
    [Export] OptionButton severityMenu;
    public TaskDescriptor curDescriptor = null;
    private bool windowOpen;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        windowOpen = true;
        openCloseButton.Pressed += () =>
        {
            if (windowOpen)
            {
                a.Play("ControlMoveOut");
                windowOpen = false;
            }
            else
            {
                a.Play("ControlMoveIn");
                windowOpen = true;
            }
        };

        menuButton.Pressed += () => SignalManager.Instance.EmitSignal(SignalManager.SignalName.ExitProject);
        descriptionBox.TextChanged += OnTextEdit;
        titleLine.TextChanged += OnTitleEdit;
        doneCheckBox.Toggled += OnCheckToggled;
        severityMenu.ItemSelected += OneSeverityChanged;
        severityMenu.Selected = 2;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetDescriptor(TaskDescriptor t)
    {
        if (t == null)
        {
            curDescriptor = null;
            descriptionBox.Text = "";
            titleLine.Text = "";
            doneCheckBox.ButtonPressed = false;
            severityMenu.Selected = 2;
            return;
        }
        curDescriptor = t;
        descriptionBox.Text = curDescriptor.Description;
        titleLine.Text = curDescriptor.Title;
        doneCheckBox.ButtonPressed = curDescriptor.Done;
        severityMenu.Selected = (int)curDescriptor.Severity;
    }

    private void OnTextEdit()
    {
        if (curDescriptor != null)
        {
            curDescriptor.Description = descriptionBox.Text;
            SignalManager.Instance.EmitSignal(SignalManager.SignalName.NodeEdited, curDescriptor.uid);
        }
    }

    private void OnTitleEdit()
    {
        if (curDescriptor != null)
        {
            curDescriptor.Title = titleLine.Text;
            SignalManager.Instance.EmitSignal(SignalManager.SignalName.NodeEdited, curDescriptor.uid);
        }
    }

    private void OnCheckToggled(bool pressed)
    {
        if (curDescriptor != null)
        {
            curDescriptor.Done = pressed;
            SignalManager.Instance.EmitSignal(SignalManager.SignalName.FinishedStatusChange, curDescriptor.uid, curDescriptor.Done);
        }
    }

    private void OneSeverityChanged(long index)
    {
        if (curDescriptor != null)
        {
            curDescriptor.Severity = (TaskSeverities)index;
            SignalManager.Instance.EmitSignal(SignalManager.SignalName.NodeEdited, curDescriptor.uid);
        }
    }
}
