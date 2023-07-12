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

public class ModeController : MonoBehaviour
{
    public static ClickMode ClickMode;
    public static ClickMode ReserveClickMode;

    private VisualElement root;
    private List<VisualElement> elements = new List<VisualElement>();

    // Start is called before the first frame update
    void Start()
    {        
        root = GameObject.Find("ModeUI").GetComponent<UIDocument>().rootVisualElement;
        setup();
        UI.SetBlocking(root);
        enterPlayMode(null);
    }

    private void setup() {
        List<(string, string, EventCallback<ClickEvent>)> uiConfig = new List<(string, string, EventCallback<ClickEvent>)>{
            ("Play", "Play ICON", enterPlayMode),
            ("EditMap", "Change the terrain", enterEditMapMode),
            ("Config", "Modify system configuration", enterConfigMode),
            ("File", "Save, load, or exit the program", enterFileMode),
            ("Info", "View information about the program", enterInfoMode),
            ("Appearance", "Change the background or tile colors", enterAppearanceMode)
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
        GameObject.Find("ReserveCamera").GetComponent<Camera>().enabled = false;
    }

    public void ActivateElement(VisualElement v) {
        elements.Add(v);
        v.AddToClassList("active");
        if (v.ClassListContains("modal")) {
            v.style.display = DisplayStyle.Flex;
        }

    }

    public void ActivateElementByName(string name) {
        VisualElement v = root.Q(name);
        ActivateElement(v);
    }

    public void DeactivateElement(VisualElement v) {
        v.RemoveFromClassList("active");
        if (v.ClassListContains("modal")) {
            v.style.display = DisplayStyle.None;
        }
    }

    public void DeactivateByName(string name) {
        VisualElement v = root.Q(name);
        DeactivateElement(v);
    }

    private void enterPlayMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("Play");
        GameObject.Find("TokenUI").GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
        GameObject.Find("ReserveCamera").GetComponent<Camera>().enabled = true;
        // Because this can only happen when world clicks are suspended, we set reserveclickmode
        ReserveClickMode = ClickMode.Play;
    }

    private void enterEditMapMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("EditMap");
        ActivateElementByName("EditMapFlyout");
        Block.ToggleSpacers(true);
        // Because this can only happen when world clicks are suspended, we set reserveclickmode
        ReserveClickMode = ClickMode.Edit;
    }

    private void enterAppearanceMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("Appearance");
        ActivateElementByName("AppearanceFlyout");
    }

    private void enterConfigMode(ClickEvent evt) {
        clearState();
        ActivateElementByName("Config");
        ActivateElementByName("ConfigFlyout");
        GameObject.Find("TokenUI").GetComponent<UIDocument>().rootVisualElement.style.display = DisplayStyle.Flex;
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
}
