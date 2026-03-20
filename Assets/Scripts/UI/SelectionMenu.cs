using System;
using System.Collections;
using System.Collections.Generic;
using ShunUI;
using ShunUI.Primitives;
using UnityEngine;
using UnityEngine.UIElements;

public struct MenuItem
{
    public string Name;
    public string Label;
    public Action Action;

    public MenuItem(string name, string label, Action onClick)
    {
        Name = name;
        Label = label;
        Action = onClick;
    }
}

public class SelectionMenu
{
    // private static string ActiveItem;
    public static Transform FollowTransform;
    public static bool Visible;

    public static Vector2 Offset;

    private static ShunContextMenu CMenu;

    public static void Setup()
    {
        UI.SetBlocking(UI.System, "SelectionMenu");

        VisualElement parent = UI.System.Q("Tabletop").Q("Frame");
        CMenu = new ShunContextMenu();
        CMenu.name = "ContextMenu";
        VisualElement menuMount = new();
        menuMount.style.display = DisplayStyle.None;
        menuMount.Add(CMenu);
        parent.Add(menuMount);
    }

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

    public static void Reset(string title, Vector2 offset, Transform follow = null)
    {
        FollowTransform = follow;
        CMenu.ClearItems();
        // var test = CMenu.AddItem("Test");

        // var test2 = new ShunMenuItem();
        // test2.label = "Child";
        // test2.clicked += () => { Debug.Log("child"); };
        // test.AddSubmenuItem(test2);

        Vector2 pos = UI.GetTransformScreenPosition(FollowTransform, UI.System, Camera.main);
        CMenu.OpenAtPosition(pos + offset);
    }

    public static void Hide()
    {
        Visible = false;
        FollowTransform = null;
        if (CMenu != null)
        {
            CMenu.ClearItems();
        }
    }

    public static void AddItem(string name, string label, Action action)
    {
        CMenu.AddItem(label, null, action);
    }
}
