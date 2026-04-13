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

    public MenuItem(string label, bool enabled = true)
    {
        Label = label;
        if (!enabled)
        {
            Label = $"<color=grey>{label}</color>";
        }
        Action = null;
        Children = new();
    }
}

public class SelectionMenu
{
    // private static string ActiveItem;
    public static Transform FollowTransform;
    public static bool Visible;

    public static void Open(Vector2 offset, Transform follow = null)
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

    public static bool IsOpen
    {
        get
        {
            var contextMenu = UI.System.Q<ShunContextMenu>();
            return contextMenu.isOpen;
        }
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
