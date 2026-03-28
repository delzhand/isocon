using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class Config
{
    public static void OpenModal(bool isTabletopMode)
    {
        var dialog = Modal2.SetCurrentDialog("ShunDialog1");
        var dialogContent = Modal2.Contents("ShunDialog1");
        if (isTabletopMode)
        {
            Modal2.SetCloseAction(StateManager.ToNeutral);
        }
        dialogContent.Clear();

        Modal2.AddDialogHeader("Settings");

        Dictionary<string, string> tabs = new();
        tabs.Add("Interface", "Interface");
        tabs.Add("Actors", "Actors");
        tabs.Add("Misc", "Other");
        var configTabs = Modal2.AddTabs("ConfigTabs", tabs);

        var dataPathField = Modal2.AddTextField("DataPath", "Data Path", Preferences.Current.DataPath, "The directory where tokens, maps, sessions, etc will be saved");
        Modal2.MoveToTab(dataPathField, configTabs, "Misc");

        var hudField = Modal2.AddSwitchField("ShowHUD", "Display Info HUD", Preferences.Current.ShowHUD, "Show an overlay with connection and player information");
        Modal2.MoveToTab(hudField, configTabs, "Interface");

        var indicatorsField = Modal2.AddSwitchField("ShowIndicators", "Display Tile Indicators", Preferences.Current.ShowIndicators, "Show tile coordinates");
        Modal2.MoveToTab(indicatorsField, configTabs, "Interface");

        List<string> scaleOptions = new();
        for (int i = 75; i <= 250; i += 25)
        {
            scaleOptions.Add(i + "%");
        }

        var uiScaleField = Modal2.AddSelectField("UIScale", "UI Scale", Preferences.Current.UIScale, scaleOptions, "Control how large the user interface appears");
        Modal2.MoveToTab(uiScaleField, configTabs, "Interface");

        var wUIScaleField = Modal2.AddSelectField("WUIScale", "Actor UI Scale", Preferences.Current.WorldUIScale, scaleOptions, "Control how large the floating elements above actors appear");
        Modal2.MoveToTab(wUIScaleField, configTabs, "Actors");

        List<string> fpsOptions = StringUtility.CreateArray("15", "30", "60", "90", "120").ToList<string>();
        var fpsField = Modal2.AddToggleField("FPSLimit", "FPS Limit", $"{Preferences.Current.TargetFramerate}", fpsOptions, false, "Set a cap on rendering speed");
        Modal2.MoveToTab(fpsField, configTabs, "Misc");

        int bbOpacity = Mathf.RoundToInt(Preferences.Current.BlockBorderOpacity);
        var blockBorderField = Modal2.AddSliderField("BlockBorder", "Block Border Minimum", bbOpacity, "Set a minimum opacity on block borders when not dragging an actor");
        Modal2.MoveToTab(blockBorderField, configTabs, "Misc");

        var actorBorderField = Modal2.AddSelectField("ActorBorder", "Token Outline Color", Preferences.Current.TokenOutline, ColorUtility.CommonColors().ToList<string>(), "Set an outline color for improved token contrast");
        Modal2.MoveToTab(actorBorderField, configTabs, "Actors");

        Dictionary<string, string> dragOptions = new();
        dragOptions.Add("Pan", "Rotate with Middle, Pan with Right");
        dragOptions.Add("Drag", "Rotate with Right, Pan with Middle");
        var rightClickField = Modal2.AddSelectField("CameraControls", "Camera Controls", dragOptions[Preferences.Current.DragPan ? "Drag" : "Pan"], dragOptions.Values.ToList<string>(), "Choose which mouse buttons rotate and pan the camera");
        Modal2.MoveToTab(rightClickField, configTabs, "Interface");

        var footer = Modal2.AddDialogFooter(() =>
        {
            dialog.Close();
        });

        var confirm = new ShunDialogClose();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Save Config";
        confirm.clicked += () =>
        {
            SaveConfig();
            dialog.Close();
        };
        footer.Add(confirm);

        dialog.Open();
    }

    private static void SaveConfig()
    {
        // var dialogContent = Modal2.Contents("ShunDialog1");
        Preferences.Current.DataPath = Modal2.GetTextFieldValue("ShunDialog1", "DataPath");
        Preferences.Current.ShowHUD = Modal2.GetSwitchFieldValue("ShunDialog1", "ShowHUD");
        Preferences.Current.UIScale = Modal2.GetSelectFieldValue("ShunDialog1", "UIScale");
        Preferences.Current.WorldUIScale = Modal2.GetSelectFieldValue("ShunDialog1", "WUIScale");
        Preferences.Current.DragPan = Modal2.GetSelectFieldValue("ShunDialog1", "CameraControls") == "Rotate with Right, Pan with Middle";

        Preferences.Current.ShowIndicators = Modal2.GetSwitchFieldValue("ShunDialog1", "ShowIndicators");
        Block.ToggleIndicators(Preferences.Current.ShowIndicators);

        Preferences.Current.BlockBorderOpacity = Modal2.GetSliderFieldValue("ShunDialog1", "BlockBorder");
        BlockRendering.ToggleAllBorders(false);

        Preferences.Current.TokenOutline = Modal2.GetSelectFieldValue("ShunDialog1", "ActorBorder");
        Actor.SetAllTokenOutlines();

        float uiValue = float.Parse(Preferences.Current.UIScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = uiValue;

        float wuiValue = float.Parse(Preferences.Current.WorldUIScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/WorldUI").GetComponent<UIDocument>().panelSettings.scale = wuiValue;

        int fpsValue = int.Parse(Modal2.GetToggleFieldValues("ShunDialog1", "FPSLimit").First());
        fpsValue = Math.Max(fpsValue, 3);
        Preferences.Current.TargetFramerate = fpsValue;
        Application.targetFrameRate = fpsValue;

        Preferences.Save();
        Toast.AddSuccess("Configuration saved.");
    }
}
