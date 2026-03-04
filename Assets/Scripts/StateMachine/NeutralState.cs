using System.Collections.Generic;
using System.Drawing;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NeutralState : TabletopSubstate
{
    // private bool _bottomBarVisible = true;

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
    }

    protected override void EnableInterface()
    {
        base.EnableInterface();
        UI.ToggleDisplay(UI.System.Q("BottomBar"), true);
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

        if (Input.GetKeyUp(KeyCode.A))
        {
            AddToken.OpenModal(new ClickEvent());
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
        Token focused = Token.GetFocused();

        if (focused && selected)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", true);
            if (Token.RebuildPanels)
            {
                selected.Data.GetSystemToken().InitPanel("LeftTokenPanel", true);
                focused.Data.GetSystemToken().InitPanel("RightTokenPanel");
                Token.RebuildPanels = false;
            }
            selected.Data.UpdateTokenPanel("LeftTokenPanel");
            focused.Data.UpdateTokenPanel("RightTokenPanel");
        }
        else if (focused && !selected)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", false);
            if (Token.RebuildPanels)
            {
                focused.Data.GetSystemToken().InitPanel("LeftTokenPanel");
                Token.RebuildPanels = false;
            }
            focused.Data.UpdateTokenPanel("LeftTokenPanel");
        }
        else if (selected && !focused)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", false);
            if (Token.RebuildPanels)
            {
                selected.Data.GetSystemToken().InitPanel("LeftTokenPanel", true);
                Token.RebuildPanels = false;
            }
            selected.Data.UpdateTokenPanel("LeftTokenPanel");
        }
        else
        {
            UI.ToggleActiveClass("LeftTokenPanel", false);
            UI.ToggleActiveClass("RightTokenPanel", false);
        }
    }

    #region Callbacks
    protected override void BindCallbacks()
    {
        UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").RegisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(GoToConfig);
        UI.TopBar.Q("Session").RegisterCallback<ClickEvent>(GoToSession);
        UI.TopBar.Q("FixedView").RegisterCallback<ClickEvent>(FixView);
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.TopBar.Q("AddToken").RegisterCallback<ClickEvent>(ShowAddTokenModal);
        UI.System.Q("DeployToggle").RegisterCallback<ClickEvent>(ToggleBottomBar);
        UI.System.Q("AddSystemTag").RegisterCallback<ClickEvent>(ShowSystemTagModal);
        Dragger.LeftClickRelease += LeftClickRelease;
        Dragger.RightClickRelease += RightClickRelease;
        Dragger.LeftDragStart += LeftDragStart;
        Dragger.LeftDragRelease += LeftDragRelease;

    }

    protected override void UnbindCallbacks()
    {
        UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToEditing);
        UI.TopBar.Q("MarkerMode").UnregisterCallback<ClickEvent>(GoToMarking);
        UI.TopBar.Q("Config").UnregisterCallback<ClickEvent>(GoToConfig);
        UI.TopBar.Q("Session").UnregisterCallback<ClickEvent>(GoToSession);
        UI.TopBar.Q("Dice").UnregisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.TopBar.Q("AddToken").UnregisterCallback<ClickEvent>(ShowAddTokenModal);
        UI.System.Q("DeployToggle").UnregisterCallback<ClickEvent>(ToggleBottomBar);
        UI.System.Q("AddSystemTag").UnregisterCallback<ClickEvent>(ShowSystemTagModal);
        Dragger.LeftClickRelease -= LeftClickRelease;
        Dragger.RightClickRelease -= RightClickRelease;
        Dragger.LeftDragStart -= LeftDragStart;
        Dragger.LeftDragRelease -= LeftDragRelease;

    }

    private void LeftClickRelease()
    {
        // Debug.Log("left click release");
        Pointer.PickToken()?.ToggleSelect();
    }

    private void RightClickRelease()
    {
        // Debug.Log("right click release");
        Pointer.PickToken()?.ToggleMenu();
    }

    private void LeftDragStart()
    {
        Token t = Pointer.PickToken();
        t?.StartDragging();
    }

    private void LeftDragRelease()
    {
        Token.StopDragging(Pointer.PickBlock());
    }

    private void ShowAddTokenModal(ClickEvent evt)
    {
        AddToken.OpenModal(new ClickEvent());
    }

    private void ToggleBottomBar(ClickEvent evt)
    {
        UI.ToggleActiveClass(UI.System.Q("BottomBar"));
        UI.ToggleDisplay(UI.System.Q("DeployToggle").Q("Attn"), false);
    }

    private void ShowConsole(ClickEvent evt)
    {
        IsoConsole.OpenModal(evt);
    }

    private void AdvanceRound(ClickEvent evt)
    {
        // Modal.DoubleConfirm("Advance Turn", GameSystem.Current().TurnAdvanceMessage(), () =>
        // {
        //     Player.Self().CmdRequestGameSystemCommand("IncrementTurn");
        // });
    }

    private void ShowSystemTagModal(ClickEvent evt)
    {
        Modal.Reset("Add Tag");
        Modal.AddTextField("TagName", "Tag Name", "");
        Modal.AddDropdownField("ColorField", "Color", "Gray", ColorUtility.CommonColors());
        Modal.AddDropdownField("TagType", "Type", "Simple", StringUtility.CreateArray("Simple", "Number", "Clock"), (evt) => AddSystemTagModalConditions());
        Modal.AddIntField("TagValue", "Tag Initial Value", 0);
        Modal.AddPreferredButton("Add", AddSystemTagSubmit);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        AddSystemTagModalConditions();

        SelectionMenu.Hide();
    }

    private void AddSystemTagSubmit(ClickEvent evt)
    {
        string tagName = UI.Modal.Q<TextField>("TagName").value;
        int tagValue = UI.Modal.Q<IntegerField>("TagValue").value;
        string colorValue = UI.Modal.Q<DropdownField>("ColorField").value;
        string tagType = UI.Modal.Q<DropdownField>("TagType").value;
        GameSystemTag tag = new();
        tag.Name = tagName;
        tag.Value = tagValue;
        tag.Type = tagType;
        tag.Color = ColorUtility.GetCommonColor(colorValue);
        Player.Self().CmdRequestGameSystemCommand($"AddTag|{JsonUtility.ToJson(tag)}");
        Modal.Close();
    }

    private void AddSystemTagModalConditions()
    {
        string tagType = UI.Modal.Q<DropdownField>("TagType").value;
        UI.ToggleDisplay(UI.Modal.Q("TagValue"), tagType != "Simple");
    }

    #endregion
}