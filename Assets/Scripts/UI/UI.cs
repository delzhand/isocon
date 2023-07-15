using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public enum HelpType {
    Standard,
    Error,
    Success
}

public class UI : MonoBehaviour
{
    private static VisualElement gameUI;
    private static VisualElement systemUI;

    private static List<string> suspensions = new List<string>();

    void Start() {
    }

    public static bool ClicksSuspended {
        get {
            return suspensions.Count > 0;
        }
    }

    public static VisualElement GameInfo {
        get { 
            if (gameUI == null) {
                gameUI = GameObject.Find("WorldCanvas/TokenUI").GetComponent<UIDocument>().rootVisualElement;
            }
            return gameUI;
        }
    }

    public static VisualElement System {
        get {
            if (systemUI == null) {
                systemUI = GameObject.Find("ModeUI").GetComponent<UIDocument>().rootVisualElement;
            }
            return systemUI;
        }
    }

    public static void AttachHelp(VisualElement root, string query, string text) {
        root.Q(query).RegisterCallback<MouseOverEvent>((evt) => {
            UI.SetHelpText(text);
        });
        root.Q(query).RegisterCallback<MouseOutEvent>((evt) => {
            UI.SetHelpText("");
        });
    }

    public static void DisableHelp() {
        System.Q("HelpBar").style.display = DisplayStyle.None;
        PlayerPrefs.SetInt("ShowHelp", 0);
    }

    public static void EnableHelp() {
        System.Q("HelpBar").style.display = DisplayStyle.Flex; 
        PlayerPrefs.SetInt("ShowHelp", 1);
    }



    public static void SetBlocking(VisualElement root, string[] blockingElements) {
        foreach(string s in blockingElements) {
            root.Q(s).RegisterCallback<MouseEnterEvent>((evt) => {
                if (!suspensions.Contains(s)) {
                    suspensions.Add(s);
                    // printSuspensions("Enter " + s);
                }
            });            
            root.Q(s).RegisterCallback<MouseLeaveEvent>((evt) => {
                suspensions.Remove(s);
                // printSuspensions("Leave " + s);
            });
        }
    }

    private static void printSuspensions(string s2) {
        Debug.Log(s2);
        StringBuilder sb = new StringBuilder();
        foreach(string s in suspensions) {
            sb.Append(s + " / ");
        }
        Debug.Log(sb.ToString());
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
