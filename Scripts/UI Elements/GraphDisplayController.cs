using Godot;
using System;
using System.Security;

public partial class GraphDisplayController : Control
{
    [Export] public string uid;
    [Export] public TaskGraph graphDisplay;
    [Export] public TitleLabel titleLabel;
    [Export] public TextureButton addButton;
    [Export] public TextureButton addFrameButton;
    [Export] public CreateNewTasksModal taskModal;
    [Export] public SidePanelHolder sidePanelHolder;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        graphDisplay.LoadFromUID(uid);
        titleLabel.LoadFromUID(uid);
        taskModal.createButton.Pressed += () =>
        {
            graphDisplay.CreateNodeWithUndo(new TaskDescriptor(
                title: taskModal.titleField.Text,
                description: taskModal.descriptionField.Text,
                severity: (TaskSeverities)taskModal.severitySelector.Selected,
                spawnPoint: (graphDisplay.ScrollOffset + graphDisplay.Size / 2) / graphDisplay.Zoom
            ));
            taskModal.DisappearAnimation();
        };

        addButton.Pressed += () => taskModal.AppearAnimation();
        sidePanelHolder.saveButton.Pressed += graphDisplay.SaveBoard;
        sidePanelHolder.menuButton.Pressed += graphDisplay.SaveBoard;
        addFrameButton.Pressed += () =>
        {
            graphDisplay.CreateGroupWithUndo(new TaskGroup());
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
