using System;
using System.Collections;
using System.Collections.Generic;
using ShunUI;
using ShunUI.Primitives;
using UnityEngine;
using UnityEngine.UIElements;

public struct MenuItem
{
    public string Label;
    public Action Action;
    public List<MenuItem> Children;

    public MenuItem(string label, Action onClick)
    {
        Label = label;
        Action = onClick;
        Children = new();
    }
}

public class SelectionMenu
{
    // private static string ActiveItem;
    public static Transform FollowTransform;
    public static bool Visible;

    public static Vector2 Offset;

    // private static ShunContextMenu CMenu;

    // public static void Setup()
    // {
    //     UI.SetBlocking(UI.System, "SelectionMenu");

    //     // VisualElement parent = UI.System.Q("Tabletop").Q("Frame");
    //     // CMenu = new ShunContextMenu();
    //     // CMenu.name = "ContextMenu";
    //     // VisualElement menuMount = new();
    //     // menuMount.style.display = DisplayStyle.None;
    //     // menuMount.Add(CMenu);
    //     // parent.Add(menuMount);
    // }

    public static VisualElement Find()
    {
        return UI.System.Q("SelectionMenu");
    }

    public static void Update()
    {
        UI.ToggleDisplay(UI.System.Q("SelectionMenu"), SelectionMenu.Visible);
        if (Visible)
        {
            if (FollowTransform != null)
            {
                UI.FollowTransform(FollowTransform, UI.System.Q("SelectionMenu"), UI.System, Camera.main, Offset);
                UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, Length.Percent(-100)));
            }
            else
            {
                UI.System.Q("SelectionMenu").style.top = 10;
                UI.System.Q("SelectionMenu").style.left = 10;
                UI.System.Q("SelectionMenu").style.translate = new StyleTranslate(new Translate(0, 0));
            }
        }
    }

    public static void Reset(Vector2 offset, Transform follow = null)
    {
        FollowTransform = follow;
        var contextMenu = UI.System.Q<ShunContextMenu>();
        contextMenu.ClearItems();

        Vector2 pos = UI.GetTransformScreenPosition(FollowTransform, UI.System, Camera.main);
        contextMenu.OpenAtPosition(pos + offset);
    }

    public static void Hide()
    {
        Visible = false;
        FollowTransform = null;
        var contextMenu = UI.System.Q<ShunContextMenu>();
        contextMenu.ForceClose();
    }

    public static void AddItem(string label, Action action, List<MenuItem> children)
    {
        var contextMenu = UI.System.Q<ShunContextMenu>();
        var parent = contextMenu.AddItem(label, null, action);
        if (children != null)
        {
            foreach (MenuItem child in children)
            {
                var childItem = new ShunMenuItem();
                childItem.label = child.Label;
                childItem.clicked += child.Action;
                childItem.clicked += Hide;
                parent.AddSubmenuItem(childItem);
            }
        }
    }
}
