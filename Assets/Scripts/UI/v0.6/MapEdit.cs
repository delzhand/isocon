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
    public static string EditOp;
    public static string ShapeOp = "Solid";
    public static string MultiOp = "CloneRow";
    public static string StyleOp = "Paint";

    // public static BlockType Shape;

    public static void Setup()
    {
        VisualElement root = UI.System.Q("ToolsPanel");
        UI.SetBlocking(UI.System, "ToolsPanel");
        UI.ToggleDisplay(root, false);
        root.Query<Button>(null, "tool-button").ForEach(RegisterButton);
        OptionsSetup();

        UI.System.Q("ClickCatcher").RegisterCallback<ClickEvent>(CloseOptionFlyout);
    }

    public static void OptionsSetup() {
        VisualElement root = UI.System.Q("ToolsPanel");

        // Shape
        foreach (string s in StringUtility.Arr("ShapeSlope", "ShapeSolid", "ShapeHidden")) {
            root.Q(s).RegisterCallback<ClickEvent>((evt) => {
                ShapeOp = s;
                root.Query(null, "tool-sub-button").ForEach((item) => {
                    UI.ToggleActiveClass(item, item.name == s);
                });
            });
        }

        // Multi
        foreach (string s in StringUtility.Arr("CloneRow", "CloneCol", "RemoveRow", "RemoveCol", "AddHeight")) {
            root.Q(s).RegisterCallback<ClickEvent>((evt) => {
                MultiOp = s;
                root.Query(null, "tool-sub-button").ForEach((item) => {
                    UI.ToggleActiveClass(item, item.name == s);
                });
            });
        }

        // Style
        foreach (string s in StringUtility.Arr("Paint", "Texture", "Eraser")) {
            root.Q(s).RegisterCallback<ClickEvent>((evt) => {
                StyleOp = s;
                root.Query(null, "tool-sub-button").ForEach((item) => {
                    UI.ToggleActiveClass(item, item.name == s);
                });
                UI.System.Q("ToolOptions").Query(null, "style-option-group").ForEach((item) => {
                    UI.ToggleDisplay(item, false);
                });
                if (s == "Paint") {
                    UI.ToggleDisplay("ToolOptions", true);
                    UI.ToggleDisplay("PaintOptions", true);
                }
                if (s == "Texture") {
                    UI.ToggleDisplay("ToolOptions", true);
                    UI.ToggleDisplay("TextureOptions", true);
                }
                if (s == "Eraser") {
                    UI.ToggleDisplay("ToolOptions", false);
                }
            });
        }

        // // Data
        // root.Q("DataOptions").Q("Save").RegisterCallback<ClickEvent>((evt) => {
        //     OpenSaveModal(new ClickEvent());
        // });
        // root.Q("DataOptions").Q("Open").RegisterCallback<ClickEvent>((evt) => {
        //     OpenOpenModal(new ClickEvent());
        // });
        // root.Q("DataOptions").Q("Reset").RegisterCallback<ClickEvent>((evt) => {
        //     ResetConfirm(new ClickEvent());
        // });

        // // Environment
        // root.Q("EnvOptions").Q("LightAngle").RegisterCallback<ChangeEvent<float>>((evt) => {
        //     TerrainController.LightAngle = evt.newValue;
        //     TerrainController.UpdateLight();
        // });
        // root.Q("EnvOptions").Q("LightHeight").RegisterCallback<ChangeEvent<float>>((evt) => {
        //     TerrainController.LightHeight = evt.newValue;
        //     TerrainController.UpdateLight();
        // });
        // root.Q("EnvOptions").Q("LightIntensity").RegisterCallback<ChangeEvent<float>>((evt) => {
        //     TerrainController.LightIntensity = evt.newValue;
        //     TerrainController.UpdateLight();
        // });
        // root.Q("EnvOptions").Q("TopBgColor").RegisterCallback<ClickEvent>((evt) => {
        //     Modal.Reset("Set Top Background Color");
        //     Modal.AddColorField("TopBgColor");
        //     Modal.AddPreferredButton("Close", Modal.CloseEvent);
        // });
        // root.Q("EnvOptions").Q("BotBgColor").RegisterCallback<ClickEvent>((evt) => {
        //     Modal.Reset("Set Bottom Background Color");
        //     Modal.AddColorField("BotBgColor");
        //     Modal.AddPreferredButton("Close", Modal.CloseEvent);
        // });
        // root.Q("EnvOptions").Q("TopBlockColor").RegisterCallback<ClickEvent>((evt) => {
        //     Modal.Reset("Set Default Block Top Color");
        //     Modal.AddColorField("TopBlockColor");
        //     Modal.AddPreferredButton("Close", Modal.CloseEvent);
        // });
        // root.Q("EnvOptions").Q("SideBlockColor").RegisterCallback<ClickEvent>((evt) => {
        //     Modal.Reset("Set Default Block Side Color");
        //     Modal.AddColorField("SideBlockColor");
        //     Modal.AddPreferredButton("Close", Modal.CloseEvent);
        // });

        // VisualElement styleSearch = SearchField.Create(StringUtility.Arr("None", "Paint", "Acid Flow", "Acid", "Old Brick", "Brick", "Gray Brick", "White Brick", "Dry Grass", "Grass", "Gold", "Lava Flow", "Lava", "Metal", "Gray Metal", "Poison Flow", "Poison", "Sand", "Snow", "Soil", "Stone", "Small Tile", "Big Tile", "Water Flow", "Water", "Wood", "Old Wood"), "Style Search");
        // styleSearch.name = "StyleSearch";
        // styleSearch.style.minWidth = 300;
        // root.Q("StyleOptions").Add(styleSearch);
        // styleSearch.BringToFront();
    }

    public static void ToggleEditMode(ClickEvent evt) {
        if (Cursor.Mode != CursorMode.Editing) {
            StartEditing();
        }
        else {
            EndEditing();
        }
    }

    private static void StartEditing() {
        UI.ToggleDisplay("ToolsPanel", true);
        Block.DeselectAll();
        Token.DeselectAll();
        Token.UnfocusAll();
        Block.ToggleSpacers(true);
        Cursor.Mode = CursorMode.Editing;
        UI.ToggleActiveClass(UI.System.Q("FloatingControls").Q("EditMap"), true);
        UI.ToggleDisplay("BottomBar", false);
    }

    private static void EndEditing() {
        Cursor.Mode = CursorMode.Default;
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleDisplay("ColorPanel", false);
        UI.ToggleDisplay("ToolOptions", false);
        Block.ToggleSpacers(false);
        State.SetCurrentJson();
        Player.Self().CmdMapSync();
        UI.ToggleActiveClass(UI.System.Q("FloatingControls").Q("EditMap"), false);
        UI.ToggleDisplay("BottomBar", true);
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
        TerrainController.ResetTerrain();
        Toast.Add("Map reset.");
    }

    private static void OpenSaveModal(ClickEvent evt) {
        Modal.Reset("Save Map");

        VisualElement modal = Modal.Find();

        TextField filenameField = new TextField();
        filenameField.name = "Filename";
        filenameField.label = "Filename";
        if (CurrentFile.Length > 0) {
            filenameField.value = CurrentFile;
        }
        filenameField.AddToClassList("no-margin");
        filenameField.style.minWidth = 400;
        modal.Q("Contents").Add(filenameField);

        Button confirm = new Button();
        confirm.text = "Confirm";
        confirm.RegisterCallback<ClickEvent>(ConfirmMapSave);
        confirm.AddToClassList("preferred");
        modal.Q("Buttons").Add(confirm);

        Button cancel = new Button();
        cancel.text = "Cancel";
        cancel.RegisterCallback<ClickEvent>(Modal.CloseEvent);
        modal.Q("Buttons").Add(cancel);
    }

    private static void OpenOpenModal(ClickEvent evt) {
        Modal.Reset("Open Map");
        Modal.AddSearchField("SearchField", "Filename", "", GetAllMapFiles());
        Modal.AddPreferredButton("Confirm", ConfirmMapOpen);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ConfirmMapOpen(ClickEvent evt) {
        string value = Modal.Find().Q("SearchField").Q<TextField>("SearchInput").value;
        if (!TerrainController.MapDirty) {
            OpenFile();
            Modal.Close();
        }
        else {
            Modal.DoubleConfirm("Confirm Open", "You have unsaved changes. Discard?", OpenFile);
        }
    }

    private static void OpenFile() {
        string filename = Modal.Find().Q("SearchField").Q<TextField>("SearchInput").value;
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
        string value = Modal.Find().Q<TextField>("Filename").value;
        if (value.Length == 0) {
            Toast.Add("Not a valid filename.", ToastType.Error);
        }
        else {
            if (value.EndsWith(".json")) {
                value = value.Replace(".json", "");
            }
            if (!value.EndsWith(".png")) {
                value += ".png";
                Modal.Find().Q<TextField>("Filename").value = value;
            }
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
        string filename = Modal.Find().Q<TextField>("Filename").value;
        string path = PlayerPrefs.GetString("DataFolder", Application.persistentDataPath);
        string fullPath = path + "/maps/" + filename;
        MapSaver.StegSave(fullPath);
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

    private static void RegisterButton(Button button) {
        button.clickable.clickedWithEventInfo += ButtonClick;
    }

    private static void ButtonClick(EventBase obj) {
        VisualElement root = UI.System.Q("ToolsPanel");
        root.Query(null, "tool-button").ForEach((item) => {
            item.RemoveFromClassList("active");
        });
        Button button = (Button)obj.target;
        button.AddToClassList("active");
        EditOp = button.name;
        ButtonActions(button);
    }

    private static void OpenOptionFlyout(VisualElement v) {
        UI.ToggleDisplay("ClickCatcher", true);
        UI.ToggleDisplay(v, true);
        UI.System.Q("ToolsPanel").Q(ShapeOp).AddToClassList("active");
        UI.System.Q("ToolsPanel").Q(MultiOp).AddToClassList("active");
        UI.System.Q("ToolsPanel").Q(StyleOp).AddToClassList("active");
    }

    private static void CloseOptionFlyout(ClickEvent evt) {
        UI.ToggleDisplay("ClickCatcher", false);
        VisualElement root = UI.System.Q("ToolsPanel");
        root.Query(null, "tool-options").ForEach((item) => {
            UI.ToggleDisplay(item, false);
        });
    }

    private static void ButtonActions(Button button) {
        CloseOptionFlyout(new ClickEvent());
        if (StringUtility.Arr("ChangeShape", "MultiBlock", "StyleBlock").ToList().Contains(button.name)) {
            OpenOptionFlyout(button.Q("Options"));
        }

        UI.ToggleDisplay("ToolOptions", false);
        if (StringUtility.Arr("StyleBlock").ToList().Contains(button.name)) {
            UI.ToggleDisplay("ToolOptions", true);
            UI.ToggleDisplay($"{button.name}Options", true);
        }
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
                Environment.Color1 = c;
                Block.SetColor("top1", c);
                Block.SetColor("top2", ColorUtility.DarkenColor(c, .2f));
                break;
            case "SideBlockColor":
                Environment.Color2 = c;
                Block.SetColor("side1", c);
                Block.SetColor("side2", ColorUtility.DarkenColor(c, .2f));
                break;
            case "TopBgColor":
                Environment.Color3 = c;
                MeshRenderer mra = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
                mra.material.SetColor("_Color1", c);
                break;
            case "BotBgColor":
                Environment.Color4 = c;
                MeshRenderer mrb = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
                mrb.material.SetColor("_Color2", c);
                break;
            case "Color5":
                Environment.Color5 = c;
                break;
            case "Color6":
                Environment. Color6 = c;
                break;
        }
    }

}
