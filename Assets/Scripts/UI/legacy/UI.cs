using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class UI : MonoBehaviour
{
    private static VisualElement systemUI;

    private static List<string> suspensions = new List<string>();
    private static bool hardSuspend = false;
    
    void Start() {
    }

    void Update() {
        List<VisualElement> dropdowns = UI.System.parent.Query(null, "unity-base-dropdown__container-outer").ToList();
        if (dropdowns.Count > 0) {
            hardSuspend = true;
        }
        else  {
            hardSuspend = false;
        }
    }

    public static bool ClicksSuspended {
        get {
            return suspensions.Count > 0 || hardSuspend;
        }
    }

    public static VisualElement System {
        get {
            if (systemUI == null) {
                systemUI = GameObject.Find("SystemUI").GetComponent<UIDocument>().rootVisualElement;
            }
            return systemUI;
        }
    }

    public static void SetBlocking(VisualElement root, string blockingElement) {
        SetBlocking(root, new string[]{blockingElement});
    }

    public static void SetBlocking(VisualElement root, string[] blockingElements) {
        foreach(string s in blockingElements) {
            root.Q(s).RegisterCallback<MouseEnterEvent>((evt) => {
                if (!suspensions.Contains(s)) {
                    suspensions.Add(s);
                }
            });            
            root.Q(s).RegisterCallback<MouseLeaveEvent>((evt) => {
                suspensions.Remove(s);
            });
        }
    }

    // public static void SetHardSuspend(bool val) {
    //     hardSuspend = val;
    // }

    public static void SetScale(string element, float value) {
        GameObject.Find(element).GetComponent<UIDocument>().panelSettings.scale = value;
    }

    public static void FollowToken(Token token, VisualElement element, Camera camera, Vector2 offset, bool useAnchor = true) {
        Vector3 worldPos = token.transform.Find("Offset/Avatar/Cutout/Cutout Quad").position;
        if (useAnchor) {
            worldPos = token.transform.Find("Offset/Avatar/Cutout/Cutout Quad/LabelAnchor").position;
        }
        Vector3 viewportPos = camera.WorldToViewportPoint(worldPos);
        if (element.resolvedStyle.width != float.NaN) {
            Vector2 screenPos = new Vector2(
                Mathf.RoundToInt((viewportPos.x * UI.System.resolvedStyle.width)),
                Mathf.RoundToInt((1f - viewportPos.y) * UI.System.resolvedStyle.height)
            );
            Vector2 pos = screenPos + offset;
            element.style.position = Position.Absolute;
            element.style.left = pos.x;
            element.style.top = pos.y;
        }
    }

    public static bool InElement(string elementName) {
        VisualElement v = UI.System.Q(elementName);
        if (v == null) {
            return false;
        }
        Vector2 mp = Input.mousePosition;
        Vector2 min = UI.System.Q(elementName).layout.min;
        Vector2 max = UI.System.Q(elementName).layout.max;
        return (mp.x >= min.x && mp.x <= max.x && mp.y >= min.y && mp.y <= max.y);   
    }

    public static void ToggleDisplay(string name) {
        bool isShown = (System.Q(name).resolvedStyle.display != DisplayStyle.None);
        ToggleDisplay(name, !isShown);
    }

    public static void ToggleDisplay(string name, bool shown) {
        try {
            ToggleDisplay(System.Q(name), shown);
        }
        catch (Exception e) {
            Debug.Log(e.Message);
            throw new Exception($"Could not find element {name}");
        }
    }

    public static void ToggleDisplay(VisualElement e, bool shown) {
        if (shown) {
            e.style.display = DisplayStyle.Flex;
        }
        else {
            e.style.display = DisplayStyle.None;
        }
    }

    public static void ToggleHidden(string name) {
        bool isHidden = System.Q(name).GetClasses().ToArray().Contains<string>("hidden");
        if (isHidden) {
            ToggleHidden(name, false);
        }
        else {
            ToggleHidden(name, true);
        }
    }
    public static void ToggleHidden(string name, bool on) {
        if (on) {
            System.Q(name).AddToClassList("hidden");
        }
        else {
            System.Q(name).RemoveFromClassList("hidden");
        }
    }

    public static void ToggleActiveClass(string name, bool active) {
        ToggleActiveClass(UI.System.Q(name), active);
    }

    public static void ToggleActiveClass(VisualElement e, bool active) {
        if (active) {
            e.AddToClassList("active");
        }
        else {
            e.RemoveFromClassList("active");
        }
    }

    public static void HoverSetup(VisualElement e) {
        e.RegisterCallback<MouseEnterEvent>((evt) => {
            e.AddToClassList("hover");
        });
        e.RegisterCallback<MouseLeaveEvent>((evt) => {
            e.RemoveFromClassList("hover");
        });
    }
}
