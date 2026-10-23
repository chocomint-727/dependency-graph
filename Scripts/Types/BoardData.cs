using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[GlobalClass]
public partial class BoardData : Resource
{
    [Export] public string BoardUID;
    [Export] public Godot.Collections.Array<TaskDescriptor> Tasks;
    [Export] public Godot.Collections.Array<TaskLink> Links;
    [Export] public Godot.Collections.Array<TaskGroup> Groups;

    public BoardData() : this("test_uid") { }

    public BoardData(string uid)
    {
        BoardUID = uid;
        Tasks = new();
        Links = new();
        Groups = new();
    }
}
