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
        UI.ToggleDisplay(UI.System.Q("TopRight"), true);

        UI.ToggleDisplay(UI.TopBar.Q("EditMap"), true);
        UI.ToggleDisplay(UI.TopBar.Q("DragMode"), true);
        UI.ToggleDisplay(UI.TopBar.Q("MarkerMode"), true);
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), true);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), true);
    }

    protected virtual void DisableInterface()
    {
    }
    #endregion

    protected virtual void HandleKeypresses()
    {
        Modal.HandleKeypresses();
    }

    protected bool DisallowShortcutKeys()
    {
        return UI.System.panel.focusController.focusedElement is TextField;
    }

    #region Callbacks
    protected virtual void BindCallbacks()
    {
    }

    protected virtual void UnbindCallbacks()
    {
    }

    protected virtual void GoToEditing(ClickEvent evt)
    {
        SM.ChangeSubState(new MapEditingState());
    }

    protected virtual void GoToMarking(ClickEvent evt)
    {
        SM.ChangeSubState(new TileMarkingState());
    }

    protected virtual void GoToNeutral(ClickEvent evt)
    {
        SM.ChangeSubState(new NeutralState());
    }

    protected virtual void GoToConfig(ClickEvent evt)
    {
        Config.OpenModal(evt);
    }

    protected void ChangeDragMode(ClickEvent evt)
    {
        Viewport.TogglePanMode();
    }

    protected void FixView(ClickEvent evt)
    {
        Quaternion q = Quaternion.Euler(new Vector3(0, 0, 20));
        GameObject.Find("CameraOrigin").transform.rotation = q;
    }

    #endregion
}