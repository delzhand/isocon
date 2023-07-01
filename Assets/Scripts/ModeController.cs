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

public enum ElementType {
    Modal,
    Flyout,
    Button
}

public class ModeController : MonoBehaviour
{
    public Mode CurrentMode;
    public AlterOption CurrentAlter = AlterOption.SET_SHAPE_SOLID;

    private string[] activeButtons;
    private string[] flyouts;
    private string[] modals;

    // Start is called before the first frame update
    void Start()
    {
        Environment.SetBackground(Background.FANTASIA);
        activeButtons = new String[]{"ViewMode", "AlterMode", "AddToken", "Appearance", "Config", "Data"};
        flyouts = new String[]{"AlterFlyout", "AppearanceFlyout", "ConfigFlyout", "CameraFlyout", "DataFlyout"};
        modals = new String[]{"LoadFileModal", "FilenameModal", "LoadConfirmModal", "InfoModal"};
        RegisterCallbacks();
        toggleElement(ElementType.Modal, null);
        toggleElement(ElementType.Button, "ViewMode");
        toggleElement(ElementType.Flyout, "CameraFlyout");
        Block.ToggleSpacers(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void RegisterCallbacks() {
        UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();

        (modeUI.rootVisualElement.Q("ViewMode") as Button).RegisterCallback<ClickEvent>((evt) => {

            CurrentMode = Mode.View;
            toggleElement(ElementType.Button, "ViewMode");
            toggleElement(ElementType.Flyout, "CameraFlyout");
            Block.ToggleSpacers(false);
        });
        UI.AttachHelp(modeUI, "ViewMode", "Click on any tile to focus the camera on it.");

        (modeUI.rootVisualElement.Q("AlterMode") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Alter;
            toggleElement(ElementType.Button, "AlterMode");
            toggleElement(ElementType.Flyout, "AlterFlyout");
            Block.ToggleSpacers(true);
        });
        UI.AttachHelp(modeUI, "AlterMode", "Edit terrain by clicking on tiles.");

        (modeUI.rootVisualElement.Q("AddToken") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            toggleElement(ElementType.Button, "AddToken");
            toggleElement(ElementType.Flyout, "TokenFlyout");
            Block.ToggleSpacers(false);
        });
        UI.AttachHelp(modeUI, "AddToken", "Add objects and units to the field.");

        (modeUI.rootVisualElement.Q("Appearance") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            toggleElement(ElementType.Button, "Appearance");
            toggleElement(ElementType.Flyout, "AppearanceFlyout");
            Block.ToggleSpacers(false);
        });
        UI.AttachHelp(modeUI, "Appearance", "Change the appearance of the map or background.");
        
        (modeUI.rootVisualElement.Q("Config") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            toggleElement(ElementType.Button, "Config");
            toggleElement(ElementType.Flyout, "ConfigFlyout");
            Block.ToggleSpacers(false);
        });
        UI.AttachHelp(modeUI, "Config", "Scale the UI or world markers.");

        (modeUI.rootVisualElement.Q("AlterOptionField") as EnumField).RegisterValueChangedCallback((evt) => {
            CurrentAlter = (AlterOption)evt.newValue;
        });
        UI.AttachHelp(modeUI, "AlterOptionField", "Change what happens when a tile is clicked.");


        (modeUI.rootVisualElement.Q("UIScaleSlider") as Slider).RegisterValueChangedCallback((evt) => {
            UI.SetScale("UICanvas/ModeUI", evt.newValue);
        });
        UI.AttachHelp(modeUI, "UIScaleSlider", "Make the general UI larger or smaller.");

        (modeUI.rootVisualElement.Q("InfoScaleSlider") as Slider).RegisterValueChangedCallback((evt) => {
            UI.SetScale("WorldCanvas/TokenUI", evt.newValue);
        });
        UI.AttachHelp(modeUI, "InfoScaleSlider", "Make the battle information larger or smaller.");


        (modeUI.rootVisualElement.Q("BackgroundEnum") as EnumField).RegisterValueChangedCallback((evt) => {
            Environment.SetBackground((Background)evt.newValue);
        });
        UI.AttachHelp(modeUI, "BackgroundEnum", "Change the background gradient to match the mood or setting of the battle.");

        (modeUI.rootVisualElement.Q("PaletteEnum") as EnumField).RegisterValueChangedCallback((evt) => {
            Environment.SetPalette((Palette)evt.newValue);
        });
        UI.AttachHelp(modeUI, "BackgroundEnum", "Change the background gradient to match the mood or setting of the battle.");


        (modeUI.rootVisualElement.Q("Data") as Button).RegisterCallback<ClickEvent>((evt) => {
            CurrentMode = Mode.Other;
            toggleElement(ElementType.Button, "Data");
            toggleElement(ElementType.Flyout, "DataFlyout");
            Block.ToggleSpacers(false);
        });
        UI.AttachHelp(modeUI, "Data", "Save or load maps.");

        (modeUI.rootVisualElement.Q("SaveMapButton") as Button).RegisterCallback<ClickEvent>((evt) => {
            DataController.currentOp = FileOp.Save;
            if (DataController.currentFileName != null && DataController.currentFileName.Length > 0) {
                State.SaveState(DataController.currentFileName);
                toggleElement(ElementType.Modal, null);
                toggleElement(ElementType.Flyout, null);
            }
            else {
                toggleElement(ElementType.Modal, "FilenameModal");
            }
        });

        (modeUI.rootVisualElement.Q("SaveMapAsButton") as Button).RegisterCallback<ClickEvent>((evt) => {
            DataController.currentOp = FileOp.Save;
            toggleElement(ElementType.Modal, "FilenameModal");
        });

        (modeUI.rootVisualElement.Q("LoadButton") as Button).RegisterCallback<ClickEvent>((evt) => {
            DataController.InitializeFileList();
            toggleElement(ElementType.Modal, "LoadFileModal");
        });

        (modeUI.rootVisualElement.Q("FilenameCancel") as Button).RegisterCallback<ClickEvent>((evt) => {
            toggleElement(ElementType.Modal, null);
        });

        (modeUI.rootVisualElement.Q("FilenameConfirm") as Button).RegisterCallback<ClickEvent>((evt) => {
            string value = (modeUI.rootVisualElement.Q("Filename") as TextField).value;
            if (value.Length > 0) {
                DataController.currentFileName = value;
                State.SaveState(value);
                toggleElement(ElementType.Modal, null);
            }
            else {
                UI.SetHelpText("Filename cannot be empty.", HelpType.Error);
            }
        });

        
        (modeUI.rootVisualElement.Q("FileSelectCancel") as Button).RegisterCallback<ClickEvent>((evt) => {
            toggleElement(ElementType.Modal, null);
        });

        (modeUI.rootVisualElement.Q("ResetButton") as Button).RegisterCallback<ClickEvent>((evt) => {
            DataController.currentFileName = null;
            TerrainEngine.ResetTerrain();
        });

        modeUI.rootVisualElement.Q("Info").RegisterCallback<ClickEvent>((evt) => {
            toggleElement(ElementType.Modal, "InfoModal");
        });

        (modeUI.rootVisualElement.Q("InfoClose") as Button).RegisterCallback<ClickEvent>((evt) => {
            toggleElement(ElementType.Modal, null);
        });

        (modeUI.rootVisualElement.Q("AddObjectButton") as Button).RegisterCallback<ClickEvent>((evt) => {
        });
        UI.AttachHelp(modeUI, "AddObjectButton", "Create a generic object.");

        (modeUI.rootVisualElement.Q("AddUnitButton") as Button).RegisterCallback<ClickEvent>((evt) => {
        });
        UI.AttachHelp(modeUI, "AddUnitButton", "Create a player, npc, or foe.");


    }

    public static Mode GetMode() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentMode;
    }

    public static AlterOption GetAlterOption() {
        return GameObject.Find("Mode").GetComponent<ModeController>().CurrentAlter;
    }

    public static void CloseModal(string name) {
        UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        (doc.rootVisualElement.Q(name) as VisualElement).RemoveFromClassList("active");
        (doc.rootVisualElement.Q(name) as VisualElement).SetEnabled(false);
    }

    private void toggleElement(ElementType type, string name = null) {
        UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();
        string[] elements = new string[]{};
        switch(type) {
            case ElementType.Flyout:
                elements = flyouts;
                break;
            case ElementType.Modal:
                elements = modals;
                break;
            case ElementType.Button:
                elements = activeButtons;
                break;
        }
        // Toggle all off
        // Disable modals because they don't get moved offscreen entirely by uss and can still be clicked otherwise
        for (int i = 0; i < elements.Length; i++) {
            doc.rootVisualElement.Q(elements[i]).RemoveFromClassList("active");
            if (type == ElementType.Modal) {
                doc.rootVisualElement.Q(elements[i]).SetEnabled(false);
            }
        }
        // Toggle specific element on
        if (name != null) {
            doc.rootVisualElement.Q(name).AddToClassList("active");
            if (type == ElementType.Modal) {
                doc.rootVisualElement.Q(name).SetEnabled(true);
            }
        }
    }
}
