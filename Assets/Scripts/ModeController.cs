using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum Mode {
    View,
    Alter,
    Other
}

public class ModeController : MonoBehaviour
{
    public Mode CurrentMode;
    public AlterOption CurrentAlter = AlterOption.SET_SHAPE_SOLID;

    // Start is called before the first frame update
    void Start()
    {
        RegisterCallbacks();
        CurrentMode = Mode.Other;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void RegisterCallbacks() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();

        (modeUI.rootVisualElement.Q("ViewMode") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.View;
            activateButton("ViewMode");
            activateFlyout("CameraFlyout");
            Block.ToggleSpacers(false);
            UI.SetHelpText("Click on any tile to focus the camera on it.");
        });

        (modeUI.rootVisualElement.Q("AlterMode") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Alter;
            activateButton("AlterMode");
            activateFlyout("AlterFlyout");
            Block.ToggleSpacers(true);
            UI.SetHelpText("Choose a change operation and click a tile to make the selected change.");
        });

        (modeUI.rootVisualElement.Q("AddToken") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            activateButton("AddToken");
            activateFlyout();
            Block.ToggleSpacers(false);
            UI.SetHelpText("This mode is not yet implemented.");
        });

        (modeUI.rootVisualElement.Q("Appearance") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            activateButton("Appearance");
            activateFlyout("AppearanceFlyout");
            Block.ToggleSpacers(false);
            UI.SetHelpText("Change the appearance of the map or background.");
        });
        
        (modeUI.rootVisualElement.Q("Config") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            activateButton("Config");
            activateFlyout("ConfigFlyout");
            Block.ToggleSpacers(false);
            UI.SetHelpText("Scale the UI or world markers.");
        });

        (modeUI.rootVisualElement.Q("AlterOptionField") as EnumField).RegisterValueChangedCallback((evt) => {
            CurrentAlter = (AlterOption)evt.newValue;
        });

        (modeUI.rootVisualElement.Q("UIScaleSlider") as Slider).RegisterValueChangedCallback((evt) => {
            UI.SetScale(evt.newValue);
        });

        (modeUI.rootVisualElement.Q("BackgroundEnum") as EnumField).RegisterValueChangedCallback((evt) => {
            Environment.ChangeBackground((Background)evt.newValue);
        });
    }

    public static Mode GetMode() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentMode;
    }

    public static AlterOption GetAlterOption() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentAlter;
    }

    private void activateButton(string name) {
        UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        String[] buttons = new String[]{"ViewMode", "AlterMode", "AddToken", "Appearance", "Config"};
        for (int i = 0; i < buttons.Length; i++) {
            (doc.rootVisualElement.Q(buttons[i]) as Button).RemoveFromClassList("active");
        }
        (doc.rootVisualElement.Q(name) as Button).AddToClassList("active");
    }

    private void activateFlyout(string name = null) {
        UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        String[] buttons = new String[]{"AlterFlyout", "AppearanceFlyout", "ConfigFlyout", "CameraFlyout"};
        for (int i = 0; i < buttons.Length; i++) {
            (doc.rootVisualElement.Q(buttons[i]) as VisualElement).RemoveFromClassList("active");
        }
        if (name != null) {
            (doc.rootVisualElement.Q(name) as VisualElement).AddToClassList("active");
        }
    }
}
