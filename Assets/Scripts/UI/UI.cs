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
        UI.SetScale(PlayerPrefs.GetFloat("UIScale"));
    }

    public static bool IsPointerOverUI(Vector2 screenPos) {
        GameObject uiObject = GameObject.Find("ModeUI");
        if (uiObject == null) {
            return false;
        }
        UIDocument ui = uiObject.GetComponent<UIDocument>();
        Vector2 pointerUiPos = new Vector2{ x = screenPos.x , y = Screen.height - screenPos.y };
        List<VisualElement> picked = new List<VisualElement>();
        ui.rootVisualElement.panel.PickAll( pointerUiPos , picked );
        //Debug.Log(picked.Count);
        foreach( var ve in picked )
        if( ve!=null )
        {
            Color32 bcol = ve.resolvedStyle.backgroundColor;
            if( bcol.a!=0 && ve.enabledInHierarchy )
                //Debug.Log("ui clicked");
                return true;
        }
        return false;        
    }

    public static void AddHover(UIDocument root, string query) {
        root.rootVisualElement.Q(query).RegisterCallback<MouseOverEvent>((evt) => {
            root.rootVisualElement.Q(query).AddToClassList("hover");
        });
        root.rootVisualElement.Q(query).RegisterCallback<MouseOutEvent>((evt) => {
            root.rootVisualElement.Q(query).RemoveFromClassList("hover");
        });
    }

    public static void AttachHelp(UIDocument root, string query, string text) {
        root.rootVisualElement.Q(query).RegisterCallback<MouseOverEvent>((evt) => {
            UI.SetHelpText(text);
        });
        root.rootVisualElement.Q(query).RegisterCallback<MouseOutEvent>((evt) => {
            UI.SetHelpText("");
        });
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
                helpbar.style.backgroundColor = new Color(1, 0, 0, .3f);
                break;
            case HelpType.Success:
                helpbar.style.backgroundColor = new Color(0, 1, 0, .3f);
                break;
        }
    }

    public static void Log(string message) {
        Label output = GameObject.Find("UICanvas/DebugUI").GetComponent<UIDocument>().rootVisualElement.Q("Output") as Label;
        output.text = message;
    }

    public static void SetScale(float value) {
        GameObject.Find("UICanvas/ModeUI").GetComponent<UIDocument>().panelSettings.scale = value;
    }

    public static float GetScale() {
        return GameObject.Find("UICanvas/ModeUI").GetComponent<UIDocument>().panelSettings.scale;
    }
}
