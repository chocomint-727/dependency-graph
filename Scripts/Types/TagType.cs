using Godot;
using System;

public partial class TagType : Resource
{
    // Called when the node enters the scene tree for the first time.
    [Export] public string tagName;
    [Export] public Color tagColor;
    [Export] public string uid;

    public TagType() : this("New Tag", new Color(1.0f, 1.0f, 1.0f)) { }

    public TagType(string name, Color color)
    {
        tagName = name;
        tagColor = color;

    }
}
