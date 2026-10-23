using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass]
public partial class TaskDescriptor : Resource
{
    [Export] public string Title;
    [Export] public string Description;
    [Export] public TaskSeverities Severity;
    [Export] public bool Done;
    [Export] public string uid;
    [Export] public Vector2 position;

    public TaskDescriptor() : this(null, null, TaskSeverities.MID) { }

    public TaskDescriptor(string title, string description, TaskSeverities severity)
    {
        Title = title;
        Description = description;
        Severity = severity;
        Done = false;
        uid = System.Guid.NewGuid().ToString();
        position = Vector2.Zero;
    }

    public TaskDescriptor(string title, string description, TaskSeverities severity, Vector2 spawnPoint)
    {
        Title = title;
        Description = description;
        Severity = severity;
        Done = false;
        uid = System.Guid.NewGuid().ToString();
        position = spawnPoint;
    }
}