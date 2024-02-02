using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ToastType
{
    Standard,
    Error,
    Success,
    Custom
}

public class Toast : MonoBehaviour
{
    private static GameObject obj;

    public string Message = "";
    private float duration = 5;
    private VisualElement element;
    private ToastType type;

    // Start is called before the first frame update
    void Start()
    {
        if (type != ToastType.Custom)
        {
            element = new Label();
            element.style.paddingBottom = 4;
            element.style.paddingTop = 4;
            element.style.paddingLeft = 8;
            element.style.paddingRight = 8;
            element.style.borderTopWidth = 2;
            element.style.borderBottomWidth = 2;
            element.style.borderLeftWidth = 2;
            element.style.borderRightWidth = 2;
            element.style.borderTopColor = Color.white;
            element.style.borderBottomColor = Color.white;
            element.style.borderLeftColor = Color.white;
            element.style.borderRightColor = Color.white;
            (element as Label).text = Message;
            switch (type)
            {
                case ToastType.Standard:
                    element.style.backgroundColor = ColorUtility.UIBlue;
                    break;
                case ToastType.Error:
                    element.style.backgroundColor = ColorUtility.UIErrorRed;
                    break;
                case ToastType.Success:
                    element.style.backgroundColor = ColorUtility.UISuccessGreen;
                    break;
            }
        }
        UI.System.Q("Toasts").Add(element);
        UI.System.Q("Toasts").style.display = DisplayStyle.Flex;
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            UI.System.Q("Toasts").Remove(element);
            if (UI.System.Q("Toasts").childCount == 0)
            {
                UI.System.Q("Toasts").style.display = DisplayStyle.None;
            }
            Destroy(this);
        }
    }

    private static void Add(string message, ToastType type)
    {
        FileLogger.Write(message);
        Toast t = GetAttachmentObject().AddComponent<Toast>();
        t.Message = message;
        t.type = type;
    }

    public static void AddSimple(string message)
    {
        Add(message, ToastType.Standard);
    }

    public static void AddSuccess(string message)
    {
        Add(message, ToastType.Success);
    }

    public static void AddError(string message)
    {
        Add(message, ToastType.Error);
    }

    public static void AddCustom(VisualElement v, float duration = 5)
    {
        Toast t = GetAttachmentObject().AddComponent<Toast>();
        t.type = ToastType.Custom;
        t.element = v;
        t.duration = duration;
    }

    private static GameObject GetAttachmentObject()
    {
        if (obj == null)
        {
            obj = GameObject.Find("AppState");
        }
        return obj;
    }
}
