using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

public enum GraphStates
{
    NORMAL,
    ADDING,
    DELETING,
}

public enum PopupOptions
{
    DELETE,
    ADD_LINK,
    REMOVE_LINK
}

public partial class TaskGraph : GraphEdit
{
    [Export] public string boardUID;
    [Export] public Color arrowColor;
    [Export] public GraphDisplayPopup nodeOptionsPopUp;
    [Export] public SidePanelHolder dashboardPanel;
    public BoardData boardData;
    public BoardMeta boardMeta;
    private readonly Dictionary<string, List<string>> adjacencyList = new();
    public IReadOnlyDictionary<string, List<string>> AdjacencyList => adjacencyList;
    private PackedScene _taskCardScene = GD.Load<PackedScene>("res://Scenes/UI Components/TaskCard.tscn");
    private CanvasItem _connectionLayer;
    private UndoRedo _undoRedo = new UndoRedo();
    private GraphStates curState = GraphStates.NORMAL;
    private string _linkSourceUid = null;
    private HashSet<string> finishedUIDs = new();
    private int mostBlocked;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        curState = GraphStates.NORMAL;
        foreach (Node child in GetChildren(includeInternal: true))
        {
            if (child.Name == "CLAYER" || child.Name == "_connection_layer")
            {
                var connections = child.GetSignalConnectionList("draw");
                child.Connect("draw", new Callable(this, nameof(OnConnectionLayerDraw)));
                _connectionLayer = child as CanvasItem;

                connections = child.GetSignalConnectionList("draw");
            }
        }

        EndNodeMove += OnEndNodeMove;

        // when the add or delete pop up menus are clicked, they will send a uid. set the graph states as well as the link source uid
        nodeOptionsPopUp.IdPressed += (id) =>
        {
            if (nodeOptionsPopUp.card == null)
            {
                throw new Exception("Node options pop up came with a null somehow");
            }
            switch (id)
            {
                case (long)PopupOptions.DELETE:
                    RemoveNodeWithUndo(nodeOptionsPopUp.card.descriptor.uid);
                    return;
                case (long)PopupOptions.ADD_LINK:
                    SetSelected(nodeOptionsPopUp.card);
                    _linkSourceUid = nodeOptionsPopUp.card.descriptor.uid;
                    nodeOptionsPopUp.card.SetLinkHighlight(true);
                    curState = GraphStates.ADDING;
                    return;
                case (long)PopupOptions.REMOVE_LINK:
                    SetSelected(nodeOptionsPopUp.card);
                    _linkSourceUid = nodeOptionsPopUp.card.descriptor.uid;
                    nodeOptionsPopUp.card.SetLinkHighlight(true);
                    curState = GraphStates.DELETING;
                    return;
            }

            nodeOptionsPopUp.card = null;
        };

        NodeDeselected += (_) =>
        {
            dashboardPanel.SetDescriptor(null);
            if (curState == GraphStates.NORMAL)
            {
                return;
            }
            CallDeferred(nameof(CancelPendingLink));
        };

        SignalManager.Instance.FinishedStatusChange += OnFinishedUpdate;
        SignalManager.Instance.NodeEdited += UpdateTaskCard;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            bool ctrlOrCmd = key.CtrlPressed || key.MetaPressed; // MetaPressed = Cmd on macOS
            if (key.Keycode == Key.Z && ctrlOrCmd)
            {
                AcceptEvent();
                if (key.ShiftPressed)
                {
                    _undoRedo.Redo();
                }
                else
                {
                    _undoRedo.Undo();
                }
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("Save"))
        {
            SaveBoard();
        }

        if (Input.IsActionJustPressed("SelectAll"))
        {
            foreach (TaskCard card in GetChildren().OfType<TaskCard>())
            {
                card.Selected = true;
            }
        }
    }

    private bool WouldCreateCycle(string from, string to)
    {
        // if a path already exists from `to` back to `from`,
        // adding from -> to would close a cycle
        var visited = new HashSet<string>();
        var stack = new Stack<string>();
        stack.Push(to);

        while (stack.Count > 0)
        {
            string current = stack.Pop();
            if (current == from) return true;
            if (!visited.Add(current)) continue;

            if (adjacencyList.TryGetValue(current, out var neighbors))
            {
                foreach (var next in neighbors)
                {
                    stack.Push(next);
                }
            }
        }

        return false;
    }

    public void SaveBoard()
    {
        boardMeta.LastSaved = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        boardMeta.LastScreenPosition = ScrollOffset;
        boardMeta.LastScreenZoom = Zoom;

        Error e = ResourceSaver.Save(boardData, $"user://boards/{boardUID}/data.tres");
        if (e != Error.Ok)
        {
            GD.PrintErr($"Failed to save board {boardUID}: {e}");
        }

        e = ResourceSaver.Save(boardMeta, $"user://boards/{boardUID}/meta.tres");
        if (e != Error.Ok)
        {
            GD.PrintErr($"Failed to save metadata {boardUID}: {e}");
        }
    }
    public void ClearGraph()
    {
        // Remove all visual TaskCard children — filtering with OfType in case
        // any non-TaskCard UI elements are ever parented here too
        foreach (TaskCard card in GetChildren().OfType<TaskCard>())
        {
            RemoveChild(card);
            card.QueueFree();
        }

        adjacencyList.Clear();
        boardData = null; // or a fresh empty BoardData, depending on what you want mid-load
    }

    public void LoadFromUID(string uid)
    {
        ClearGraph();
        boardUID = uid;
        boardMeta = ResourceLoader.Load<BoardMeta>($"user://boards/{boardUID}/meta.tres");
        boardData = ResourceLoader.Load<BoardData>($"user://boards/{boardUID}/data.tres");
        SpawnSavedNodes();
        SpawnSavedEdges();

        ProcessBlocking();

        Zoom = boardMeta.LastScreenZoom;
        ScrollOffset = boardMeta.LastScreenPosition;
    }

    public void SpawnSavedNodes()
    {
        foreach (TaskDescriptor t in boardData.Tasks)
        {
            SpawnTaskCard(t);
        }
    }

    public void SpawnSavedEdges()
    {
        foreach (TaskLink t in boardData.Links)
        {
            if (!ConnectLocal(t.SourceUid, t.TargetUid))
            {
                GD.PrintErr($"Saved link failed to reconnect on load: {t.SourceUid} -> {t.TargetUid} — possible data corruption or missing node");
            }
        }

    }

    /* 
        local instantiation 
    */

    public void SpawnTaskCard(TaskDescriptor task)
    {
        var card = _taskCardScene.Instantiate<TaskCard>();
        card.Name = task.uid;
        card.descriptor = task;
        if (task.Done)
        {
            finishedUIDs.Add(task.uid);
        }
        AddChild(card);
        card.NodeSelected += () => OnCardSelected(card);
        card.RightClicked += (card) =>
        {
            nodeOptionsPopUp.Position = (Vector2I)GetLocalMousePosition();
            nodeOptionsPopUp.card = card;
            nodeOptionsPopUp.Visible = true;
        };

    }

    public void UpdateTaskCard(string uid)
    {
        var node = GetNodeOrNull<TaskCard>(uid);
        if (node is null)
        {
            GD.PrintErr($"UpdateTaskCard called for a node that doesn't exist: {uid}");
            return;
        }

        node.SetDescriptorFields();
    }

    public bool ConnectLocal(string sourceUID, string targetUID)
    {
        if (!adjacencyList.TryGetValue(sourceUID, out var targets))
        {
            targets = new List<string>();
            adjacencyList[sourceUID] = targets;
        }
        targets.Add(targetUID);
        return true;
    }

    public void SpawnTaskGroup(TaskGroup group)
    {
        var frame = new GraphFrame();
        frame.PositionOffset = group.position;
        frame.Name = group.uid;
        frame.Title = group.Title;
        AddChild(frame);
        foreach (string uid in group.TaskUids)
        {
            var node = GetNodeOrNull<TaskCard>(uid);
            if (node is null)
            {
                GD.PrintErr($"Attempted to attach a node that doesn't exist: {uid}");
            }
            AttachGraphElementToFrame(uid, frame.Name);
        }
    }

    /*
        Durable and consistent creation so that local stuff does not disagree with the saved data
    */

    private void OnEndNodeMove()
    {
        foreach (TaskCard card in GetChildren().OfType<TaskCard>())
        {
            if (card.Selected)
            {
                card.descriptor.position = card.PositionOffset;
            }
        }
        QueueRedraw();
    }

    public bool CreateLink(string sourceUID, string targetUID)
    {
        if (!ConnectLocal(sourceUID, targetUID))
        {
            GD.Print("Error in ConnectVisual");
            return false;
        }
        boardData.Links.Add(new TaskLink { SourceUid = sourceUID, TargetUid = targetUID, Label = EdgeTypes.BLOCKS });
        ProcessBlocking();
        return true;
    }

    public void CreateLinkWithUndo(string sourceUID, string targetUID)
    {
        _undoRedo.CreateAction("Create Link");
        _undoRedo.AddDoMethod(Callable.From(() => { CreateLink(sourceUID, targetUID); _connectionLayer.QueueRedraw(); }));
        _undoRedo.AddUndoMethod(Callable.From(() => { RemoveLink(sourceUID, targetUID); _connectionLayer.QueueRedraw(); }));
        _undoRedo.CommitAction();
    }

    public void RemoveLink(string sourceUID, string targetUID)
    {
        DisconnectNode(sourceUID, 0, targetUID, 0);

        if (adjacencyList.TryGetValue(sourceUID, out var targets))
        {
            targets.Remove(targetUID);
        }

        boardData.Links.RemoveWhere(l => l.SourceUid == sourceUID && l.TargetUid == targetUID);

        ProcessBlocking();
    }
    public void RemoveLinkWithUndo(string sourceUID, string targetUID)
    {
        _undoRedo.CreateAction("Create Link");
        _undoRedo.AddDoMethod(Callable.From(() => { RemoveLink(sourceUID, targetUID); _connectionLayer.QueueRedraw(); }));
        _undoRedo.AddUndoMethod(Callable.From(() => { CreateLink(sourceUID, targetUID); _connectionLayer.QueueRedraw(); }));
        _undoRedo.CommitAction();
    }

    public bool CreateNode(TaskDescriptor task)
    {
        SpawnTaskCard(task);
        boardData.Tasks.Add(task);
        ProcessBlocking();
        return true;
    }

    public void CreateNodeWithUndo(TaskDescriptor task)
    {
        _undoRedo.CreateAction("Create Node");
        _undoRedo.AddDoMethod(Callable.From(() => CreateNode(task)));
        _undoRedo.AddUndoMethod(Callable.From(() => RemoveNode(task.uid)));
        _undoRedo.CommitAction();
    }

    public bool RemoveNode(string uid)
    {
        var node = GetNodeOrNull<TaskCard>(uid);
        if (node is null)
        {
            GD.PrintErr($"RemoveNode called for a node that doesn't exist: {uid}");
            return false;
        }

        var affectedLinks = boardData.Links
            .Where(l => l.SourceUid == uid || l.TargetUid == uid)
            .ToList();

        foreach (var link in affectedLinks)
        {
            RemoveLink(link.SourceUid, link.TargetUid);
        }

        node.QueueFree();
        adjacencyList.Remove(uid);
        boardData.Tasks.RemoveWhere(t => t.uid == uid);

        ProcessBlocking();

        return true;
    }

    private void RestoreNode(TaskDescriptor task, List<TaskLink> links)
    {
        CreateNode(task);
        foreach (var link in links)
        {
            CreateLink(link.SourceUid, link.TargetUid);
        }
    }

    public void RemoveNodeWithUndo(string uid)
    {
        var task = boardData.Tasks.FirstOrDefault(t => t.uid == uid);
        if (task is null) return;

        // capture everything needed to fully reconstruct this node's state
        var affectedLinks = boardData.Links
            .Where(l => l.SourceUid == uid || l.TargetUid == uid)
            .Select(l => new TaskLink { SourceUid = l.SourceUid, TargetUid = l.TargetUid, Label = l.Label })
            .ToList();

        _undoRedo.CreateAction("Delete Task");
        _undoRedo.AddDoMethod(Callable.From(() => RemoveNode(uid)));
        _undoRedo.AddUndoMethod(Callable.From(() => RestoreNode(task, affectedLinks)));
        _undoRedo.CommitAction();
    }

    public void CreateGroup(TaskGroup group)
    {
        SpawnTaskGroup(group);
        boardData.Groups.Add(group);
    }

    public bool RemoveGroup(string uid)
    {
        var group = GetNodeOrNull<GraphFrame>(uid);
        if (group == null)
        {
            GD.PrintErr($"RemoveGroup called for a node that doesn't exist: {uid}");
            return false;
        }

        boardData.Groups.RemoveWhere(t => t.uid == uid);
        group.QueueFree();
        return true;
    }

    public void CreateGroupWithUndo(TaskGroup group)
    {
        _undoRedo.CreateAction("Create Group");
        _undoRedo.AddDoMethod(Callable.From(() => CreateGroup(group)));
        _undoRedo.AddUndoMethod(Callable.From(() => RemoveGroup(group.uid)));
        _undoRedo.CommitAction();
    }

    public void RemoveGroupWithUndo(string uid)
    {
        var group = boardData.Groups.FirstOrDefault(t => t.uid == uid);
        if (group == null)
        {
            GD.PrintErr($"RemoveGroup called for a node that doesn't exist: {uid}");
            return;
        }

        _undoRedo.CreateAction("Remove Group");
        _undoRedo.AddDoMethod(Callable.From(() => RemoveGroup(uid)));
        _undoRedo.AddUndoMethod(Callable.From(() => CreateGroup(group)));
        _undoRedo.CommitAction();
    }

    /*
        Drawing custom floating edges
    */

    private void OnConnectionLayerDraw()
    {
        foreach (TaskLink link in boardData.Links)
        {
            var sourceCard = GetNodeOrNull<TaskCard>(link.SourceUid);
            var targetCard = GetNodeOrNull<TaskCard>(link.TargetUid);
            if (sourceCard is null || targetCard is null) continue;

            var (start, end) = GetEdgeIntersectionPoints(sourceCard, targetCard);
            float arrowLength = 15f;
            float arrowWidth = 20f;

            Vector2 direction = (end - start).Normalized();
            Vector2 perpendicular = new(-direction.Y, direction.X); // 90° rotation

            Vector2 baseCenter = end - direction * arrowLength;
            _connectionLayer.DrawLine(start, baseCenter, arrowColor, 4f, antialiased: true);



            // Pull the tip back slightly so it doesn't visually overlap the target node's edge

            Vector2 baseLeft = baseCenter + perpendicular * (arrowWidth / 2f);
            Vector2 baseRight = baseCenter - perpendicular * (arrowWidth / 2f);

            Vector2[] trianglePoints = { end, baseLeft, baseRight };
            Color[] colors = { arrowColor, arrowColor, arrowColor }; // DrawPolygon requires one color per point

            _connectionLayer.DrawPolygon(trianglePoints, colors);
        }
    }

    private (Vector2 start, Vector2 end) GetEdgeIntersectionPoints(GraphNode source, GraphNode target)
    {
        // Work entirely in UNSCALED graph-content space first —
        // PositionOffset and Size are both already zoom-independent.
        Rect2 sourceRect = new Rect2(source.PositionOffset, source.Size);
        Rect2 targetRect = new Rect2(target.PositionOffset, target.Size);

        Vector2 sourceCenter = sourceRect.Position + sourceRect.Size / 2f;
        Vector2 targetCenter = targetRect.Position + targetRect.Size / 2f;

        Vector2 sourcePoint = GetRectEdgeIntersection(sourceRect, sourceCenter, targetCenter);
        Vector2 targetPoint = GetRectEdgeIntersection(targetRect, targetCenter, sourceCenter);

        // Apply zoom exactly ONCE, at the very end, converting from
        // graph-content space into the CLAYER's draw space.
        float zoom = Zoom;
        return (sourcePoint * zoom, targetPoint * zoom);
    }

    private Vector2 GetRectEdgeIntersection(Rect2 rect, Vector2 from, Vector2 towards)
    {
        Vector2 dir = (towards - from).Normalized();
        float halfW = rect.Size.X / 2f;
        float halfH = rect.Size.Y / 2f;

        float scaleX = dir.X != 0 ? halfW / Mathf.Abs(dir.X) : float.PositiveInfinity;
        float scaleY = dir.Y != 0 ? halfH / Mathf.Abs(dir.Y) : float.PositiveInfinity;
        float scale = Mathf.Min(scaleX, scaleY);

        return from + dir * scale;
    }

    /*
        Edge creation state machine
    */

    private void OnCardSelected(TaskCard card)
    {
        string uid = card.descriptor.uid;
        if (curState == GraphStates.NORMAL)
        {
            dashboardPanel.SetDescriptor(card.descriptor);
            return;
        }
        else if (curState == GraphStates.ADDING)
        {
            if (_linkSourceUid is null)
            {
                throw new Exception("Somehow link source is null in adding state, trace back");
            }
            if (WouldCreateCycle(_linkSourceUid, uid))
            {
                GD.Print("Would create cycle, not allowed"); // Change to Toast TODO
                CancelPendingLink();
                return;
            }

            CreateLinkWithUndo(_linkSourceUid, uid);
        }
        else if (curState == GraphStates.DELETING)
        {
            if (_linkSourceUid is null)
            {
                throw new Exception("Somehow link source is null in deleting state, trace back");
            }

            if (adjacencyList.TryGetValue(_linkSourceUid, out var targets))
            {
                if (targets.Contains(uid))
                {
                    RemoveLinkWithUndo(_linkSourceUid, uid);
                }

            }
            if (adjacencyList.TryGetValue(uid, out targets))
            {
                if (targets.Contains(_linkSourceUid))
                {
                    RemoveLinkWithUndo(uid, _linkSourceUid);
                }

            }
        }
        CancelPendingLink();
        _connectionLayer.QueueRedraw();
    }

    private void CancelPendingLink()
    {
        curState = GraphStates.NORMAL;
        if (_linkSourceUid != null)
        {
            GetNode<TaskCard>(_linkSourceUid).SetLinkHighlight(false);
        }
        _linkSourceUid = null;
    }

    /* 
        Blocking logic
    */

    public void OnFinishedUpdate(string uid, bool done)
    {
        if (done)
        {
            finishedUIDs.Add(uid);
        }
        else
        {
            finishedUIDs.Remove(uid);
        }

        ProcessBlocking();
    }

    public void ProcessBlocking()
    {
        mostBlocked = 0;
        foreach (TaskDescriptor t in boardData.Tasks)
        {
            GetNodeOrNull<TaskCard>(t.uid).SetStatus(false);
        }
        foreach (TaskDescriptor t in boardData.Tasks)
        {
            var card = GetNodeOrNull<TaskCard>(t.uid);
            if (finishedUIDs.Contains(t.uid))
            {
                card.SetBlockingCount(-1);
                continue;
            }
            if (!adjacencyList.TryGetValue(t.uid, out var targets))
            {
                card.SetBlockingCount(0);
                continue;
            }
            card.SetBlockingCount(targets.Count);
            if (targets.Count > mostBlocked)
            {
                mostBlocked = targets.Count;
            }
            foreach (string uid in targets)
            {
                GetNodeOrNull<TaskCard>(uid).SetStatus(true);
            }
        }
    }
}
