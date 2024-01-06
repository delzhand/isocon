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
    private static string ActiveItem;

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

    public static void Hide() {
        VisualElement menu = Find();
        UI.ToggleDisplay(menu, false);
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

    public static void ActivateItem(string name) {
        UI.ToggleDisplay("CurrentOp", true);
        UI.System.Q("CurrentOp").Q<Label>("Op").text = $"{Token.GetSelected().Data.Name}: {name}"; 
        ActiveItem = name;
        SelectionMenu.Hide();
        // SelectionMenu.Find().Q(ActiveItem).AddToClassList("active");
    }

    public static void DeactivateItem() {
        VisualElement e = SelectionMenu.Find().Q(ActiveItem);
        if (e != null) {
            e.RemoveFromClassList("active");
        }
        ActiveItem = "";
    }

    public static bool IsActive(string name) {
        return ActiveItem == name;
    }
}
