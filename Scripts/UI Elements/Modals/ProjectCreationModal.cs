using Godot;
using System;

public partial class ProjectCreationModal : BaseModal
{
    [Export] Button createButton;
    [Export] TextEdit titleField;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        createButton.Pressed += () =>
        {
            if (titleField.Text.Length > 0)
            {
                string uid = System.Guid.NewGuid().ToString();
                GD.Print($"Creating new project with uid {uid} and name {titleField.Text}");
                string dirPath = $"user://boards/{uid}";
                if (!DirAccess.DirExistsAbsolute(dirPath))
                {
                    DirAccess.MakeDirRecursiveAbsolute(dirPath);
                }
                BoardData boardData = new(uid);
                ResourceSaver.Save(boardData, $"user://boards/{uid}/data.tres");
                BoardMeta boardMeta = new(titleField.Text, uid);
                ResourceSaver.Save(boardMeta, $"user://boards/{uid}/meta.tres");

                SignalManager.Instance.EmitSignal(SignalManager.SignalName.EnterProject, uid);
            }
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public override void AppearAnimation()
    {
        titleField.Text = "";
        base.AppearAnimation();
    }

    public override void DisappearAnimation()
    {
        base.DisappearAnimation();
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
