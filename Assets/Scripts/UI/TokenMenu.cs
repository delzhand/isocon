using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenMenu
{
    public static void ShowMenu() {
        Block.DeselectAll();
        Block.DehighlightAll();
        TokenData data = Token.GetSelected().Data;
        SelectionMenu.Reset("TOKEN MENU", new Vector2(30, 50), Token.GetSelected().transform);
        MenuItem[] systemItems = GameSystem.Current().GetTokenMenuItems(data);
        foreach (MenuItem m in systemItems) {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }
    }

}
