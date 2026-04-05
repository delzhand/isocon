using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEditingState : BaseState
{
    public static List<Column> MarkedColumns;
    public static bool AltMode;

    public override void OnEnter()
    {
        base.OnEnter();
        Block.DeselectAll();
        Actor.Deselect();
        Actor.UnfocusAll();
        BlockRendering.ToggleSpacers(true);
        BlockRendering.ToggleAllBorders(true);
        Player.Self().SetOp("Editing Map");
        Tutorial.Init("edit mode");
        EnableInterface();
        BindCallbacks();
    }

    public override void OnExit()
    {
        base.OnExit();
        BlockRendering.ToggleSpacers(false);
        MapSync();
        DisableInterface();
        UnbindCallbacks();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        TerrainController.Organize();
        Pointer.PointAtBlocks();
    }


    protected void EnableInterface()
    {
        Debug.Log("enable interface");
        UI.ToggleDisplay(UI.TopBar, true);
        UI.ToggleDisplay(UI.TopBar.Q("Dice"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Config"), false);
        UI.ToggleDisplay(UI.TopBar.Q("Isocon"), false);
        UI.ToggleDisplay(UI.TopBar.Q("SessionWrapper"), false);
        UI.ToggleDisplay(UI.TopBar.Q("AddActor"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("MarkerMode"), false);
        UI.ToggleDisplay("ToolsPanel", true);
        UI.ToggleDisplay("DiceRoller", false);
        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay("BottomRight", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), false);
        UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), true);
        UI.TopBar.Q("EditMap").Q<Label>("Label").text = "Sync <u>M</u>ap";
        UI.ToggleDisplay(UI.TopBar.Q("EditingActions"), true);
    }

    protected void DisableInterface()
    {
        Debug.Log("disable interface");
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleDisplay("ToolOptions", false);
        UI.ToggleDisplay(UI.TopBar.Q("Isocon"), true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), true);
        UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), false);
        UI.TopBar.Q("EditMap").Q<Label>("Label").text = "Edit <u>M</u>ap";
    }

    private void MapSync()
    {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        Actor.MoveAllActorsToOptimalBlock();
        Player.Self().CmdMapSync(Compression.CompressString(json));
    }

    protected void BindCallbacks()
    {
        // UI.TopBar.Q("EditMap").RegisterCallback<ClickEvent>(GoToNeutral);
        // UI.TopBar.Q("CancelEditMap").RegisterCallback<ClickEvent>(Cancel);
        // UI.TopBar.Q("LoadMap").RegisterCallback<ClickEvent>(LoadMap);
        // UI.TopBar.Q("SaveMap").RegisterCallback<ClickEvent>(SaveMap);
        Dragger.LeftClickStart += LeftClickStart;
        Dragger.LeftDragUpdate += LeftDragUpdate;
    }

    protected void UnbindCallbacks()
    {
        // UI.TopBar.Q("EditMap").UnregisterCallback<ClickEvent>(GoToNeutral);
        // UI.TopBar.Q("CancelEditMap").UnregisterCallback<ClickEvent>(Cancel);
        // UI.TopBar.Q("LoadMap").UnregisterCallback<ClickEvent>(LoadMap);
        // UI.TopBar.Q("SaveMap").UnregisterCallback<ClickEvent>(SaveMap);
        Dragger.LeftClickStart -= LeftClickStart;
        Dragger.LeftDragUpdate -= LeftDragUpdate;
    }

    private void LeftClickStart()
    {
        MarkedColumns = new();
        LeftDragUpdate();
    }

    private void LeftDragUpdate()
    {
        var block = Pointer.PickBlock();
        if (block)
        {
            TerrainController.Edit(block);
        }
    }

    public override void HandleInput()
    {
        // if (DisallowShortcutKeys())
        // {
        //     return;
        // }

        if (Input.GetKeyUp(KeyCode.M))
        {
            ExitEditing();
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt) && MapEdit.EditOp == "StyleBlock")
        {
            AltMode = true;
            CustomCursor.SetSample();
        }
        else if (Input.GetKeyDown(KeyCode.RightAlt) && MapEdit.EditOp == "StyleBlock")
        {
            AltMode = true;
            CustomCursor.SetSample();
        }
        else if ((Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt)) && MapEdit.EditOp == "StyleBlock")
        {
            AltMode = false;
            CustomCursor.SetDefault();
        }

    }

    private void ExitEditing()
    {
        if (TerrainController.MapDirty)
        {
            Modal2.Confirm("PrimaryDialog", "You have unsaved changes. Discard?", () => StateManager.PopState());
        }
        else
        {
            StateManager.PopState();
        }
    }


    // private void Cancel(ClickEvent evt)
    // {
    //     State.SetSceneFromState(revertState);
    //     GoToNeutral(evt);
    // }

    // private void SaveMap(ClickEvent evt)
    // {
    //     SM.ChangeSubState(new ModalState());
    //     Modal2.SetCurrentDialog("PrimaryDialog");
    //     Modal2.SetCloseAction(BackToEditing);
    //     Modal2.AddDialogHeader("Save Map");
    //     Modal2.AddInlineTextField("MapName", "Map Name", MapMeta.Title);
    //     Modal2.AddInlineTextField("CreatorName", "Creator Name", MapMeta.CreatorName ?? Player.Self().Name);
    //     Modal2.AddInlineTextAreaField("Description", "Description", MapMeta.Description);
    //     Modal2.AddDialogFooter();
    //     Modal2.AddFooterConfirm("Save", () =>
    //     {
    //         FileBrowserHelper.Open(MapEdit.WriteFile, "", FileBrowserType.Maps, true);
    //         Modal2.Close();
    //     });

    //     Modal2.Open();
    // }

    // private void LoadMap(ClickEvent evt)
    // {
    //     if (TerrainController.MapDirty)
    //     {
    //         SM.ChangeSubState(new ModalState());

    //         Modal2.SetCurrentDialog("PrimaryDialog");
    //         Modal2.SetCloseAction(BackToEditing);
    //         Modal2.AddDialogHeader("Discard Changes?");
    //         Modal2.AddLongMarkup("You have unsaved changes. These change will be lost if a map is loaded.");
    //         Modal2.AddDialogFooter();
    //         Modal2.AddFooterConfirm("Discard and Continue", () =>
    //         {
    //             FileBrowserHelper.Open(MapEdit.OpenFile, "", FileBrowserType.Maps);
    //             Modal2.Close();
    //         });
    //         Modal2.Open();
    //     }
    //     else
    //     {
    //         FileBrowserHelper.Open(MapEdit.OpenFile, "", FileBrowserType.Maps);
    //     }
    // }

}

