using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenMenu
{
    public static void ShowMenu()
    {
        Block.DeselectAll();
        Block.DehighlightAll();
        TokenData data = Token.GetSelected().Data;
        SelectionMenu.Reset("TOKEN MENU", new Vector2(30, 50), Token.GetSelected().transform);

        IUnitToken st = UnitTokenRegistry.DoInterfaceCallback(data.System, data.SystemData);
        MenuItem[] systemItems = st.GetTokenMenuItems(data.Placed);
        foreach (MenuItem m in systemItems)
        {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }
    }

}
