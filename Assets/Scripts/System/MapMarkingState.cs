using UnityEngine;
using UnityEngine.UIElements;

public class MapMarkingState : TabletopSubstate
{
    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        Block.DeselectAll();
        Token.DeselectAll();
        Token.UnfocusAll();
        BlockMesh.ToggleAllBorders(true);
        Cursor.Mode = CursorMode.Marking;
        Player.Self().SetOp("Marking Tiles");
        Tutorial.Init("Marking Mode");
    }

    public override void OnExit()
    {
        base.OnExit();
        BlockMesh.ToggleAllBorders(false);
        Player.Self().ClearOp();
    }

    protected override void EnableInterface()
    {
        base.EnableInterface();
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Info"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Sync"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), false);
        UI.ToggleDisplay("DiceRoller", false);
        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleActiveClass(UI.TopBar.Q("MarkerMode"), true);
        UI.TopBar.Q("MarkerMode").Q<Label>("Label").text = "Stop Marking <u>T</u>iles";
    }

    protected override void DisableInterface()
    {
        base.DisableInterface();
        UI.ToggleActiveClass(UI.TopBar.Q("MarkerMode"), false);
        UI.TopBar.Q("MarkerMode").Q<Label>("Label").text = "Mark <u>T</u>iles";
    }


    protected override void BindCallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToNeutral);
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>(ChangeDragMode);
    }

    protected override void UnbindCallbacks()
    {
        UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToNeutral);
        UI.TopBar.Q("DragMode").UnregisterCallback<ClickEvent>(ChangeDragMode);
    }

    protected override void HandleKeypresses()
    {
        base.HandleKeypresses();
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
    }
}