using Godot;
using System;

public partial class TitleLabel : Label
{
    // Called when the node enters the scene tree for the first time.
    [Export] public string boardUID;
    BoardMeta boardMeta;
    public override void _Ready()
    {
        // boardMeta = ResourceLoader.Load<BoardMeta>($"user://boards/{boardUID}/meta.tres");
        // Text = boardMeta.Name;
    }

    public void LoadFromUID(string uid)
    {
        boardMeta = ResourceLoader.Load<BoardMeta>($"user://boards/{uid}/meta.tres");
        Text = boardMeta.Name;
    }
}
