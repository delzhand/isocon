
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public interface ISystemToken
{
    string Serialize();
    string Label();
    string GetOverheadAsset();
    MenuItem[] GetTokenMenuItems(bool placed);
    void HandleCommand(string command, TokenData tokenData);
    void UpdateOverhead(TokenData tokenData);
    void UpdateTokenPanel(TokenData tokenData, string elementName);
    void InitTokenPanel(string elementName);

}

[Serializable]
public abstract class SystemToken : ISystemToken
{
    public string System;
    public TokenMeta TokenMeta;
    public string Shape;
    public Color Color;

    public virtual string Label()
    {
        throw new NotImplementedException();
    }

    public virtual string Serialize()
    {
        throw new NotImplementedException();
    }

    public virtual string GetOverheadAsset()
    {
        return null;
    }

    public virtual MenuItem[] GetTokenMenuItems(bool placed)
    {
        List<MenuItem> items = new();
        if (placed)
        {
            items.Add(new MenuItem("Remove", "Remove", ClickRemove));
            items.Add(new MenuItem("Flip", "Flip", ClickFlip));
        }
        // items.Add(new MenuItem("Reshape", "Reshape", ClickReshape));
        items.Add(new MenuItem("EditName", "Rename", ClickEditName));
        items.Add(new MenuItem("Clone", "Clone", ClickClone));
        items.Add(new MenuItem("Delete", "Delete", ClickDelete));
        return items.ToArray();
    }

    private static void ClickFlip(ClickEvent evt)
    {
        Token.GetSelected().transform.Find("Offset/Avatar/Cutout/Cutout Quad").Rotate(new Vector3(0, 180, 0));
        Token.Deselect();
    }

    private static void ClickRemove(ClickEvent evt)
    {
        Token.GetSelected().Remove();
        Token.Deselect();
    }

    private static void ClickDelete(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Delete Token", $"Are you sure you want to delete {name}? This action cannot be undone.", () =>
        {
            Token.Deselect();
            Player.Self().CmdRequestDeleteToken(data.Id);
        });
    }

    private static void ClickClone(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Clone Token", $"Are you sure you want to clone {name}?", () =>
        {
            Player.Self().CmdCreateToken(data.System, data.TokenMeta, data.Name, data.Size, data.Color, data.SystemData);
            Token.Deselect();
        });
    }

    private static void ClickEditName(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        Modal.Reset("Edit Name");
        Modal.AddTextField("Name", "Name", data.Name);
        Modal.AddPreferredButton("Confirm", (evt) =>
        {
            string newName = UI.Modal.Q<TextField>("Name").value.Trim();
            Player.Self().CmdRequestTokenDataCommand(data.Id, $"Rename|{newName}");
            Modal.Close();
            Token.Deselect();
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    public virtual void HandleCommand(string command, TokenData tokenData)
    {
    }

    public virtual void UpdateOverhead(TokenData tokenData)
    {
    }

    public virtual void UpdateTokenPanel(TokenData tokenData, string elementName)
    {
    }

    public virtual void InitTokenPanel(string elementName)
    {
    }

    public string SymbolString(string character, int value, int max)
    {
        StringBuilder sb = new();
        for (int i = 0; i < max; i++)
        {
            if (i == value)
            {
                sb.Append("<color=white>");
            }
            sb.Append(character);
        }
        sb.Append("</color>");
        return sb.ToString();
    }

    public void DirectCommand(string command)
    {
        Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, command);
        SelectionMenu.Hide();
    }
}

[Serializable]
public class SystemTokenMeta
{
    public string Name;
    public string System;
    public TokenMeta TokenMeta;
    public string Shape;
    public Color Color;
}