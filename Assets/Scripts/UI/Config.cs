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
        ShunDialogHelper.SetTargetDialog("ShunDialog1");
        var dialog = ShunDialogHelper.Dialog;
        // ShunDialogHelper.SetCloseAction(BackToNeutral);
        ShunDialogHelper.Contents.Clear();

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
        var results = ShunDialogHelper.Results("ShunDialog1");
        Preferences.Current.DataPath = results.Q<ShunInput>("DataPath").value;
        Preferences.Current.ShowHUD = results.Q<ShunSwitch>("ShowHUD").value;
        Preferences.Current.UIScale = results.Q<ShunSelect>("UIScale").selectedValue;
        Preferences.Current.WorldUIScale = results.Q<ShunSelect>("WUIScale").selectedValue;
        Preferences.Current.TargetFramerate = int.Parse(ShunDialogHelper.GetToggleFieldValues(results.Q<ShunToggleGroup>("FPSLimit")).First());
        Preferences.Current.BlockBorderOpacity = results.Q<ShunSlider>("BlockBorder").value;
        Preferences.Current.TokenOutline = results.Q<ShunSelect>("ActorBorder").selectedValue;
        Preferences.Current.DragPan = results.Q<ShunSelect>("CameraControls").selectedValue == "Rotate with Right, Pan with Middle";

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
