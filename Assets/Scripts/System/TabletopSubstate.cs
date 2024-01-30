using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class TabletopSubstate : BaseState
{
    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        EnableInterface();
        BindCallbacks();
    }

    public override void OnExit()
    {
        base.OnExit();
        DisableInterface();
        UnbindCallbacks();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        HandleKeypresses();
        Viewport.HandleInput();
    }

    #region Interface
    protected virtual void EnableInterface()
    {
        UI.ToggleDisplay("BottomBar", true);
        UI.ToggleDisplay("TopBar", true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), true);

        UI.ToggleDisplay(UI.TopBar.Q("EditMap"), true);
        UI.ToggleDisplay(UI.TopBar.Q("DragMode"), true);
        UI.ToggleDisplay(UI.TopBar.Q("MarkerMode"), true);
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), true);
        UI.ToggleDisplay(UI.TopBar.Q("Info"), true);
        UI.ToggleDisplay(UI.TopBar.Q("Sync"), true);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), true);
    }

    protected virtual void DisableInterface()
    {
    }
    #endregion

    protected virtual void HandleKeypresses()
    {
        if (Input.GetKeyUp(KeyCode.C))
        {
            ChangeDragMode(new ClickEvent());
            return;
        }

    }

    #region Callbacks
    protected virtual void BindCallbacks()
    {
    }

    protected virtual void UnbindCallbacks()
    {
    }

    protected void GoToEditing(ClickEvent evt)
    {
        SM.ChangeSubState(new MapEditingState());
    }

    protected void GoToMarking(ClickEvent evt)
    {
        SM.ChangeSubState(new MapMarkingState());
    }

    protected void GoToNeutral(ClickEvent evt)
    {
        SM.ChangeSubState(new NeutralState());
    }

    protected void ChangeDragMode(ClickEvent evt)
    {
        Viewport.TogglePanMode();
    }

    #endregion
}