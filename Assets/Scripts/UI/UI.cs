using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class UI : MonoBehaviour
{
    private static VisualElement systemUI;

    private static List<string> suspensions = new List<string>();
    private static bool hardSuspend = false;

    void Start() {
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

    public static void SetHardSuspend(bool val) {
        hardSuspend = val;
    }
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
            element.style.left = pos.x;
            element.style.top = pos.y;
        }
    }

    public static bool InElement(string elementName) {
        Vector2 mp = Input.mousePosition;
        Vector2 min = UI.System.Q(elementName).layout.min;
        Vector2 max = UI.System.Q(elementName).layout.max;
        return (mp.x >= min.x && mp.x <= max.x && mp.y >= min.y && mp.y <= max.y);   
    }

    public static void ToggleDisplay(string name, bool shown) {
        ToggleDisplay(System.Q(name), shown);
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
}
