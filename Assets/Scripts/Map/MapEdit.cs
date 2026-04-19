using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UIElements;

public enum BlockFocusMode
{
    Single,
    Row,
    Column
}

public class MapEdit
{
    public static string EditOp = "AddBlock";
    public static string ShapeOp = "ShapeSolid";
    public static string ResizeOp = "ResizeCloneCol";
    public static string StyleOp = "StylePaint";

    public static BlockFocusMode FocusMode = BlockFocusMode.Single;

    public static void Setup()
    {
        VisualElement toolsRoot = UI.System.Q("ToolsPanel");
        VisualElement optionsRoot = UI.System.Q("ToolOptions");

        UI.ToggleDisplay(toolsRoot, false);
        UI.ToggleDisplay(optionsRoot, false);

        UI.System.Q("ClickCatcher").RegisterCallback<ClickEvent>(CloseSubtoolFlyouts);

        OptionsSetup();

        toolsRoot.Query(null, "tool").ForEach((tool) =>
        {
            FocusMode = BlockFocusMode.Single;
            Button toolButton = tool as Button;
            // Clear handler that eats mousedown
            toolButton.clickable.activators.Clear();
            // Tooltip show
            toolButton.Q("Icon").RegisterCallback<MouseEnterEvent>((evt) =>
            {
                UI.ToggleDisplay(toolButton.Q("Tooltip"), true);
            });
            // Tooltip hide
            toolButton.Q("Icon").RegisterCallback<MouseLeaveEvent>((evt) =>
            {
                UI.ToggleDisplay(toolButton.Q("Tooltip"), false);
            });
            toolButton.RegisterCallback<MouseDownEvent>((evt) =>
            {
                UI.ToggleDisplay("ClickCatcher", false);
                // set op
                EditOp = tool.name;
                if (EditOp == "StyleBlock")
                {
                    Tutorial.Init("style shortcut");
                }
                // set active class on current
                toolsRoot.Query(null, "tool").ForEach((item) =>
                {
                    UI.ToggleActiveClass(item, item.name == tool.name);
                });
                // close options
                UI.ToggleDisplay(optionsRoot, false);
                // close flyouts
                CloseSubtoolFlyouts(new ClickEvent());
                if (toolButton.ClassListContains("has-subtools"))
                {
                    Tutorial.Init("subtools");
                    if (Input.GetMouseButtonDown(1))
                    {
                        ShowSubtools(tool.name);
                    }
                }
                if (toolButton.ClassListContains("has-options"))
                {
                    OpenOptions(toolButton.name);
                }
                if (toolButton.ClassListContains("has-subtool-options"))
                {
                    OpenOptions(toolButton.name, true);
                }
            });
        });

        toolsRoot.Query(null, "subtool").ForEach((tool) =>
        {
            string s = tool.name;
            toolsRoot.Q<Button>(s).RegisterCallback<ClickEvent>((evt) =>
            {
                if (s.StartsWith("Shape"))
                {
                    ShapeOp = s;
                    toolsRoot.Q("ChangeShape").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
                }
                else if (s.StartsWith("Style"))
                {
                    StyleOp = s;
                    toolsRoot.Q("StyleBlock").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
                }
                else if (s.StartsWith("Resize"))
                {
                    ResizeOp = s;
                    toolsRoot.Q("ResizeMap").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
                }
                ActivateSubtool();
                CloseSubtoolFlyouts(new ClickEvent());
                if (tool.ClassListContains("has-options"))
                {
                    OpenOptions(tool.name);
                }
            });
        });
    }

    public static void ShowSubtools(string claim)
    {
        UI.ToggleDisplay(UI.System.Q("ToolsPanel").Q(claim).Q("Tooltip"), false);
        OpenSubtoolFlyout(UI.System.Q("ToolsPanel").Q(claim).Q("Options"));
    }

    public static void OpenOptions(string s, bool sub = false)
    {
        VisualElement optionsRoot = UI.System.Q("ToolOptions");
        UI.ToggleDisplay(optionsRoot, true);
        if (sub && s == "StyleBlock")
        {
            s = StyleOp;
        }
        optionsRoot.Query(null, "tool-options").ForEach((item) =>
        {
            UI.ToggleDisplay(item, item.name.StartsWith(s));
        });
    }

    public static void OptionsSetup()
    {

        VisualElement optionsRoot = UI.System.Q("ToolOptions");

        // Environment
        optionsRoot.Q("EnvironmentOptions").Q("LightAngle").RegisterCallback<ChangeEvent<float>>((evt) =>
        {
            TerrainController.LightAngle = evt.newValue;
            TerrainController.UpdateLight();
        });
        optionsRoot.Q("EnvironmentOptions").Q("LightHeight").RegisterCallback<ChangeEvent<float>>((evt) =>
        {
            TerrainController.LightHeight = evt.newValue;
            TerrainController.UpdateLight();
        });
        optionsRoot.Q("EnvironmentOptions").Q("LightIntensity").RegisterCallback<ChangeEvent<float>>((evt) =>
        {
            TerrainController.LightIntensity = evt.newValue;
            TerrainController.UpdateLight();
        });
        optionsRoot.Q("EnvironmentOptions").Q("TopBgColor").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Set Top Background Color");
            Modal2.AddColorField("TopBgColor", Environment.BgTopColor);
            Modal2.AddDialogFooter("Close");
            Modal2.Open("Color");
        });
        optionsRoot.Q("EnvironmentOptions").Q("BotBgColor").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Set Bottom Background Color");
            Modal2.AddColorField("BotBgColor", Environment.BgBottomColor);
            Modal2.AddDialogFooter("Close");
            Modal2.Open("Color");
        });
        optionsRoot.Q("EnvironmentOptions").Q("TopBlockColor").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Set Default Block Top Color");
            Modal2.AddColorField("TopBlockColor", Environment.TileTopColor);
            Modal2.AddDialogFooter("Close");
            Modal2.Open("Color");
        });
        optionsRoot.Q("EnvironmentOptions").Q("SideBlockColor").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Set Default Block Side Color");
            Modal2.AddColorField("SideBlockColor", Environment.TileSideColor);
            Modal2.AddDialogFooter("Close");
            Modal2.Open("Color");
        });

        // Tile Paint
        optionsRoot.Q("StylePaintOptions").Q("TopBlockPaint").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Set Block Top Paint");
            Modal2.AddColorField("TopBlockPaint", Environment.CurrentPaintTop);
            Modal2.AddDialogFooter("Close");
            Modal2.Open("Color");
        });
        optionsRoot.Q("StylePaintOptions").Q("SideBlockPaint").RegisterCallback<ClickEvent>((evt) =>
        {
            Modal2.CreateContext("PrimaryDialog");
            Modal2.AddDialogHeader("Set Block Side Paint");
            Modal2.AddColorField("SideBlockPaint", Environment.CurrentPaintSide);
            Modal2.AddDialogFooter("Close");
            Modal2.Open("Color");
        });

    }

    private static void OpenSubtoolFlyout(VisualElement v)
    {
        UI.ToggleDisplay("ClickCatcher", true);
        // Close existing subtool flyouts
        UI.System.Q("ToolsPanel").Query(null, "subtool-flyout").ForEach((item) =>
        {
            UI.ToggleDisplay(item, false);
        });
        // Show passed-in flyout
        UI.ToggleDisplay(v, true);
        ActivateSubtool();
    }

    private static void CloseSubtoolFlyouts(ClickEvent evt)
    {
        UI.ToggleDisplay("ClickCatcher", false);
        UI.System.Q("ToolsPanel").Query(null, "subtool-flyout").ForEach((item) =>
        {
            UI.ToggleDisplay(item, false);
        });
    }

    private static void ActivateSubtool()
    {
        UI.System.Q("ToolsPanel").Query(null, "subtool").ForEach((tool) =>
        {
            UI.ToggleActiveClass(tool, StringUtility.CheckInList(tool.name, ShapeOp, ResizeOp, StyleOp));
        });
    }

    public static void OpenFile()
    {
        string filename = FileBrowser.Result[0];
        MapSaver.LegacyLoad(filename);
    }


    public static void WriteFile()
    {
        MapSaver.RegSave(FileBrowser.Result[0]);
    }

    public static void ColorChanged()
    {
        VisualElement root = UI.System.Q("ToolOptions");
        Color c = ColorField.FromSliders();
        root.Q(ColorField.CurrentName).style.backgroundColor = c;
        switch (ColorField.CurrentName)
        {
            case "TopBlockColor":
                Environment.TileTopColor = c;
                BlockRendering.SetSharedMaterialColor("top1", c);
                BlockRendering.SetSharedMaterialColor("top2", ColorUtility.DarkenColor(c, .2f));
                break;
            case "SideBlockColor":
                Environment.TileSideColor = c;
                BlockRendering.SetSharedMaterialColor("side1", c);
                BlockRendering.SetSharedMaterialColor("side2", ColorUtility.DarkenColor(c, .2f));
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
                Environment.CurrentPaintSide = c;
                break;
        }
    }

    public static void ActivateStylePaint()
    {
        VisualElement toolsRoot = UI.System.Q("ToolsPanel");
        string s = "StylePaint";
        StyleOp = s;
        toolsRoot.Q("StyleBlock").Q("Icon").style.backgroundImage = toolsRoot.Q(s).Q("Icon").resolvedStyle.backgroundImage;
    }
}
