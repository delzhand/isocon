using System.Collections.Generic;
using System.Drawing;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NeutralState : TabletopSubstate
{
    private bool _showSync = false;
    private bool _showInfo = false;
    private Token _dragToken;

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        ShowTokenPanels();
        SelectionMenu.Update();
        TileShare.Offsets();
        Pointer.Point();
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

        if (Input.GetKeyUp(KeyCode.X))

        {
            ShowConsole(new ClickEvent());
        }
    }

    private void ShowTokenPanels()
    {
        Token selected = Token.GetSelected();
        GameSystem.Current()?.UpdateTokenPanel(selected != null ? selected.Data.Id : null, "SelectedTokenPanel");

        Token focused = Token.GetFocused();
        GameSystem.Current()?.UpdateTokenPanel(focused != null ? focused.Data.Id : null, "FocusedTokenPanel");
    }

    #region Callbacks
    protected override void BindCallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("DragMode").RegisterCallback<ClickEvent>(ChangeDragMode);
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(GoToConfig);
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(Config.OpenModal);
        UI.TopBar.Q("Info").RegisterCallback<ClickEvent>(ToggleInfo);
        UI.TopBar.Q("Sync").RegisterCallback<ClickEvent>(ToggleSync);
        UI.System.Q("BottomBar").Q("AddToken").RegisterCallback<ClickEvent>(ShowAddTokenModal);
        Dragger.LeftClickRelease += LeftClickRelease;
        Dragger.RightClickRelease += RightClickRelease;
        Dragger.LeftDragStart += LeftDragStart;
        Dragger.LeftDragRelease += LeftDragRelease;

    }

    protected override void UnbindCallbacks()
    {
        UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("DragMode").UnregisterCallback<ClickEvent>(ChangeDragMode);
        UI.TopBar.Q("Config").UnregisterCallback<ClickEvent>(GoToConfig);
        UI.TopBar.Q("Dice").UnregisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.TopBar.Q("Config").UnregisterCallback<ClickEvent>(Config.OpenModal);
        UI.TopBar.Q("Info").UnregisterCallback<ClickEvent>(ToggleInfo);
        UI.TopBar.Q("Sync").UnregisterCallback<ClickEvent>(ToggleSync);
        UI.System.Q("BottomBar").Q("AddToken").UnregisterCallback<ClickEvent>(ShowAddTokenModal);
        Dragger.LeftClickRelease -= LeftClickRelease;
        Dragger.RightClickRelease -= RightClickRelease;
        Dragger.LeftDragStart -= LeftDragStart;
        Dragger.LeftDragRelease -= LeftDragRelease;

    }

    private void LeftClickRelease()
    {
        Pointer.PickToken()?.ToggleInspect();
    }

    private void RightClickRelease()
    {
        Pointer.PickToken()?.ToggleMenu();
    }

    private void LeftDragStart()
    {
        _dragToken = Pointer.PickToken();
        _dragToken?.StartDragging();
    }

    private void LeftDragRelease()
    {
        _dragToken?.StopDragging(Pointer.PickBlock());
    }

    private void ShowAddTokenModal(ClickEvent evt)
    {
        AddToken.OpenModal(new ClickEvent());
    }

    private void ShowConsole(ClickEvent evt)
    {
        IsoConsole.OpenModal(evt);
    }

    public void ToggleInfo(ClickEvent evt)
    {
        _showInfo = !_showInfo;
        UI.ToggleDisplay("InfoWindow", _showInfo);
        UI.ToggleActiveClass(UI.TopBar.Q("Info"), _showInfo);

    }

    public void ToggleSync(ClickEvent evt)
    {
        _showSync = !_showSync;
        UI.ToggleDisplay("SyncPanel", _showSync);
        UI.ToggleActiveClass(UI.TopBar.Q("Sync"), _showSync);
    }

    #endregion
}