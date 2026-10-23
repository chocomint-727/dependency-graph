using Godot;
using System;

public partial class StateSettings : Resource
{
    [Export] public Godot.Collections.Dictionary<string, TagType> projectTagRegistry;
    [Export] public Godot.Collections.Dictionary<string, TagType> masterNodeTagRegistry;
    public StateSettings()
    {
        projectTagRegistry = new();
        masterNodeTagRegistry = new();
    }
}
