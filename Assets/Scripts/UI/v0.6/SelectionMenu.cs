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
    public static void Setup() {
        UI.SetBlocking(UI.System, "SelectionMenu");
    }

    public static VisualElement Find() {
        return UI.System.Q("SelectionMenu");
    }

    public static void Reset(string title) {
        VisualElement menu = Find();
        UI.ToggleDisplay(menu, true);
        menu.Q<Label>("Label").text = title;
        menu.Q("Contents").Clear();
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
