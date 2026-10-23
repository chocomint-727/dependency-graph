using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class BoardMeta : Resource
{
    [Export] public string BoardUID;
    [Export] public string Name;
    [Export] public long CreatedAt;
    [Export] public long LastSaved;
    [Export] public Vector2 LastScreenPosition;
    [Export] public float LastScreenZoom;
    [Export] public Dictionary<string, bool> projectTags;

    public BoardMeta() : this("New Project", "test_uid") { }

    public BoardMeta(string name, string uid)
    {
        Name = name;
        BoardUID = uid;
        CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        LastSaved = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        projectTags = new();

        LastScreenPosition = Vector2.Zero;
        LastScreenZoom = 1.0f;
    }
}
