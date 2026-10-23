using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum EdgeTypes
{
    BLOCKS,
    FACILITATES,

}

[GlobalClass]
public partial class TaskLink : Resource
{
    [Export] public string SourceUid;
    [Export] public string TargetUid;
    [Export] public EdgeTypes Label;

    public TaskLink() : this("", "", EdgeTypes.BLOCKS) { }
    public TaskLink(string sourceUID, string targetUID, EdgeTypes label)
    {
        SourceUid = sourceUID;
        TargetUid = targetUID;
        Label = label;
    }
}