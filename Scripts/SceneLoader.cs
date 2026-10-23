using Godot;
using System;

public partial class SceneLoader : CanvasLayer
{
    [Export] private AnimationPlayer a;
    private PackedScene graphScreen = GD.Load<PackedScene>("res://Scenes/UI Components/GraphDisplay.tscn");
    private string graphScreenName = "graphScreen";
    private PackedScene lobbyScreen = GD.Load<PackedScene>("res://Scenes/UI Components/WelcomeScreen.tscn");
    private string lobbyScreenName = "lobbyScreen";

    private Node curScene;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SignalManager.Instance.EnterProject += OnEnterProject;
        SignalManager.Instance.ExitProject += OnExitProject;
        OnExitProject();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public async void OnEnterProject(string uid)
    {
        a.Play("CloseCurtains");
        await ToSignal(a, AnimationMixer.SignalName.AnimationFinished);
        if (curScene != null)
        {
            curScene.QueueFree();
            curScene = null;
        }
        var gdc = graphScreen.Instantiate<GraphDisplayController>();
        gdc.uid = uid;
        gdc.Name = graphScreenName;
        AddChild(gdc);
        curScene = gdc;
        a.Play("OpenCurtains");
    }

    public async void OnExitProject()
    {
        a.Play("CloseCurtains");
        await ToSignal(a, AnimationMixer.SignalName.AnimationFinished);
        if (curScene != null)
        {
            curScene.QueueFree();
            curScene = null;
        }
        var lobby = lobbyScreen.Instantiate<WelcomeScreen>();
        lobby.Name = lobbyScreenName;
        AddChild(lobby);
        curScene = lobby;
        a.Play("OpenCurtains");
    }

}
