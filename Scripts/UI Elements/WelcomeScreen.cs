using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WelcomeScreen : Node
{
    [Export] GridContainer boardsList;
    [Export] TextureButton newProjectButton;
    [Export] ProjectCreationModal newProjModal;
    // [Export] AddTagButton
    private PackedScene _projectCard = GD.Load<PackedScene>("res://Scenes/UI Components/ProjectCard.tscn");


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        foreach (BoardMeta meta in WalkFolder())
        {
            CreateTitleLabel(meta);
        }

        newProjectButton.Pressed += () => newProjModal.AppearAnimation();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public List<BoardMeta> WalkFolder()
    {
        List<BoardMeta> metadata = new();
        string[] uids = DirAccess.GetDirectoriesAt("user://boards");
        foreach (string fileName in uids)
        {
            metadata.Add(ResourceLoader.Load<BoardMeta>($"user://boards/{fileName}/meta.tres"));
        }

        return metadata.OrderByDescending(m => m.LastSaved).ToList();
    }

    public void CreateTitleLabel(BoardMeta metadata)
    {
        var titleCard = _projectCard.Instantiate<ProjectCard>();
        titleCard.Name = metadata.BoardUID;
        titleCard.SetMeta(metadata);
        titleCard.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        // titleCard.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        boardsList.AddChild(titleCard);
    }
}
