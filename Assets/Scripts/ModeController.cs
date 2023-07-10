using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ClickMode {
    Play,
    Edit,
    Other
}

public enum ElementType {
    Modal,
    Flyout,
    Button
}

public class ModeController : MonoBehaviour
{
    public static ClickMode ClickMode;
    public AlterOption CurrentAlter = AlterOption.SET_SHAPE_SOLID;

    // private string[] activeButtons;
    // private string[] flyouts;
    // private string[] modals;

    private VisualElement root;
    private List<VisualElement> elements = new List<VisualElement>();

    // Start is called before the first frame update
    void Start()
    {        
        root = GameObject.Find("ModeUI").GetComponent<UIDocument>().rootVisualElement;
        setup();
        enterPlayMode(null);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void setup() {
        List<(string, string, EventCallback<ClickEvent>)> uiConfig = new List<(string, string, EventCallback<ClickEvent>)>{
            ("Play", "Play ICON", enterPlayMode),
            ("EditMap", "Change the terrain or environment", enterEditMapMode),
            ("Config", "Modify system configuration", enterConfigMode),
            ("File", "Save, load, or exit the program", enterFileMode),
            ("Info", "View information about the program", enterInfoMode),
        };

        foreach((string, string, EventCallback<ClickEvent>) item in uiConfig) {
            UI.AttachHelp(root, item.Item1, item.Item2);
            root.Q(item.Item1).RegisterCallback<ClickEvent>(item.Item3);
        }
    }

    private void clearState() {
        foreach (VisualElement v in elements) {
            DeactivateElement(v);
        }
        elements.Clear();
        Block.DeselectAll();
        Block.ToggleSpacers(false);
        ClickMode = ClickMode.Other;
        GameObject.Find("TokenUI").GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.None;
    }

    public void ActivateElement(VisualElement v) {
        elements.Add(v);
        v.AddToClassList("active");
    }

    public void ActivateElementByName(string name) {
        VisualElement v = root.Q(name);
        ActivateElement(v);
    }

    public void DeactivateElement(VisualElement v) {
        v.RemoveFromClassList("active");
    }

    public void DeactivateByName(string name) {
        VisualElement v = root.Q(name);
        DeactivateElement(v);
    }

    private void enterPlayMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("Play");
        ClickMode = ClickMode.Play;
        GameObject.Find("TokenUI").GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
    }

    private void enterEditMapMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("EditMap");
        ActivateElementByName("EditMapFlyout");
        Block.ToggleSpacers(true);
        ClickMode = ClickMode.Edit;
    }

    private void enterConfigMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("Config");
        ActivateElementByName("ConfigFlyout");
    }

    private void enterFileMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("File");
        ActivateElementByName("FileFlyout");
    }

    private void enterInfoMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("InfoFlyout");
    }

    private void exitInfoMode(ClickEvent evt) {
        clearState();
    }

    // private void RegisterCallbacks() {

    //     UIDocument modeUI = GameObject.Find("ModeUI").GetComponent<UIDocument>();

    //     (modeUI.rootVisualElement.Q("ViewMode") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         CurrentMode = Mode.View;
    //         toggleElement(ElementType.Button, "ViewMode");
    //         toggleElement(ElementType.Flyout, "CameraFlyout");
    //         toggleElement(ElementType.Modal, null);
    //         Block.ToggleSpacers(false);
    //     });
    //     UI.AttachHelp(modeUI, "ViewMode", "Click on any tile to focus the camera on it.");

    //     (modeUI.rootVisualElement.Q("AlterMode") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         CurrentMode = Mode.Alter;
    //         toggleElement(ElementType.Button, "AlterMode");
    //         toggleElement(ElementType.Flyout, "AlterFlyout");
    //         toggleElement(ElementType.Modal, null);
    //         Block.ToggleSpacers(true);
    //     });
    //     UI.AttachHelp(modeUI, "AlterMode", "Edit terrain by clicking on tiles.");

    //     (modeUI.rootVisualElement.Q("AddToken") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         CurrentMode = Mode.Other;
    //         toggleElement(ElementType.Flyout, null);
    //         toggleElement(ElementType.Button, "AddToken");
    //         toggleElement(ElementType.Modal, "AddTokenModal");
    //         Block.ToggleSpacers(false);
    //     });
    //     UI.AttachHelp(modeUI, "AddToken", "Add objects and units to the field.");

    //     (modeUI.rootVisualElement.Q("Appearance") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         CurrentMode = Mode.Other;
    //         toggleElement(ElementType.Button, "Appearance");
    //         toggleElement(ElementType.Flyout, "AppearanceFlyout");
    //         toggleElement(ElementType.Modal, null);
    //         Block.ToggleSpacers(false);
    //     });
    //     UI.AttachHelp(modeUI, "Appearance", "Change the appearance of the map or background.");
        
    //     (modeUI.rootVisualElement.Q("Config") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         CurrentMode = Mode.Other;
    //         toggleElement(ElementType.Button, "Config");
    //         toggleElement(ElementType.Flyout, "ConfigFlyout");
    //         toggleElement(ElementType.Modal, null);
    //         Block.ToggleSpacers(false);
    //     });
    //     UI.AttachHelp(modeUI, "Config", "Scale the UI or world markers.");

    //     (modeUI.rootVisualElement.Q("AlterOptionField") as EnumField).RegisterValueChangedCallback((evt) => {
    //         CurrentAlter = (AlterOption)evt.newValue;
    //     });
    //     UI.AttachHelp(modeUI, "AlterOptionField", "Change what happens when a tile is clicked.");





    //     modeUI.rootVisualElement.Q("Info").RegisterCallback<ClickEvent>((evt) => {
    //         toggleElement(ElementType.Modal, "InfoModal");
    //     });

    //     (modeUI.rootVisualElement.Q("InfoClose") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         toggleElement(ElementType.Modal, null);
    //     });

    //     (modeUI.rootVisualElement.Q("AddTokenConfirm") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         Reserve.Adjust();
    //         toggleElement(ElementType.Modal, null);
    //         Token.CreateNew();
    //         CurrentMode = Mode.View;
    //         toggleElement(ElementType.Button, "ViewMode");
    //         toggleElement(ElementType.Flyout, "CameraFlyout");
    //         Block.ToggleSpacers(false);
    //     });

    //     (modeUI.rootVisualElement.Q("AddTokenCancel") as Button).RegisterCallback<ClickEvent>((evt) => {
    //         toggleElement(ElementType.Modal, null);
    //     });

    //     Token.InitModal();
    // }

    public static AlterOption GetAlterOption() {
        return GameObject.Find("Engine").GetComponent<ModeController>().CurrentAlter;
    }

    // public static void CloseModal(string name) {
    //     UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();
    //     (doc.rootVisualElement.Q(name) as VisualElement).RemoveFromClassList("active");
    //     (doc.rootVisualElement.Q(name) as VisualElement).SetEnabled(false);
    // }

    // private void toggleElement(ElementType type, string name = null) {
    //     UIDocument doc = GameObject.Find("ModeUI").GetComponent<UIDocument>();
    //     string[] elements = new string[]{};
    //     switch(type) {
    //         case ElementType.Flyout:
    //             elements = flyouts;
    //             break;
    //         case ElementType.Modal:
    //             elements = modals;
    //             break;
    //         case ElementType.Button:
    //             elements = activeButtons;
    //             break;
    //     }
    //     // Toggle all off
    //     // Disable modals because they don't get moved offscreen entirely by uss and can still be clicked otherwise
    //     for (int i = 0; i < elements.Length; i++) {
    //         doc.rootVisualElement.Q(elements[i]).RemoveFromClassList("active");
    //         if (type == ElementType.Modal) {
    //             doc.rootVisualElement.Q(elements[i]).SetEnabled(false);
    //         }
    //     }
    //     // Toggle specific element on
    //     if (name != null) {
    //         doc.rootVisualElement.Q(name).AddToClassList("active");
    //         if (type == ElementType.Modal) {
    //             doc.rootVisualElement.Q(name).SetEnabled(true);
    //         }
    //     }
    // }
}
