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
    public static string ShapeOp = "ShapeSlope";
    public static string ResizeOp = "ResizeCloneCol";
    public static string StyleOp = "StylePaint";

    public static void Setup()
    {
        UI.SetBlocking(UI.System, "ToolsPanel");

        VisualElement toolsRoot = UI.System.Q("ToolsPanel");
        VisualElement optionsRoot = UI.System.Q("ToolOptions");

        UI.ToggleDisplay(toolsRoot, false);
        UI.ToggleDisplay(optionsRoot, false);

        UI.System.Q("ClickCatcher").RegisterCallback<ClickEvent>(CloseSubtoolFlyouts);

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
                    LongPress.Add(tool.name);
                }
                if (toolButton.ClassListContains("has-options")) {
                    OpenOptions(toolButton.name);
                }
                if (toolButton.ClassListContains("has-subtool-options")) {
                    OpenOptions(toolButton.name, true);
                }
            });       
            if (toolButton.ClassListContains("has-subtools")) {
                toolButton.RegisterCallback<MouseUpEvent>((evt) => {
                    LongPress.ClearAll();
                });
            }
        });

        // Subtools
        toolsRoot.Query(null, "subtool").ForEach((tool) => {
            string s = tool.name;
            toolsRoot.Q<Button>(s).RegisterCallback<ClickEvent>((evt) => {
                Debug.Log(s);
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

    public static void LongPressResult(string claim) {
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

    // public static void OptionsSetup() {
    //     VisualElement toolsRoot = UI.System.Q("ToolsPanel");
    //     VisualElement optionsRoot = UI.System.Q("ToolOptions");

    //     // Shape
    //     LongPressChecker(toolsRoot.Q<Button>("ChangeShape"));
    //     foreach (string s in StringUtility.Arr("ShapeSlope", "ShapeSolid", "ShapeHidden")) {
    //         toolsRoot.Q(s).RegisterCallback<ClickEvent>((evt) => {
    //             ShapeOp = s;
    //             toolsRoot.Query(null, "tool-sub-button").ForEach((item) => {
    //                 UI.ToggleActiveClass(item, item.name == s);
    //             });
    //             UI.ToggleDisplay(optionsRoot, false);
    //         });
    //     }

    //     // Multi
    //     foreach (string s in StringUtility.Arr("CloneRow", "CloneCol", "RemoveRow", "RemoveCol", "AddHeight")) {
    //         toolsRoot.Q(s).RegisterCallback<ClickEvent>((evt) => {
    //             ResizeOp = s;
    //             toolsRoot.Query(null, "tool-sub-button").ForEach((item) => {
    //                 UI.ToggleActiveClass(item, item.name == s);
    //             });
    //         });
    //     }

    //     // Style
    //     foreach (string s in StringUtility.Arr("Paint", "Texture", "Eraser")) {
    //         toolsRoot.Q(s).RegisterCallback<ClickEvent>((evt) => {
    //             StyleOp = s;
    //             toolsRoot.Query(null, "tool-sub-button").ForEach((item) => {
    //                 UI.ToggleActiveClass(item, item.name == s);
    //             });
    //             UI.System.Q("ToolOptions").Query(null, "style-option-group").ForEach((item) => {
    //                 UI.ToggleDisplay(item, false);
    //             });
    //             if (s == "Paint") {
    //                 UI.ToggleDisplay("ToolOptions", true);
    //                 UI.ToggleDisplay("PaintOptions", true);
    //             }
    //             if (s == "Texture") {
    //                 UI.ToggleDisplay("ToolOptions", true);
    //                 UI.ToggleDisplay("TextureOptions", true);
    //             }
    //             if (s == "Eraser") {
    //                 UI.ToggleDisplay("ToolOptions", false);
    //             }
    //         });
    //     }

    //     // Data
    //     optionsRoot.Q("DataOptions").Q("Save").RegisterCallback<ClickEvent>((evt) => {
    //         OpenSaveModal(new ClickEvent());
    //     });
    //     optionsRoot.Q("DataOptions").Q("Open").RegisterCallback<ClickEvent>((evt) => {
    //         OpenOpenModal(new ClickEvent());
    //     });
    //     optionsRoot.Q("DataOptions").Q("Reset").RegisterCallback<ClickEvent>((evt) => {
    //         ResetConfirm(new ClickEvent());
    //     });

    //     // Environment
    //     optionsRoot.Q("EnvOptions").Q("LightAngle").RegisterCallback<ChangeEvent<float>>((evt) => {
    //         TerrainController.LightAngle = evt.newValue;
    //         TerrainController.UpdateLight();
    //     });
    //     optionsRoot.Q("EnvOptions").Q("LightHeight").RegisterCallback<ChangeEvent<float>>((evt) => {
    //         TerrainController.LightHeight = evt.newValue;
    //         TerrainController.UpdateLight();
    //     });
    //     optionsRoot.Q("EnvOptions").Q("LightIntensity").RegisterCallback<ChangeEvent<float>>((evt) => {
    //         TerrainController.LightIntensity = evt.newValue;
    //         TerrainController.UpdateLight();
    //     });
    //     optionsRoot.Q("EnvOptions").Q("TopBgColor").RegisterCallback<ClickEvent>((evt) => {
    //         Modal.Reset("Set Top Background Color");
    //         Modal.AddColorField("TopBgColor");
    //         Modal.AddPreferredButton("Close", Modal.CloseEvent);
    //     });
    //     optionsRoot.Q("EnvOptions").Q("BotBgColor").RegisterCallback<ClickEvent>((evt) => {
    //         Modal.Reset("Set Bottom Background Color");
    //         Modal.AddColorField("BotBgColor");
    //         Modal.AddPreferredButton("Close", Modal.CloseEvent);
    //     });
    //     optionsRoot.Q("EnvOptions").Q("TopBlockColor").RegisterCallback<ClickEvent>((evt) => {
    //         Modal.Reset("Set Default Block Top Color");
    //         Modal.AddColorField("TopBlockColor");
    //         Modal.AddPreferredButton("Close", Modal.CloseEvent);
    //     });
    //     optionsRoot.Q("EnvOptions").Q("SideBlockColor").RegisterCallback<ClickEvent>((evt) => {
    //         Modal.Reset("Set Default Block Side Color");
    //         Modal.AddColorField("SideBlockColor");
    //         Modal.AddPreferredButton("Close", Modal.CloseEvent);
    //     });
    // }

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
