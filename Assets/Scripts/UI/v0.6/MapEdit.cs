using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public class MapEdit
{
    private static string CurrentFile = "";
    private static string EditOp;

    public static BlockType Shape;

    public static void Setup()
    {
        VisualElement root = UI.System.Q("ToolsPanel");
        UI.SetBlocking(UI.System, "ToolsPanel");
        UI.ToggleDisplay(root, false);
        root.Query<Button>(null, "tool-button").ForEach(RegisterButton);

        // ColorEdit.Setup();
        // RegisterColorChangeCallback("Color1");
        // RegisterColorChangeCallback("Color2");
        // RegisterColorChangeCallback("Color3");
        // RegisterColorChangeCallback("Color4");
        // RegisterColorChangeCallback("Color5");
        // RegisterColorChangeCallback("Color6");

        OptionsSetup();
    }

    public static void OptionsSetup() {
        VisualElement root = UI.System.Q("ToolOptions");
        UI.SetBlocking(UI.System, "ToolOptions");
        UI.ToggleDisplay(root, false);

        // Shape
        root.Q("ShapeSolid").AddToClassList("active");
        root.Q("ShapeSolid").RegisterCallback<ClickEvent>((evt) => {
            Shape = BlockType.Solid;
            root.Q("ShapeSolid").AddToClassList("active");
            root.Q("ShapeSlope").RemoveFromClassList("active");
            root.Q("ShapeHidden").RemoveFromClassList("active");
        });
        root.Q("ShapeSlope").RegisterCallback<ClickEvent>((evt) => {
            Shape = BlockType.Slope;
            root.Q("ShapeSolid").RemoveFromClassList("active");
            root.Q("ShapeSlope").AddToClassList("active");
            root.Q("ShapeHidden").RemoveFromClassList("active");
        });
        root.Q("ShapeHidden").RegisterCallback<ClickEvent>((evt) => {
            Shape = BlockType.Hidden;
            root.Q("ShapeSolid").RemoveFromClassList("active");
            root.Q("ShapeSlope").RemoveFromClassList("active");
            root.Q("ShapeHidden").AddToClassList("active");
        });

        // Data
        root.Q("DataOptions").Q("Save").RegisterCallback<ClickEvent>((evt) => {
            OpenSaveModal(new ClickEvent());
        });
        root.Q("DataOptions").Q("Open").RegisterCallback<ClickEvent>((evt) => {
            OpenOpenModal(new ClickEvent());
        });
        root.Q("DataOptions").Q("Reset").RegisterCallback<ClickEvent>((evt) => {
            ResetConfirm(new ClickEvent());
        });

        // Environment
        root.Q("EnvOptions").Q("LightAngle").RegisterCallback<ChangeEvent<float>>((evt) => {
            TerrainController.LightAngle = evt.newValue;
            TerrainController.UpdateLight();
        });
        root.Q("EnvOptions").Q("LightHeight").RegisterCallback<ChangeEvent<float>>((evt) => {
            TerrainController.LightHeight = evt.newValue;
            TerrainController.UpdateLight();
        });
        root.Q("EnvOptions").Q("LightIntensity").RegisterCallback<ChangeEvent<float>>((evt) => {
            TerrainController.LightIntensity = evt.newValue;
            TerrainController.UpdateLight();
        });
        root.Q("EnvOptions").Q("TopBgColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Top Background Color");
            Modal.AddColorField("TopBgColor");
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        root.Q("EnvOptions").Q("BotBgColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Bottom Background Color");
            Modal.AddColorField("BotBgColor");
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        root.Q("EnvOptions").Q("TopBlockColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Default Block Top Color");
            Modal.AddColorField("TopBlockColor");
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });
        root.Q("EnvOptions").Q("SideBlockColor").RegisterCallback<ClickEvent>((evt) => {
            Modal.Reset("Set Default Block Side Color");
            Modal.AddColorField("SideBlockColor");
            Modal.AddPreferredButton("Close", Modal.CloseEvent);
        });

        // VisualElement styleSearch = SearchField.Create(StringUtility.Arr("None", "Paint", "Acid Flow", "Acid", "Old Brick", "Brick", "Gray Brick", "Dry Grass", "Grass", "Gold", "Lava Flow", "Lava", "Metal", "Gray Metal", "Poison Flow", "Poison", "Sand", "Snow", "Soil", "Stone", "Water Flow", "Water", "Wood", "Old Wood"), "Style Search");
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
        Cursor.Mode = CursorMode.Editing;
        UI.ToggleActiveClass(UI.System.Q("FloatingControls").Q("EditMap"), true);
        UI.ToggleDisplay("BottomBar", false);
    }

    private static void EndEditing() {
        Cursor.Mode = CursorMode.Default;
        UI.ToggleDisplay("ToolsPanel", false);
        UI.ToggleDisplay("ColorPanel", false);
        UI.ToggleDisplay("ToolOptions", false);
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
        root.Query<Button>(null, "tool-button").ForEach(DisableButton);
        Button button = (Button)obj.target;
        button.AddToClassList("active");
        EditOp = button.name;
        ShowOptions(button.name);
    }

    private static void ShowOptions(string op) {
        UI.ToggleDisplay("ToolOptions", false);
        UI.System.Q("ToolOptions").Query(null, "tool-option-group").ForEach((item) => {
            UI.ToggleDisplay(item, false);
        });
        if (op == "ChangeShape") {
            UI.ToggleDisplay("ToolOptions", true);
            UI.ToggleDisplay("ShapeOptions", true);
        }
        if(op == "StyleBlock") {
            UI.ToggleDisplay("ToolOptions", true);
            UI.ToggleDisplay("StyleOptions", true);
        }
        if (op == "MultiBlock") {
            UI.ToggleDisplay("ToolOptions", true);
            UI.ToggleDisplay("MultiOptions", true);
        }
        if (op == "Data") {
            UI.ToggleDisplay("ToolOptions", true);
            UI.ToggleDisplay("DataOptions", true);
        }
        if (op == "Environment") {
            UI.ToggleDisplay("ToolOptions", true);
            UI.ToggleDisplay("EnvOptions", true);
        }
    }

    private static void DisableButton(Button button) {
        button.RemoveFromClassList("active");
    }

    public static string GetOp() {
        return EditOp;
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

    // private static void RegisterColorChangeCallback(string elementName) {
    //     UI.System.Q(elementName).RegisterCallback<ClickEvent>((evt) => {

    //         UI.ToggleDisplay("ColorPanel", true);
    //         ColorEdit.ClearColorChangeListeners();
    //         ColorEdit.SetColor(UI.System.Q(elementName).resolvedStyle.backgroundColor);
    //         ColorEdit.onColorChange += (c) => HandleColorChange(elementName, c);
    //     });
    // }

    // private static void HandleColorChange(string elementName, Color c) {
    //     UI.System.Q(elementName).style.backgroundColor = c;
    //     switch(elementName) {
    //         case "Color1":
    //             Environment.Color1 = c;
    //             Block.SetColor("top1", c);
    //             Block.SetColor("top2", ColorUtility.DarkenColor(c, .2f));
    //             break;
    //         case "Color2":
    //             Environment.Color2 = c;
    //             Block.SetColor("side1", c);
    //             Block.SetColor("side2", ColorUtility.DarkenColor(c, .2f));
    //             break;
    //         case "Color3":
    //             Environment.Color3 = c;
    //             MeshRenderer mra = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
    //             mra.material.SetColor("_Color1", c);
    //             break;
    //         case "Color4":
    //             Environment.Color4 = c;
    //             MeshRenderer mrb = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
    //             mrb.material.SetColor("_Color2", c);
    //             break;
    //         case "Color5":
    //             Environment.Color5 = c;
    //             break;
    //         case "Color6":
    //             Environment. Color6 = c;
    //             break;
    //     }
    // }
}
