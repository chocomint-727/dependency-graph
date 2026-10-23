using Godot;
using System;
using System.Runtime.Serialization;

public partial class ProjectCard : PanelContainer
{
    private BoardMeta meta;
    [Export] private Label titleField;
    [Export] private HFlowContainer tagList;
    private PackedScene tagContainer = GD.Load<PackedScene>("res://Scenes/UI Components/TagContainer.tscn");

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        titleField.Text = meta.Name;
        foreach (string tagUID in meta.projectTags.Keys)
        {
            var tagLabel = tagContainer.Instantiate<TagContainer>();
            tagLabel.tagUID = tagUID;
            tagList.AddChild(tagLabel);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void SetMeta(BoardMeta to)
    {
        meta = to;
    }

    public void AddTag(string tagUID)
    {
        meta.projectTags[tagUID] = true;
        Save();
    }

    public bool RemoveTag(string tagUID)
    {
        var tagLabel = GetNodeOrNull<TagContainer>(tagUID);
        if (tagLabel == null)
        {
            GD.Print("No child with tag name found");
        }
        if (meta.projectTags.Remove(tagUID))
        {
            GD.Print("Tag removed successfully");
            Save();
            return true;
        }
        else
        {
            GD.Print("Tag not found in project tag list");
            return false;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left && mb.Pressed)
            {
                AcceptEvent();
                GD.Print("I've been clicked D:");
                SignalManager.Instance.EmitSignal(SignalManager.SignalName.EnterProject, Name);
            }
        }
    }

    public bool Save()
    {
        Error e = ResourceSaver.Save(meta, $"user://boards/{Name}/meta.tres");
        if (e != Error.Ok)
        {
            GD.PrintErr($"Failed to save metadata {Name}: {e}");
            return false;
        }
        return true;
    }
}
