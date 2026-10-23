using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class TaskGroup : Resource
{
    [Export] public string Title;
    [Export] public Array<string> TaskUids;
    [Export] public string uid;
    [Export] public Vector2 position;

    public TaskGroup() : this("New Group", Vector2.Zero) { }

    public TaskGroup(string name, Vector2 spawnPoint)
    {
        Title = name;
        TaskUids = new();
        uid = Guid.NewGuid().ToString();
        position = spawnPoint;
    }
}
