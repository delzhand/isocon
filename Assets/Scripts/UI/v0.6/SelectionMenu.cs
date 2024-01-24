using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct MenuItem {
    public string Name;
    public string Label;
    public Action<ClickEvent> OnClick;


    public MenuItem(string name, string label, Action<ClickEvent> onClick) {
        Name = name;
        Label = label;
        OnClick = onClick;
    }
}

public class SelectionMenu
{
    // private static string ActiveItem;
    public static Transform FollowTransform;
    public static bool Visible;

    public static Vector2 Offset;

    public static void Setup() {
        UI.SetBlocking(UI.System, "SelectionMenu");
    }

    public static VisualElement Find() {
        return UI.System.Q("SelectionMenu");
    }

    public static void Reset(string title, Vector2 offset, Transform follow = null) {
        Offset = offset;
        VisualElement menu = Find();
        menu.Q<Label>("Label").text = title;
        menu.Q("Contents").Clear();
        FollowTransform = follow;
        Visible = true;
    }

    public static void Hide() {
        Visible = false;
        FollowTransform = null;
    }

    public static void AddItem(string name, string label, Action<ClickEvent> clickHandler) {
        VisualElement menu = Find();
        VisualElement element = UI.CreateFromTemplate("UITemplates/MenuItem");
        element.Q<Label>("Label").text = label;
        element.name = name;
        element.RegisterCallback<ClickEvent>((evt) => {
            clickHandler.Invoke(evt);
        });
        UI.HoverSetup(element);
        menu.Q("Contents").Add(element);
    }
}
