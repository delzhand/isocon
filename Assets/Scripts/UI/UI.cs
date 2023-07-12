using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum HelpType {
    Standard,
    Error,
    Success
}

public class UI : MonoBehaviour
{
    void Start() {
    }

    public static void AttachHelp(VisualElement root, string query, string text) {
        root.Q(query).RegisterCallback<MouseOverEvent>((evt) => {
            UI.SetHelpText(text);
        });
        root.Q(query).RegisterCallback<MouseOutEvent>((evt) => {
            UI.SetHelpText("");
        });
    }

    public static void SetBlocking(VisualElement root) {
        string[] blockingElements = new string[]{"ModeControls", "HelpBar", "EditMapFlyout", "RotateCCW", "RotateCW", "ZoomSlider", "OverheadToggle"};
        foreach(string s in blockingElements) {
            root.Q(s).RegisterCallback<MouseOverEvent>((evt) => {
                ModeController.ReserveClickMode = ModeController.ClickMode;
                ModeController.ClickMode = ClickMode.Other;
            });
            root.Q(s).RegisterCallback<MouseOutEvent>((evt) => {
                ModeController.ClickMode = ModeController.ReserveClickMode;
            });
        }
    }

    public static void SetHelpText(string message, HelpType type = HelpType.Standard) {
        VisualElement helpbar = GameObject.Find("UICanvas/ModeUI").GetComponent<UIDocument>().rootVisualElement.Q("HelpBar");
        Label helptext = GameObject.Find("UICanvas/ModeUI").GetComponent<UIDocument>().rootVisualElement.Q("HelpText") as Label;
        helptext.text = message;
        switch(type) {
            case HelpType.Standard:
                helpbar.style.backgroundColor = new Color(0, 0, 0, .3f);
                break;
            case HelpType.Error:
                helpbar.style.backgroundColor = new Color(.8f, 0, 0, .3f);
                break;
            case HelpType.Success:
                helpbar.style.backgroundColor = new Color(0, .8f, 0, .3f);
                break;
        }
    }

    public static void Log(string message) {
        Label output = GameObject.Find("UICanvas/DebugUI").GetComponent<UIDocument>().rootVisualElement.Q("Output") as Label;
        output.text = message;
    }

    public static void SetScale(string element, float value) {
        GameObject.Find(element).GetComponent<UIDocument>().panelSettings.scale = value;
    }

    public static float GetScale(string element) {
        return GameObject.Find(element).GetComponent<UIDocument>().panelSettings.scale;
    }
}
