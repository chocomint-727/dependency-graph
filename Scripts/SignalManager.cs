using Godot;
using System;

public partial class SignalManager : Node
{
    [Signal] public delegate void ProjectCardClickedEventHandler(string uid);
    [Signal] public delegate void DeleteNodeEventHandler(string uid);
    [Signal] public delegate void EnterProjectEventHandler(string uid);
    [Signal] public delegate void ExitProjectEventHandler();
    [Signal] public delegate void FinishedStatusChangeEventHandler(string uid, bool done);
    [Signal] public delegate void NodeEditedEventHandler(string uid);
    [Signal] public delegate void SettingsEditedEventHandler();
    [Signal] public delegate void SettingsUpdatedEventHandler();
    public static SignalManager Instance;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }
}
