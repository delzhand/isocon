using UnityEngine;
using UnityEngine.UIElements;

public class MapEditingState : TabletopSubstate
{
    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        Block.DeselectAll();
        Token.DeselectAll();
        Token.UnfocusAll();
        Block.ToggleSpacers(true);
        BlockMesh.ToggleAllBorders(true);
        Cursor.Mode = CursorMode.Editing;
        Player.Self().SetOp("Editing Map");
        Tutorial.Init("Edit Mode");
    }

    public override void OnExit()
    {
        base.OnExit();
        Block.ToggleSpacers(false);
        BlockMesh.ToggleAllBorders(false);
        Player.Self().ClearOp();
        MapSync();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        Pointer.PointAtBlocks(BlockFocusMode.Single);
    }


    protected override void EnableInterface()
    {
        base.EnableInterface();
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Info"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Sync"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), false);
        UI.ToggleDisplay("ToolsPanel", true);
        UI.ToggleDisplay("DiceRoller", false);
        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), false);
        UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), true);
        UI.TopBar.Q("EditMap").Q<Label>("Label").text = "Save <u>M</u>ap";
    }

    protected override void DisableInterface()
    {
        base.DisableInterface();
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), false);
        UI.TopBar.Q("EditMap").Q<Label>("Label").text = "Edit <u>M</u>ap";
    }

    private void MapSync()
    {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        Player.Self().CmdMapSync(json);
    }

    protected override void BindCallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToNeutral);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>(ChangeDragMode);
    }

    protected override void UnbindCallbacks()
    {
        UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToNeutral);
        UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("DragMode").UnregisterCallback<ClickEvent>(ChangeDragMode);
    }

    protected override void HandleKeypresses()
    {
        base.HandleKeypresses();
        if (Input.GetKeyUp(KeyCode.M))
        {
            GoToNeutral(new ClickEvent());
            return;
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            GoToMarking(new ClickEvent());
            return;
        }
    }
}

public enum BlockFocusMode
{
    Single,
    Row,
    Column
}