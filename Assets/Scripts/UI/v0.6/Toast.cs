using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ToastType {
    Standard,
    Error,
    Success
}

public class Toast : MonoBehaviour
{
    public string Message = "";
    private float duration = 5;
    private Label label;

    // Start is called before the first frame update
    void Start()
    {
        label = new Label();
        label.style.backgroundColor = ColorUtility.ColorFromHex("#223A76");
        label.style.paddingBottom = 4;
        label.style.paddingTop = 4;
        label.style.paddingLeft = 8;
        label.style.paddingRight = 8;
        label.style.borderTopWidth = 2;
        label.style.borderBottomWidth = 2;
        label.style.borderLeftWidth = 2;
        label.style.borderRightWidth = 2;
        label.style.borderTopColor = Color.white;
        label.style.borderBottomColor = Color.white;
        label.style.borderLeftColor = Color.white;
        label.style.borderRightColor = Color.white;
        label.text = Message;     
        UI.System.Q("Toasts").Add(label);
        UI.System.Q("Toasts").style.display = DisplayStyle.Flex;
    }

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0) {
            UI.System.Q("Toasts").Remove(label);
            if (UI.System.Q("Toasts").childCount == 0) {
                UI.System.Q("Toasts").style.display = DisplayStyle.None;
            }
            Destroy(this);
        }
    }

    public static void Add(string message, ToastType type = ToastType.Standard) {
        FileLogger.Write(message);
        Toast t = GameObject.Find("Engine").AddComponent<Toast>();
        t.Message = message;
        // switch(type) {
        //     case ToastType.Error:
        //         t
        //         break;
        // }
    }
}
