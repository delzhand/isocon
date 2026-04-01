using System;
using System.Collections.Generic;
using ShunUI;
using ShunUI.Primitives;
using UnityEngine;
using UnityEngine.UIElements;

public class ActorMenu
{
    public static void ShowMenu()
    {
        Block.DeselectAll();
        Block.DehighlightAll();
        ActorData data = Actor.GetSelected().Data;
        SelectionMenu.Reset(new Vector2(30, 0), Actor.GetSelected().transform);

        IActorType st = ActorTypeRegistry.DoInterfaceCallback(data.Type, data.TypeData);
        var systemItems = st.GetMenuItems(data.Placed);
        foreach (MenuItem m in systemItems)
        {
            SelectionMenu.AddItem(m.Label, m.Action, m.Children);
        }
    }

}
