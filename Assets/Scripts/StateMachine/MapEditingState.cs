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

    public override void UpdateState()
    {
        base.UpdateState();
        TerrainController.Organize();
        Pointer.PointAtBlocks();
    }

    protected void EnableInterface()
    {
        UI.ToggleDisplay("DiceRoller", false);
        UI.ToggleDisplay(UI.System.Q("DetailsHud"), false);

        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), false);

        UI.ToggleDisplay("ToolsPanel", true);
    }

    protected void DisableInterface()
    {
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleDisplay("ToolOptions", false);

        UI.ToggleDisplay(UI.System.Q("DetailsHud"), Preferences.Current.ShowHUD);

        UI.ToggleDisplay("BottomBar", true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), true);
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
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            AltMode = true;
            if (MapEdit.EditOp == "StyleBlock")
            {
                CustomCursor.SetSample();
            }
            else if (MapEdit.EditOp == "AddBlock")
            {
                CustomCursor.SetRemoveHeight();
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            AltMode = false;
            CustomCursor.SetDefault();
        }

    }

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

    public static void ResetMap()
    {
        Modal2.CreateContext("PrimaryDialog");
        Modal2.AddDialogHeader("Map Size");
        Modal2.AddInlineNumberNudgerField("NewMapSizeX", "Map Width", 8, 1, 50);
        Modal2.AddInlineNumberNudgerField("NewMapSizeY", "Map Length", 8, 1, 50);
        Modal2.AddInlineNumberNudgerField("NewMapSizeZ", "Map Height", 1, 1, 10);
        Modal2.AddDialogFooter();
        Modal2.AddFooterConfirm("Create", () =>
        {
            Modal2.ReadContext("PrimaryDialog");
            int x = Modal2.GetNumberNudgerFieldValue("NewMapSizeX");
            int y = Modal2.GetNumberNudgerFieldValue("NewMapSizeY");
            int z = Modal2.GetNumberNudgerFieldValue("NewMapSizeZ");
            TerrainController.ResetTerrain(x, y, z);
            MapMeta.Reset();
            Toast.AddSimple("Map reset.");
            Modal2.Close("PrimaryDialog");
        });
        Modal2.Open("Reset Map");
    }
}

