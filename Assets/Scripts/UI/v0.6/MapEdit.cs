using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEdit
{
    private static string CurrentFile = "";
    public static string EditOp = "AddBlock";
    public static string ShapeOp = "ShapeSolid";
    public static string ResizeOp = "ResizeCloneCol";
    public static string StyleOp = "StylePaint";

    public static void Setup()
    {
        UI.SetBlocking(UI.System, StringUtility.Arr("ToolsPanel", "ToolOptions"));

        VisualElement toolsRoot = UI.System.Q("ToolsPanel");
        VisualElement optionsRoot = UI.System.Q("ToolOptions");

        UI.ToggleDisplay(toolsRoot, false);
        UI.ToggleDisplay(optionsRoot, false);

        UI.System.Q("ClickCatcher").RegisterCallback<ClickEvent>(CloseSubtoolFlyouts);

        OptionsSetup();

        toolsRoot.Query(null, "tool").ForEach((tool) => {
            Button toolButton = tool as Button;
            // Clear handler that eats mousedown
            toolButton.clickable.activators.Clear();
            // Tooltip show
            toolButton.Q("Icon").RegisterCallback<MouseEnterEvent>((evt) => {
                UI.ToggleDisplay(toolButton.Q("Tooltip"), true);
            });
            // Tooltip hide
            toolButton.Q("Icon").RegisterCallback<MouseLeaveEvent>((evt) => {
                UI.ToggleDisplay(toolButton.Q("Tooltip"), false);
            });
            toolButton.RegisterCallback<MouseDownEvent>((evt) => {
                UI.ToggleDisplay("ClickCatcher", false);
                // set op
                EditOp = tool.name;
                // set active class on current
                toolsRoot.Query(null, "tool").ForEach((item) => {
                    UI.ToggleActiveClass(item, item.name == tool.name);
                });
                // close options
                UI.ToggleDisplay(optionsRoot, false);
                // close flyouts
                CloseSubtoolFlyouts(new ClickEvent());
                if (toolButton.ClassListContains("has-subtools")) {
                    Tutorial.Init("subtools");
                    if (Input.GetMouseButtonDown(1)) {
                        ShowSubtools(tool.name);
                    }
                }
                if (toolButton.ClassListContains("has-options")) {
                    OpenOptions(toolButton.name);
                }
                if (toolButton.ClassListContains("has-subtool-options")) {
                    OpenOptions(toolButton.name, true);
                }
            });       
        });

        toolsRoot.Query(null, "subtool").ForEach((tool) => {
            string s = tool.name;
            toolsRoot.Q<Button>(s).RegisterCallback<ClickEvent>((evt) => {
                if (s.StartsWith("Shape")) {
                    ShapeOp = s;
                    toolsRoot.Q("ChangeShape").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
                }
                else if (s.StartsWith("Style")) {
                    StyleOp = s;
                    toolsRoot.Q("StyleBlock").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
                }
                else if (s.StartsWith("Resize")) {
                    ResizeOp = s;
                    toolsRoot.Q("ResizeMap").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
                }
                ActivateSubtool();
                CloseSubtoolFlyouts(new ClickEvent());
                if (tool.ClassListContains("has-options")) {
                    OpenOptions(tool.name);
                }
            });
        });
    }

    public static void ShowSubtools(string claim) {
        UI.ToggleDisplay(UI.System.Q("ToolsPanel").Q(claim).Q("Tooltip"), false);
        OpenSubtoolFlyout(UI.System.Q("ToolsPanel").Q(claim).Q("Options"));
    }

    public static void OpenOptions(string s, bool sub = false) {
        VisualElement optionsRoot = UI.System.Q("ToolOptions");
        UI.ToggleDisplay(optionsRoot, true);
        if (sub && s == "StyleBlock") {
            s = StyleOp;
        }
        optionsRoot.Query(null, "tool-options").ForEach((item) => {
            UI.ToggleDisplay(item, item.name.StartsWith(s));
        });
    }

    public static void OptionsSetup() {

        VisualElement optionsRoot = UI.System.Q("ToolOptions");

        // Data
        optionsRoot.Q("DataOptions").Q("Save").RegisterCallback<ClickEvent>((evt) => {
            OpenSaveModal(new ClickEvent());
        });
        optionsRoot.Q("DataOptions").Q("Open").RegisterCallback<ClickEvent>((evt) => {
            OpenOpenModal(new ClickEvent());
        });
        optionsRoot.Q("DataOptions").Q("Reset").RegisterCallback<ClickEvent>((evt) => {
            ResetConfirm(new ClickEvent());
        });
        optionsRoot.Q("DataOptions").Q<TextField>("MapTitle").value = MapMeta.Title;
        optionsRoot.Q("DataOptions").Q<TextField>("MapTitle").RegisterValueChangedCallback((evt) => {
            MapMeta.Title = evt.newValue;
        });
        optionsRoot.Q("DataOptions").Q<TextField>("Description").value = MapMeta.Description;
        optionsRoot.Q("DataOptions").Q<TextField>("Description").RegisterValueChangedCallback((evt) => {
            MapMeta.Description = evt.newValue;
        });
        optionsRoot.Q("DataOptions").Q<TextField>("Objective").value = MapMeta.Objective;
        optionsRoot.Q("DataOptions").Q<TextField>("Objective").RegisterValueChangedCallback((evt) => {
            MapMeta.Objective = evt.newValue;
        });
        optionsRoot.Q("DataOptions").Q<TextField>("Author").value = MapMeta.CreatorName;
        optionsRoot.Q("DataOptions").Q<TextField>("Author").RegisterValueChangedCallback((evt) => {
            MapMeta.CreatorName = evt.newValue;
        });
        optionsRoot.Q("DataOptions").Q<DropdownField>("System").value = MapMeta.System;
        optionsRoot.Q("DataOptions").Q<DropdownField>("System").RegisterValueChangedCallback((evt) => {
            MapMeta.System = evt.newValue;
        });

        // Environment
        optionsRoot.Q("EnvironmentOptions").Q("LightAngle").RegisterCallback<ChangeEvent<float>>((evt) => {
            TerrainController.LightAngle = evt.newValue;
            TerrainController.UpdateLight();
        });
        optionsRoot.Q("EnvironmentOptions").Q("LightHeight").RegisterCallback<ChangeEvent<float>>((evt) => {
            TerrainController.LightHeight = evt.newValue;
            TerrainController.UpdateLight();
        });
        optionsRoot.Q("EnvironmentOptions").Q("LightIntensity").RegisterCallback<ChangeEvent<float>>((evt) => {
            TerrainController.LightIntensity = evt.newValue;
            TerrainController.UpdateLight();
        });
        optionsRoot.Q("EnvironmentOptions").Q("TopBgColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Top Background Color");
            Modal.AddColorField("TopBgColor", Environment.BgTopColor);
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        optionsRoot.Q("EnvironmentOptions").Q("BotBgColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Bottom Background Color");
            Modal.AddColorField("BotBgColor", Environment.BgBottomColor);
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        optionsRoot.Q("EnvironmentOptions").Q("TopBlockColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Default Block Top Color");
            Modal.AddColorField("TopBlockColor", Environment.TileTopColor);
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        optionsRoot.Q("EnvironmentOptions").Q("SideBlockColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Default Block Side Color");
            Modal.AddColorField("SideBlockColor", Environment.TileSideColor);
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });

        // Tile Paint
        optionsRoot.Q("StylePaintOptions").Q("TopBlockPaint").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Block Top Paint");
            Modal.AddColorField("TopBlockPaint", Environment.CurrentPaintTop);
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        optionsRoot.Q("StylePaintOptions").Q("SideBlockPaint").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Block Side Paint");
            Modal.AddColorField("SideBlockPaint", Environment.CurrentPaintSide);
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });

    }

    private static void OpenSubtoolFlyout(VisualElement v) {
        UI.ToggleDisplay("ClickCatcher", true);
        // Close existing subtool flyouts
        UI.System.Q("ToolsPanel").Query(null, "subtool-flyout").ForEach((item) => {
            UI.ToggleDisplay(item, false);
        });
        // Show passed-in flyout
        UI.ToggleDisplay(v, true);
        ActivateSubtool();
    }

    private static void CloseSubtoolFlyouts(ClickEvent evt) {
        UI.ToggleDisplay("ClickCatcher", false);
        UI.System.Q("ToolsPanel").Query(null, "subtool-flyout").ForEach((item) => {
            UI.ToggleDisplay(item, false);
        });
    }

    private static void ActivateSubtool() {
        UI.System.Q("ToolsPanel").Query(null, "subtool").ForEach((tool) => {
            UI.ToggleActiveClass(tool, StringUtility.InList(tool.name, ShapeOp, ResizeOp, StyleOp));
        });
    }

    public static void ToggleEditMode(ClickEvent evt) {
        // Disable terrain effect mode if necessary
        if (Cursor.Mode == CursorMode.TerrainEffecting) {
            TerrainController.ToggleTerrainEffectMode(evt);
        }

        if (Cursor.Mode != CursorMode.Editing) {
            StartEditing();
        }
        else {
            EndEditing();
        }
    }

    private static void StartEditing() {
        UI.ToggleDisplay("ToolsPanel", true);
        UI.ToggleDisplay("DiceRoller", false);
        Block.DeselectAll();
        Token.DeselectAll();
        Token.UnfocusAll();
        Block.ToggleSpacers(true);
        Cursor.Mode = CursorMode.Editing;
        BlockMesh.ToggleBorders(true);
        UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), true);
        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), false);
    }

    private static void EndEditing() {
        Tutorial.Init("end edit");
        Cursor.Mode = CursorMode.Default;
        BlockMesh.ToggleBorders(false);
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleDisplay("ToolOptions", false);
        Block.ToggleSpacers(false);
        State.SetCurrentJson();
        Player.Self().CmdMapSync();
        UI.ToggleActiveClass(UI.TopBar.Q("EditMap"), false);
        UI.ToggleDisplay("BottomBar", true);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), true);
    }

    private static void ResetConfirm(ClickEvent evt) {
        if (!TerrainController.MapDirty) {
            ResetMap();
        }
        else {
            Modal.DoubleConfirm("Confirm Reset", "You have unsaved changes. Discard?", ResetMap);
        }
    }

    private static void ResetMap() {
        CurrentFile = "";
        Modal.Reset("Map Size");
        Modal.AddIntField("NewMapSizeX", "Map Width", 8);
        Modal.AddIntField("NewMapSizeY", "Map Length", 8);
        Modal.AddIntField("NewMapSizeZ", "Map Height", 1);
        Modal.AddPreferredButton("Reset Map", (evt) => {
            int x = UI.Modal.Q<IntegerField>("NewMapSizeX").value;
            int y = UI.Modal.Q<IntegerField>("NewMapSizeY").value;
            int z = UI.Modal.Q<IntegerField>("NewMapSizeZ").value;
            TerrainController.ResetTerrain(x, y, z);
            MapMeta.Reset();
            Toast.AddSimple("Map reset.");
            Modal.Close();
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void OpenSaveModal(ClickEvent evt) {
        Modal.Reset("Save Map");

        Modal.AddTextField("Filename", "Filename", CurrentFile.Length > 0 ? CurrentFile : "");
        UI.Modal.Q("Filename").style.minWidth = 400;

        Modal.AddDropdownField("SaveType", "Save Type", "Plaintext JSON", StringUtility.Arr("Encoded Screenshot", "Plaintext JSON"));

        Modal.AddPreferredButton("Confirm", ConfirmMapSave);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void OpenOpenModal(ClickEvent evt) {
        Modal.Reset("Open Map");
        Modal.AddSearchField("SearchField", "Filename", "", GetAllMapFiles());
        Modal.AddPreferredButton("Confirm", ConfirmMapOpen);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ConfirmMapOpen(ClickEvent evt) {
        string value = UI.Modal.Q("SearchField").Q<TextField>("SearchInput").value;
        if (!TerrainController.MapDirty) {
            OpenFile();
            Modal.Close();
        }
        else {
            Modal.DoubleConfirm("Confirm Open", "You have unsaved changes. Discard?", OpenFile);
        }
    }

    private static void OpenFile() {
        string filename = UI.Modal.Q("SearchField").Q<TextField>("SearchInput").value;
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        CurrentFile = filename.Replace(".png", "").Replace(".json", "");
        if (fullPath.EndsWith(".png")) {
            MapSaver.StegLoad(fullPath);
        }
        else {
            MapSaver.LegacyLoad(fullPath);
        }
    }

    private static void ConfirmMapSave(ClickEvent evt) {
        string value = UI.Modal.Q<TextField>("Filename").value;
        if (value.Length == 0) {
            Toast.AddError("Not a valid filename.");
        }
        else {
            string saveType = UI.Modal.Q<DropdownField>("SaveType").value;
            if (saveType == "Encoded Screenshot") {
                if (value.EndsWith(".json")) {
                    value = value.Replace(".json", "");
                }
                if (!value.EndsWith(".png")) {
                    value += ".png";
                }
            }
            else {
                if (value.EndsWith(".png")) {
                    value = value.Replace(".png", "");
                }
                if (!value.EndsWith(".json")) {
                    value += ".json";
                }
            }
            UI.Modal.Q<TextField>("Filename").value = value;
            if (FileExists(value)) {
                Modal.DoubleConfirm("Confirm Overwrite", "A file with this name already exists. Overwrite?", WriteFile);
            }
            else {
                WriteFile();
                Modal.Close();
            }
        }
    }

    private static void WriteFile() {
        string saveType = UI.Modal.Q<DropdownField>("SaveType").value;
        string filename = UI.Modal.Q<TextField>("Filename").value;
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        if (saveType == "Encoded Screenshot") {
            MapSaver.StegSave(fullPath);
        }
        else {
            MapSaver.RegSave(fullPath);
        }
    }

    private static bool FileExists(string filename) {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        return File.Exists(fullPath);
    }

    private static void GetFilesRecursively(string basePath, string relativePath, List<string> fileList)
    {
        string[] files = Directory.GetFiles(basePath + relativePath);
        foreach (string file in files)
        {
            fileList.Add(relativePath + "/" + Path.GetFileName(file));
        }

        string[] directories = Directory.GetDirectories(basePath + relativePath);
        foreach (string directory in directories)
        {
            GetFilesRecursively(basePath, relativePath + "/" + Path.GetFileName(directory), fileList);
        }
    }

    public static string[] GetAllMapFiles()
    {
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        List<string> mapFiles = new List<string>();

        if (!Directory.Exists(path + "/maps"))
        {
            Directory.CreateDirectory(path + "/maps");
        }

        GetFilesRecursively(path, "/maps", mapFiles);

        // Remove "/maps" from each string in the list
        for (int i = 0; i < mapFiles.Count; i++)
        {
            mapFiles[i] = mapFiles[i].Replace("/maps/", "");
        }

        return mapFiles.ToArray();
    }

    public static string GetMarkerEffect() {
         return UI.System.Q("ToolsPanel").Q("EffectSearch").Q<TextField>("SearchInput").value;
    }

    public static void ColorChanged() {
        VisualElement root = UI.System.Q("ToolOptions");
        Color c = ColorField.FromSliders();
        root.Q(ColorField.CurrentName).style.backgroundColor = c;
        switch (ColorField.CurrentName) {
            case "TopBlockColor":
                Environment.TileTopColor = c;
                Block.SetColor("top1", c);
                Block.SetColor("top2", ColorUtility.DarkenColor(c, .2f));
                break;
            case "SideBlockColor":
                Environment.TileSideColor = c;
                Block.SetColor("side1", c);
                Block.SetColor("side2", ColorUtility.DarkenColor(c, .2f));
                break;
            case "TopBgColor":
                Environment.BgTopColor = c;
                MeshRenderer mra = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
                mra.material.SetColor("_Color1", c);
                break;
            case "BotBgColor":
                Environment.BgBottomColor = c;
                MeshRenderer mrb = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
                mrb.material.SetColor("_Color2", c);
                break;
            case "TopBlockPaint":
                Environment.CurrentPaintTop = c;
                break;
            case "SideBlockPaint":
                Environment. CurrentPaintSide = c;
                break;
        }
    }

}
