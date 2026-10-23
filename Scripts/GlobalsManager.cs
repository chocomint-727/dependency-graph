using Godot;
using System;

public partial class GlobalsManager : Node
{
    public static GlobalsManager Instance;
    private static string settingsPath = $"user://settings.tres";
    public StateSettings settings;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        settings = ResourceLoader.Load<StateSettings>(settingsPath);
        Instance = this;
        SignalManager.Instance.SettingsEdited += Save;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private void Save()
    {
        Error e = ResourceSaver.Save(settings, settingsPath);
        if (e != Error.Ok)
        {
            GD.PrintErr($"Failed to save board settings: {e}");
            return;
        }
        SignalManager.Instance.EmitSignal(SignalManager.SignalName.SettingsUpdated);
    }
}
