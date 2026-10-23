using Godot;
using System;

public partial class TaskCard : GraphNode
{
    [Export] public TaskDescriptor descriptor;
    [Export] TextEdit titleText;

    [Export] UneditableTextWithTitle blocking;
    [Export] UneditableTextWithTitle dueDate;
    [Export] UneditableTextWithTitle status;
    [Export] public Color highlightColor;
    [Signal] public delegate void RightClickedEventHandler(TaskCard card);

    private StyleBoxFlat _highlightStyle;
    private StyleBoxFlat _normalStyle;
    private int blockingCount;
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Right)
        {
            AcceptEvent(); // consume it so it doesn't also trigger native context stuff
            EmitSignal(SignalName.RightClicked, this);
        }
    }


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetDescriptorFields();
        dueDate.SetTitleText("Severity");
        status.SetTitleText("Status");

        titleText.TextChanged += () => descriptor.Title = titleText.Text;


        _normalStyle = GetThemeStylebox("titlebar_selected") as StyleBoxFlat;

        _highlightStyle = (StyleBoxFlat)_normalStyle.Duplicate();
        _highlightStyle.BgColor = highlightColor;
        _highlightStyle.SetBorderWidthAll(3);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    public void SetLinkHighlight(bool on)
    {
        AddThemeStyleboxOverride("titlebar_selected", on ? _highlightStyle : _normalStyle);
    }

    public void SetBlockingCount(int outDeg)
    {
        blockingCount = outDeg;
        if (outDeg == -1)
        {
            blocking.SetFieldText("Done!");
            return;
        }
        blocking.SetFieldText(outDeg.ToString());
    }

    public void SetStatus(bool blocked)
    {
        status.SetFieldText(blocked ? "Blocked" : "Open");
    }

    public bool IsDone()
    {
        return descriptor.Done;
    }

    public void SetDescriptorFields()
    {
        titleText.Text = descriptor.Title;
        dueDate.SetFieldText(descriptor.Severity.ToString());
        PositionOffset = descriptor.position;
    }
}
