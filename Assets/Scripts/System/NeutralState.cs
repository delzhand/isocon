using System.Collections.Generic;
using System.Drawing;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NeutralState : TabletopSubstate
{
    private Token _dragToken;
    private bool _bottomBarVisible = false;

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
    }

    protected override void EnableInterface()
    {
        base.EnableInterface();
        UI.ToggleDisplay(UI.System.Q("BottomBar"), _bottomBarVisible);
        UI.ToggleDisplay(UI.System.Q("BottomRight"), true);
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

        if (Input.GetKeyUp(KeyCode.S))
        {
            GoToSession(new ClickEvent());
        }

        if (Input.GetKeyUp(KeyCode.V))
        {
            FixView(new ClickEvent());
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
        UI.TopBar.Q("Session").RegisterCallback<ClickEvent>(GoToSession);
        UI.TopBar.Q("FixedView").RegisterCallback<ClickEvent>(FixView);
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.System.Q("BottomRight").Q("AddToken").RegisterCallback<ClickEvent>(ShowAddTokenModal);
        UI.System.Q("BottomRight").Q("DeployToken").RegisterCallback<ClickEvent>(ToggleBottomBar);
        UI.System.Q<Button>("TurnAdvance").RegisterCallback<ClickEvent>(AdvanceRound);
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
        UI.TopBar.Q("Session").UnregisterCallback<ClickEvent>(GoToSession);
        UI.TopBar.Q("Dice").UnregisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.System.Q("BottomRight").Q("AddToken").UnregisterCallback<ClickEvent>(ShowAddTokenModal);
        UI.System.Q("BottomRight").Q("DeployToken").UnregisterCallback<ClickEvent>(ToggleBottomBar);
        UI.System.Q<Button>("TurnAdvance").UnregisterCallback<ClickEvent>(AdvanceRound);
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

    private void ToggleBottomBar(ClickEvent evt)
    {
        _bottomBarVisible = !_bottomBarVisible;
        UI.ToggleDisplay(UI.System.Q("BottomBar"), _bottomBarVisible);
    }

    private void ShowConsole(ClickEvent evt)
    {
        IsoConsole.OpenModal(evt);
    }

    private void AdvanceRound(ClickEvent evt)
    {
        Modal.DoubleConfirm("Advance Turn", GameSystem.Current().TurnAdvanceMessage(), () =>
        {
            Player.Self().CmdRequestGameDataSetValue("IncrementTurn");
        });
    }

    #endregion
}