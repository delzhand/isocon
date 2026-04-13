using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEditingState : BaseState
{
    public static List<Column> MarkedColumns;
    public static bool ClickAvailable = true;
    public static bool AltMode;

    private static State _revertState;

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
        MainMenu.SetupForMapEdit();

        _revertState = State.GetStateFromScene();
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        UnbindCallbacks();
    }

    public override void OnExit()
    {
        base.OnExit();
        Player.Self().ClearOp();
        BlockRendering.ToggleAllBorders(false);
        BlockRendering.ToggleSpacers(false);
        DisableInterface();
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    // public static void PermanentCallbacks()
    // {
    //     UI.TopBar.Q("SyncMap").RegisterCallback<ClickEvent>(Sync);
    //     UI.TopBar.Q("CancelEditMap").RegisterCallback<ClickEvent>(Cancel);
    //     UI.TopBar.Q("LoadMap").RegisterCallback<ClickEvent>(LoadMap);
    //     UI.TopBar.Q("SaveMap").RegisterCallback<ClickEvent>(SaveMap);
    //     UI.TopBar.Q("NewMap").RegisterCallback<ClickEvent>(MapEdit.NewMap);
    // }

    public override void UpdateState()
    {
        base.UpdateState();
        TerrainController.Organize();
        Pointer.PointAtBlocks();
    }

    protected void EnableInterface()
    {


        // UI.ToggleDisplay(UI.TopBar.Q("Dice"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("EditMap"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("AddTableTag"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("Config"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("Isocon"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("SessionWrapper"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("AddActor"), false);
        // UI.ToggleDisplay("BottomRight", false);
        UI.ToggleDisplay("DiceRoller", false);
        UI.ToggleDisplay(UI.System.Q("DetailsHud"), false);

        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), false);

        UI.ToggleDisplay("ToolsPanel", true);

        // UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), true);
        // UI.ToggleDisplay(UI.TopBar, true);
        // UI.ToggleDisplay(UI.TopBar.Q("EditingActions"), true);
        // UI.TopBar.Q("EditMap").Q<Label>("Label").text = "Sync <u>M</u>ap";
    }

    protected void DisableInterface()
    {
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleDisplay("ToolOptions", false);

        UI.ToggleDisplay(UI.System.Q("DetailsHud"), Preferences.Current.ShowHUD);

        UI.ToggleDisplay("BottomBar", true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), true);
        // UI.ToggleDisplay(UI.TopBar.Q("EditingActions"), false);
        // UI.ToggleDisplay(UI.TopBar.Q("Isocon"), true);
        // UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), false);
        // UI.TopBar.Q("EditMap").Q<Label>("Label").text = "Edit <u>M</u>ap";
    }

    protected void BindCallbacks()
    {
        Dragger.LeftClickStart += LeftClickStart;
        Dragger.LeftDragUpdate += LeftDragUpdate;

        Dragger.RightDragStart += Viewport.InitializeRightDrag;
        Dragger.RightDragUpdate += Viewport.UpdateRightDrag;
        Dragger.RightDragRelease += Viewport.EndRightDrag;

        Dragger.MiddleDragStart += Viewport.InitializeMiddleDrag;
        Dragger.MiddleDragUpdate += Viewport.UpdateMiddleDrag;
        Dragger.MiddleDragRelease += Viewport.EndMiddleDrag;

    }

    protected void UnbindCallbacks()
    {
        Dragger.LeftClickStart -= LeftClickStart;
        Dragger.LeftDragUpdate -= LeftDragUpdate;

        Dragger.RightDragStart -= Viewport.InitializeRightDrag;
        Dragger.RightDragUpdate -= Viewport.UpdateRightDrag;
        Dragger.RightDragRelease -= Viewport.EndRightDrag;

        Dragger.MiddleDragStart -= Viewport.InitializeMiddleDrag;
        Dragger.MiddleDragUpdate -= Viewport.UpdateMiddleDrag;
        Dragger.MiddleDragRelease -= Viewport.EndMiddleDrag;
    }

    private void LeftClickStart()
    {
        MarkedColumns = new();
        ClickAvailable = true;
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

        // if (Input.GetKeyUp(KeyCode.M))
        // {
        //     ExitEditing();
        //     return;
        // }

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

    // private void ExitEditing()
    // {
    //     if (TerrainController.MapDirty)
    //     {
    //         Modal2.Confirm("PrimaryDialog", "You have unsaved changes. Discard?", () => StateManager.PopState());
    //     }
    //     else
    //     {
    //         StateManager.PopState();
    //     }
    // }

    public static void Sync()
    {
        State state = State.GetStateFromScene();
        string json = JsonUtility.ToJson(state);
        Actor.MoveAllActorsToOptimalBlock();
        Player.Self().CmdMapSync(Compression.CompressString(json));
        StateManager.PopState();
    }

    public static void Cancel()
    {
        State.SetSceneFromState(_revertState);
        StateManager.PopState();
    }

    public static void SaveMap()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Save Map");
        Modal2.AddInlineTextField("MapName", "Map Name", MapMeta.Title);
        Modal2.AddInlineTextField("CreatorName", "Creator Name", MapMeta.CreatorName ?? Player.Self().Name);
        Modal2.AddInlineTextAreaField("Description", "Description", MapMeta.Description);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Save", () =>
        {
            FileBrowserHelper.Open(MapEdit.WriteFile, "", FileBrowserType.Maps, true);
            Modal2.Close();
        });

        Modal2.Open("Save Map");
    }

    public static void LoadMap()
    {
        if (TerrainController.MapDirty)
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Discard Changes?");
            Modal2.AddLongMarkup("You have unsaved changes. These change will be lost if a map is loaded.");
            Modal2.AddDialogFooter();
            Modal2.AddFooterConfirm("Discard and Continue", () =>
            {
                FileBrowserHelper.Open(MapEdit.OpenFile, "", FileBrowserType.Maps);
                Modal2.Close();
            });
            Modal2.Open("Load Map");
        }
        else
        {
            FileBrowserHelper.Open(MapEdit.OpenFile, "", FileBrowserType.Maps);
        }
    }

    public static void OpenMMMImportModal()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Import Map by URL");

        Modal2.AddLongMarkup("Maps from https://alessandrominali.github.io/maleghast/map.html can be imported by entering the Permalink. Maps that use custom brushes are not supported.");
        Modal2.AddInlineTextField("UrlField", "URL", "");
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Confirm", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            string url = Modal2.GetTextFieldValue("UrlField");
            if (!url.Contains("https://alessandrominali.github.io/maleghast/map"))
            {
                Toast.AddError("Does not appear to be a valid URL.");
            }
            else
            {
                MMMImporter.CreateFromURL(url);
                Modal2.Close();
            }
        });
        Modal2.Open("MMM Import");
    }
}

