using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Mirror;
using ShunUI;
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
        Autosaver.Tick();
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
            AddActorModal.OpenModal(new ClickEvent());
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
            GoToAddToken(new ClickEvent());
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
        Actor selected = Actor.GetSelected();
        Actor focused = Actor.GetFocused();

        if (focused && selected)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", true);
            if (Actor.RebuildPanels)
            {
                selected.Data.GetActorType().InitPanel(selected.Data, "LeftTokenPanel", true);
                focused.Data.GetActorType().InitPanel(focused.Data, "RightTokenPanel");
                Actor.RebuildPanels = false;
            }
            selected.Data.UpdateActorPanel("LeftTokenPanel");
            focused.Data.UpdateActorPanel("RightTokenPanel");
        }
        else if (focused && !selected)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", false);
            if (Actor.RebuildPanels)
            {
                focused.Data.GetActorType().InitPanel(focused.Data, "LeftTokenPanel");
                Actor.RebuildPanels = false;
            }
            focused.Data.UpdateActorPanel("LeftTokenPanel");
        }
        else if (selected && !focused)
        {
            UI.ToggleActiveClass("LeftTokenPanel", true);
            UI.ToggleActiveClass("RightTokenPanel", false);
            if (Actor.RebuildPanels)
            {
                selected.Data.GetActorType().InitPanel(selected.Data, "LeftTokenPanel", true);
                Actor.RebuildPanels = false;
            }
            selected.Data.UpdateActorPanel("LeftTokenPanel");
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
        UI.TopBar.Q("AddActor").RegisterCallback<ClickEvent>(GoToAddToken);
        UI.TopBar.Q("Config").RegisterCallback<ClickEvent>(GoToConfig);
        UI.TopBar.Q("Session").RegisterCallback<ClickEvent>(GoToSession);
        UI.TopBar.Q("FixedView").RegisterCallback<ClickEvent>(FixView);
        UI.TopBar.Q("Dice").RegisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.System.Q("TopBarToggle").RegisterCallback<ClickEvent>(ToggleTopBar);
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
        UI.TopBar.Q("AddActor").UnregisterCallback<ClickEvent>(GoToAddToken);
        UI.TopBar.Q("Config").UnregisterCallback<ClickEvent>(GoToConfig);
        UI.TopBar.Q("Session").UnregisterCallback<ClickEvent>(GoToSession);
        UI.TopBar.Q("Dice").UnregisterCallback<ClickEvent>(DiceRoller.ToggleVisible);
        UI.System.Q("TopBarToggle").UnregisterCallback<ClickEvent>(ToggleTopBar);
        UI.System.Q("DeployToggle").UnregisterCallback<ClickEvent>(ToggleBottomBar);
        UI.System.Q("AddSystemTag").UnregisterCallback<ClickEvent>(ShowSystemTagModal);
        Dragger.LeftClickRelease -= LeftClickRelease;
        Dragger.RightClickRelease -= RightClickRelease;
        Dragger.LeftDragStart -= LeftDragStart;
        Dragger.LeftDragRelease -= LeftDragRelease;

    }

    private void LeftClickRelease()
    {
        Pointer.PickActor()?.ToggleSelect();
    }

    private void RightClickRelease()
    {
        Actor pickedActor = Pointer.PickActor(true);
        if (pickedActor)
        {
            pickedActor.ToggleMenu();
            return;
        }
        Block pickedBlock = Pointer.PickBlock();
        if (pickedBlock)
        {
            pickedBlock.ToggleMenu();
            return;
        }
    }

    private void LeftDragStart()
    {
        Actor t = Pointer.PickActor();
        t?.StartDragging();
    }

    private void LeftDragRelease()
    {
        Actor.StopDragging(Pointer.PickBlock(), Pointer.PickPoint());
    }

    private void ToggleTopBar(ClickEvent evt)
    {
        UI.ToggleActiveClass(UI.System.Q("TopBar"));
    }

    private void ToggleBottomBar(ClickEvent evt)
    {
        UI.ToggleActiveClass(UI.System.Q("BottomBar"));
        UI.ToggleDisplay(UI.System.Q("DeployToggle").Q("Attn"), false);
    }

    private void ShowConsole(ClickEvent evt)
    {
        ConsoleModal.OpenModal(evt);
    }

    private void ShowSystemTagModal(ClickEvent evt)
    {
        SelectionMenu.Hide();

        SM.ChangeSubState(new ModalState());

        var dialog = Modal2.SetCurrentDialog("ShunDialog1");
        Modal2.SetCloseAction(StateManager.ToNeutral);
        var contents = Modal2.Contents("ShunDialog1");
        contents.Clear();

        Modal2.AddDialogHeader("Add System Tag");

        var tagType = Modal2.AddInlineSelectField("Type", "Type", "Simple", StringUtility.CreateArray("Simple", "Number", "Clock").ToList<string>());
        tagType.Q<ShunSelect>().OnSelect += () =>
        {
            var container = contents.Q<ShunContainer>("TagTypeContainer");
            container.Clear();
            string type = Modal2.GetSelectFieldValue("ShunDialog1", "Type");
            if (type == "Number" || type == "Clock")
            {
                var initVal = Modal2.AddInlineIntField("InitialValue", "Initial Value", 0);
                Modal2.MoveToContainer(initVal, container);
            }
            if (type == "Clock")
            {
                var maxVal = Modal2.AddInlineIntField("MaxValue", "Max Value", 4);
                Modal2.MoveToContainer(maxVal, container);
            }
        };

        Modal2.AddInlineTextField("TagName", "Tag Name", "", "The text that will appear on the tag");
        Modal2.AddInlineComboboxField("Color", "Color", "Black", ColorUtility.CommonColors().ToList<string>());

        var typeContainer = new ShunContainer();
        typeContainer.name = "TagTypeContainer";
        typeContainer.AddToClassList("shun-dialog__field");
        contents.Add(typeContainer);

        var footer = Modal2.AddDialogFooter("Cancel", () =>
        {
            dialog.Close();
        });

        var confirm = new ShunButton();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Create";
        confirm.clicked += () =>
        {
            AddSystemTagSubmit();
        };
        footer.Add(confirm);

        dialog.Open();
    }

    private void AddSystemTagSubmit()
    {
        string tagName = Modal2.GetTextFieldValue("ShunDialog1", "TagName");
        int tagValue = Modal2.GetIntFieldValue("ShunDialog1", "InitialValue");
        int tagMaxValue = Modal2.GetIntFieldValue("ShunDialog1", "MaxValue");
        string colorValue = Modal2.GetComboboxFieldValue("ShunDialog1", "Color");
        string tagType = Modal2.GetSelectFieldValue("ShunDialog1", "Type");
        GameSystemTag tag = new();
        tag.Name = tagName;
        tag.Value = tagValue;
        tag.Type = tagType;
        tag.MaxValue = tagMaxValue;
        tag.Color = ColorUtility.GetCommonColor(colorValue);
        Player.Self().CmdRequestGameSystemCommand($"AddTag|{JsonUtility.ToJson(tag)}");
        Modal2.Dialog("ShunDialog1").Close();
    }

    #endregion
}