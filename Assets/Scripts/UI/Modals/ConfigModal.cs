using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class ConfigModal
{
    public static void Open()
    {
        Modal2.CreateContext("PrimaryDialog");
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

        var uiScaleField = Modal2.AddComboboxField("UIScale", "UI Scale", Preferences.Current.UIScale, scaleOptions, "Control how large the user interface appears");
        Modal2.MoveToTab(uiScaleField, configTabs, "Interface");

        var wUIScaleField = Modal2.AddComboboxField("WUIScale", "Actor UI Scale", Preferences.Current.WorldUIScale, scaleOptions, "Control how large the floating elements above actors appear");
        Modal2.MoveToTab(wUIScaleField, configTabs, "Actors");


        var tokenScaleField = Modal2.AddComboboxField("TokenScale", "Actor Scale", $"{Preferences.Current.TokenScale * 100}%", scaleOptions, "Control how large the actors appear");
        Modal2.MoveToTab(tokenScaleField, configTabs, "Actors");

        List<string> fpsOptions = StringUtility.CreateArray("15", "30", "60", "90", "120").ToList<string>();
        var fpsField = Modal2.AddToggleField("FPSLimit", "FPS Limit", $"{Preferences.Current.TargetFramerate}", fpsOptions, false, "Set a cap on rendering speed");
        Modal2.MoveToTab(fpsField, configTabs, "Misc");

        int bbOpacity = Mathf.RoundToInt(Preferences.Current.BlockBorderOpacity);
        var blockBorderField = Modal2.AddSliderField("BlockBorder", "Block Border Minimum", bbOpacity, "Set a minimum opacity on block borders when not dragging an actor");
        Modal2.MoveToTab(blockBorderField, configTabs, "Misc");

        var skipTutorials = Modal2.AddSwitchField("SkipTutorials", "Skip Tutorials", Preferences.Current.SkipTutorials, "Never show tutorials");
        Modal2.MoveToTab(skipTutorials, configTabs, "Misc");

        var colorOptions = ColorUtility.CommonColors().ToList<string>();
        colorOptions.Insert(0, "None");
        var actorBorderField = Modal2.AddComboboxField("ActorBorder", "Token Outline Color", Preferences.Current.TokenOutline, colorOptions, "Set an outline color for improved token contrast");
        Modal2.MoveToTab(actorBorderField, configTabs, "Actors");

        Dictionary<string, string> dragOptions = new();
        dragOptions.Add("Pan", "Rotate with Middle, Pan with Right");
        dragOptions.Add("Rotate", "Pan with Middle, Rotate with Right");
        var rightClickField = Modal2.AddSelectField("CameraControls", "Camera Controls", dragOptions[Preferences.Current.PanWithRight ? "Pan" : "Rotate"], dragOptions.Values.ToList<string>(), "Choose which mouse buttons rotate and pan the camera");
        Modal2.MoveToTab(rightClickField, configTabs, "Interface");

        var footer = Modal2.AddDialogFooter();

        var confirm = new ShunDialogClose();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Save Config";
        confirm.clicked += () =>
        {
            SaveConfig();
            Modal2.Close();
        };
        footer.Add(confirm);

        Modal2.Open("Config");
    }

    private static void SaveConfig()
    {
        Modal2.ReadContext("PrimaryDialog");
        Preferences.Current.DataPath = Modal2.GetTextFieldValue("DataPath");

        Preferences.Current.SkipTutorials = Modal2.GetSwitchFieldValue("SkipTutorials");

        Preferences.Current.PanWithRight = Modal2.GetSelectFieldValue("CameraControls") == "Rotate with Middle, Pan with Right";
        Viewport.SetPanMode(Preferences.Current.PanWithRight);

        Preferences.Current.ShowHUD = Modal2.GetSwitchFieldValue("ShowHUD");
        UI.ToggleDisplay("DetailsHud", Preferences.Current.ShowHUD);

        Preferences.Current.ShowIndicators = Modal2.GetSwitchFieldValue("ShowIndicators");
        Block.ToggleIndicators(Preferences.Current.ShowIndicators);

        Preferences.Current.BlockBorderOpacity = Modal2.GetSliderFieldValue("BlockBorder");
        BlockRendering.ToggleAllBorders(false);

        Preferences.Current.TokenOutline = Modal2.GetComboboxFieldValue("ActorBorder");
        Actor.SetAllTokenOutlines();

        float tokenScaleValue = float.Parse(Modal2.GetComboboxFieldValue("TokenScale").Replace("%", "")) / 100f;
        Preferences.Current.TokenScale = tokenScaleValue;

        Preferences.Current.UIScale = Modal2.GetComboboxFieldValue("UIScale");
        float uiValue = float.Parse(Preferences.Current.UIScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = uiValue;

        Preferences.Current.WorldUIScale = Modal2.GetComboboxFieldValue("WUIScale");
        float wuiValue = float.Parse(Preferences.Current.WorldUIScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/WorldUI").GetComponent<UIDocument>().panelSettings.scale = wuiValue;

        int fpsValue = int.Parse(Modal2.GetToggleFieldValues("FPSLimit").First());
        fpsValue = Math.Max(fpsValue, 3);
        Preferences.Current.TargetFramerate = fpsValue;
        Application.targetFrameRate = fpsValue;

        Preferences.Save();
        Toast.AddSuccess("Configuration saved.");
    }
}
