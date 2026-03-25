using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShunUI;
using UnityEngine;
using UnityEngine.UIElements;

public class Config
{
    public static void OpenModal(ClickEvent evt)
    {
        var dialog = ShunDialogHelper.SetCurrentDialog("ShunDialog1");
        var dialogContent = ShunDialogHelper.Contents("ShunDialog1");
        // ShunDialogHelper.SetCloseAction(BackToNeutral);
        dialogContent.Clear();

        ShunDialogHelper.AddDialogHeader("Settings");

        Dictionary<string, string> tabs = new();
        tabs.Add("Interface", "Interface");
        tabs.Add("Actors", "Actors");
        tabs.Add("Misc", "Other");
        var configTabs = ShunDialogHelper.AddTabs("ConfigTabs", tabs);

        var dataPathField = ShunDialogHelper.AddTextField("DataPath", "Data Path", Preferences.Current.DataPath, "The directory where tokens, maps, sessions, etc will be saved");
        ShunDialogHelper.MoveToTab(dataPathField, configTabs, "Misc");

        var hudField = ShunDialogHelper.AddSwitchField("ShowHUD", "Display Info HUD", Preferences.Current.ShowHUD, "Show an overlay with connection and player information");
        ShunDialogHelper.MoveToTab(hudField, configTabs, "Interface");

        List<string> scaleOptions = new();
        for (int i = 75; i <= 250; i += 25)
        {
            scaleOptions.Add(i + "%");
        }

        var uiScaleField = ShunDialogHelper.AddSelectField("UIScale", "UI Scale", Preferences.Current.UIScale, scaleOptions, "Control how large the user interface appears");
        ShunDialogHelper.MoveToTab(uiScaleField, configTabs, "Interface");

        var wUIScaleField = ShunDialogHelper.AddSelectField("WUIScale", "Actor UI Scale", Preferences.Current.WorldUIScale, scaleOptions, "Control how large the floating elements above actors appear");
        ShunDialogHelper.MoveToTab(wUIScaleField, configTabs, "Actors");

        List<string> fpsOptions = StringUtility.CreateArray("15", "30", "60", "90", "120").ToList<string>();
        var fpsField = ShunDialogHelper.AddToggleField("FPSLimit", "FPS Limit", $"{Preferences.Current.TargetFramerate}", fpsOptions, false, "Set a cap on rendering speed");
        ShunDialogHelper.MoveToTab(fpsField, configTabs, "Misc");

        int bbOpacity = Mathf.RoundToInt(Preferences.Current.BlockBorderOpacity);
        var blockBorderField = ShunDialogHelper.AddSliderField("BlockBorder", "Block Border Minimum", bbOpacity, "Set a minimum opacity on block borders when not dragging an actor");
        ShunDialogHelper.MoveToTab(blockBorderField, configTabs, "Misc");

        var actorBorderField = ShunDialogHelper.AddSelectField("ActorBorder", "Token Outline Color", Preferences.Current.TokenOutline, ColorUtility.CommonColors().ToList<string>(), "Set an outline color for improved token contrast");
        ShunDialogHelper.MoveToTab(actorBorderField, configTabs, "Actors");

        Dictionary<string, string> dragOptions = new();
        dragOptions.Add("Pan", "Rotate with Middle, Pan with Right");
        dragOptions.Add("Drag", "Rotate with Right, Pan with Middle");
        var rightClickField = ShunDialogHelper.AddSelectField("CameraControls", "Camera Controls", dragOptions[Preferences.Current.DragPan ? "Drag" : "Pan"], dragOptions.Values.ToList<string>(), "Choose which mouse buttons rotate and pan the camera");
        ShunDialogHelper.MoveToTab(rightClickField, configTabs, "Interface");

        var footer = ShunDialogHelper.AddDialogFooter(() =>
        {
            BackToNeutral();
            dialog.Close();
        });

        var confirm = new ShunDialogClose();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Save Config";
        confirm.clicked += () =>
        {
            SaveConfig();
            // BackToNeutral();
            dialog.Close();
        };
        footer.Add(confirm);

        dialog.Open();
    }

    private static void SaveConfig()
    {
        var dialogContent = ShunDialogHelper.Contents("ShunDialog1");
        Preferences.Current.DataPath = dialogContent.Q<ShunInput>("DataPath").value;
        Preferences.Current.ShowHUD = dialogContent.Q<ShunSwitch>("ShowHUD").value;
        Preferences.Current.UIScale = dialogContent.Q<ShunSelect>("UIScale").selectedValue;
        Preferences.Current.WorldUIScale = dialogContent.Q<ShunSelect>("WUIScale").selectedValue;
        Preferences.Current.DragPan = dialogContent.Q<ShunSelect>("CameraControls").selectedValue == "Rotate with Right, Pan with Middle";

        Preferences.Current.BlockBorderOpacity = dialogContent.Q<ShunSlider>("BlockBorder").value;

        Preferences.Current.TokenOutline = dialogContent.Q<ShunSelect>("ActorBorder").selectedValue;
        Actor.SetAllTokenOutlines();

        float uiValue = float.Parse(Preferences.Current.UIScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/SystemUI").GetComponent<UIDocument>().panelSettings.scale = uiValue;

        float wuiValue = float.Parse(Preferences.Current.WorldUIScale.Replace("%", "")) / 100f;
        GameObject.Find("UICanvas/WorldUI").GetComponent<UIDocument>().panelSettings.scale = wuiValue;

        int fpsValue = int.Parse(ShunDialogHelper.GetToggleFieldValues(dialogContent.Q<ShunToggleGroup>("FPSLimit")).First());
        fpsValue = Math.Max(fpsValue, 3);
        Preferences.Current.TargetFramerate = fpsValue;
        Application.targetFrameRate = fpsValue;

        Preferences.Save();

        ShunSonner.Toast(
            message: "Your changes have been saved",
            title: "Success",
            variant: ToastVariant.Success,
            position: ToastPosition.BottomCenter,
            duration: 300000f
        );
    }

    private static void BackToNeutral()
    {
        // Debug.Log("BTN");
        // StateManager.Find().ChangeSubState(new NeutralState());
    }
}
