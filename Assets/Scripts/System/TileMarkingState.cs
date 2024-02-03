using UnityEngine;
using UnityEngine.UIElements;

public class TileMarkingState : TabletopSubstate
{
    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        Token.DeselectAll();
        Token.UnfocusAll();
        BlockRendering.ToggleAllBorders(true);
        // Cursor.Mode = CursorMode.Marking;
        Player.Self().SetOp("Marking Tiles");
        Tutorial.Init("Marking Mode");
    }

    public override void UpdateState()
    {
        base.UpdateState();
        Pointer.PointAtBlocks();
        SelectionMenu.Update();
    }

    protected override void EnableInterface()
    {
        base.EnableInterface();
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Info"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Sync"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Isocon"), false);
        UI.ToggleDisplay("DiceRoller", false);
        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), false);
        UI.ToggleActiveClass(UI.TopBar.Q("MarkerMode"), true);
        UI.TopBar.Q("MarkerMode").Q<Label>("Label").text = "Stop Marking <u>T</u>iles";
    }

    protected override void DisableInterface()
    {
        base.DisableInterface();
        UI.ToggleDisplay(UI.TopBar.Q("Isocon"), true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), true);
        UI.ToggleActiveClass(UI.TopBar.Q("MarkerMode"), false);
        UI.TopBar.Q("MarkerMode").Q<Label>("Label").text = "Mark <u>T</u>iles";
    }


    protected override void BindCallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToNeutral);
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>(ChangeDragMode);
        Dragger.LeftClickRelease += ToggleBlock;
        Dragger.RightClickRelease += ToggleBlockMenu;
    }

    protected override void UnbindCallbacks()
    {
        UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToNeutral);
        UI.TopBar.Q("DragMode").UnregisterCallback<ClickEvent>(ChangeDragMode);
        Dragger.LeftClickRelease -= ToggleBlock;
        Dragger.RightClickRelease -= ToggleBlockMenu;
    }

    private void ToggleBlock()
    {
        Pointer.PickBlock()?.Select();
    }

    private void ToggleBlockMenu()
    {
        Pointer.PickBlock()?.ToggleMenu();
    }

    protected override void HandleKeypresses()
    {
        base.HandleKeypresses();
        if (DisallowShortcutKeys())
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.M))
        {
            GoToEditing(new ClickEvent());
            return;
        }

        if (Input.GetKeyUp(KeyCode.T))
        {
            GoToNeutral(new ClickEvent());
            return;
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            ChangeDragMode(new ClickEvent());
            return;
        }

    }

    protected override void GoToNeutral(ClickEvent evt)
    {
        base.GoToNeutral(evt);
        Block.DeselectAll();
        BlockRendering.ToggleAllBorders(false);
        Player.Self().ClearOp();
    }

    protected override void GoToEditing(ClickEvent evt)
    {
        base.GoToEditing(evt);
        Block.DeselectAll();
    }


}