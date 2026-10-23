using Godot;
using System;

public partial class TagContainer : Node
{
    [Export] public Label text;
    [Export] public TextureRect icon;
    public string tagUID;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (GlobalsManager.Instance.settings.projectTagRegistry.TryGetValue(tagUID, out TagType tag))
        {
            SetFields(tag);
        }
        else
        {
            GD.Print($"tag with uid {tagUID} was not found, deleting label");
            QueueFree();
        }
        SignalManager.Instance.SettingsUpdated += () =>
        {
            if (GlobalsManager.Instance.settings.projectTagRegistry.TryGetValue(tagUID, out TagType tag))
            {
                SetFields(tag);
            }
            else
            {
                GD.Print($"tag with uid {tagUID} was deleted, deleting label");
                QueueFree();
            }
        };
    }

    public void SetFields(TagType tag)
    {
        text.Text = tag.tagName;
        icon.Modulate = tag.tagColor;
    }
}
