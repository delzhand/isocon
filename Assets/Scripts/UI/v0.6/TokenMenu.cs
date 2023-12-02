using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TokenMenu
{
    public static void ShowMenu() {
        Block.DeselectAll();
        Block.DehighlightAll();
        TokenData data = Token.GetSelected().onlineDataObject.GetComponent<TokenData>();
        SelectionMenu.Reset("Token Menu");
        MenuItem[] defaultItems = GetTokenMenuItems(data);
        foreach (MenuItem m in defaultItems) {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }
        MenuItem[] systemItems = GameSystem.Current().GetTokenMenuItems(data);
        foreach (MenuItem m in systemItems) {
            SelectionMenu.AddItem(m.Name, m.Label, m.OnClick);
        }
    }

    public static MenuItem[] GetTokenMenuItems(TokenData data)
    {
        List<MenuItem> items = new();
        if (!data.OnField) {
            items.Add(new MenuItem("Place", "Place", ClickPlace));
        }
        else {
            items.Add(new MenuItem("Remove", "Remove", ClickRemove));
            items.Add(new MenuItem("Move", "Move", ClickMove));
        }
        items.Add(new MenuItem("Delete", "Delete", ClickDelete));
        return items.ToArray();
    }

    public static void ClickPlace(ClickEvent evt) {
        if (SelectionMenu.IsActive("Place")) {
            EndCursorMode();
            return;
        }
        Block.DeselectAll();
        Block.UnfocusAll();
        Cursor.Mode = CursorMode.Placing;
        SelectionMenu.ActivateItem("Place");
    }

    public static void DoPlace(Block b) {
        Token.GetSelected().Place(b);
        EndCursorMode();
    }

    public static void ClickMove(ClickEvent evt) {
        if (SelectionMenu.IsActive("Move")) {
            EndCursorMode();
            return;
        }
        Block.DeselectAll();
        Block.UnfocusAll();
        Cursor.Mode = CursorMode.Moving;
        SelectionMenu.ActivateItem("Move");
    }

    public static void DoMove(Block b) {
        Token.GetSelected().Move(b);
    }

    public static void EndCursorMode() {
        Block.DehighlightAll();
        SelectionMenu.DeactivateItem();
        ShowMenu();
    }

    public static void ClickRemove(ClickEvent evt) {
        Token.GetSelected().onlineDataObject.GetComponent<TokenData>().OnField = false;
        Token.GetSelected().UpdateVisualEffect();
        ShowMenu();
    }

    public static void ClickDelete(ClickEvent evt) {
        TokenData data = Token.GetSelected().onlineDataObject.GetComponent<TokenData>();
        Token.DeselectAll();
        Player.Self().CmdRequestDeleteToken(data);
    }
}
