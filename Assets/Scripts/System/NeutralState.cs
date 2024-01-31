using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NeutralState : TabletopSubstate
{
    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        Pointer.Point();
        SelectionMenu.Update();
        TileShare.Offsets();
        ShowTokenPanels();
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
            GoToMarking(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            ChangeDragMode(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            GoToConfig(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.A))
        {
            ShowAddTokenModal(new ClickEvent());
        }
    }

    private void ShowTokenPanels()
    {
        Token selected = Token.GetSelected();
        GameSystem.Current().UpdateTokenPanel(selected != null ? selected.Data.Id : null, "SelectedTokenPanel");

        Token focused = Token.GetFocused();
        GameSystem.Current().UpdateTokenPanel(focused != null ? focused.Data.Id : null, "FocusedTokenPanel");
    }

    #region Callbacks
    protected override void BindCallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>(ChangeDragMode);
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(GoToConfig);
        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<ClickEvent>(ShowAddTokenModal);
    }

    protected override void UnbindCallbacks()
    {
        UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("DragMode").UnregisterCallback<ClickEvent>(ChangeDragMode);
        UI.TopBar.Q("Config").UnregisterCallback<ClickEvent>(GoToConfig);
        UI.System.Q("BottomBar").Q("AddToken").UnregisterCallback<ClickEvent>(ShowAddTokenModal);
    }

    private void ShowAddTokenModal(ClickEvent evt)
    {
        AddToken.OpenModal(new ClickEvent());
        ModalState.Activate();
    }

    #endregion
}