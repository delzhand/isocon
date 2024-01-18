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
        SelectionMenu.Reset("TOKEN MENU");
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
        if (data.Placed) {
            items.Add(new MenuItem("Remove", "Remove", ClickRemove));
            items.Add(new MenuItem("Flip", "Flip", ClickFlip));
        }
        // items.Add(new MenuItem("MoveLeft", "Move Up in Order", ClickMoveUp));
        // items.Add(new MenuItem("MoveLeft", "Move Down in Order", ClickMoveDown));
        items.Add(new MenuItem("EndTurn", "End Turn", ClickEndTurn));
        items.Add(new MenuItem("Clone", "Clone", ClickClone));
        items.Add(new MenuItem("Delete", "Delete", ClickDelete));
        return items.ToArray();
    }

    public static void ClickFlip(ClickEvent evt) {
        Token.GetSelected().transform.Find("Offset/Avatar/Cutout/Cutout Quad").Rotate(new Vector3(0, 180, 0));
        Token.DeselectAll();
    }

    public static void ClickRemove(ClickEvent evt) {
        Token.GetSelected().Remove();
        Token.DeselectAll();
    }

    public static void ClickDelete(ClickEvent evt) {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Delete Token", $"Are you sure you want to delete {name}? This action cannot be undone.", () => {
            Token.DeselectAll();
            Player.Self().CmdRequestDeleteToken(data.Id);
        });
    }

    public static void ClickClone(ClickEvent evt) {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Clone Token", $"Are you sure you want to clone {name}?", () => {
            Player.Self().CmdCreateToken(data.System, data.GraphicHash, data.Name, data.Size, data.Color, data.SystemData);
            Token.DeselectAll();
        });
    }

    public static void ClickEndTurn(ClickEvent evt) {
        TokenData data = Token.GetSelected().Data;
        Player.Self().CmdRequestTokenDataSetValue(data.Id, "EndTurn");
        Token.DeselectAll();
    }

    public static void ClickMoveUp(ClickEvent evt) {
        TokenData data = Token.GetSelected().Data;
    }

    public static void ClickMoveDown(ClickEvent evt) {
        TokenData data = Token.GetSelected().Data;
    }

}
