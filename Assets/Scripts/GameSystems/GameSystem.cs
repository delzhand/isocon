using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

public class GameSystem : MonoBehaviour
{
    public static string DataJson = "{}";

    protected int RoundNumber = 1;

    public static GameSystem Current()
    {
        GameSystem system = GameObject.Find("GameSystem").GetComponent<GameSystem>();

        /**
         * Game Systems Registration Point 1
         */
        switch (system.SystemName())
        {
            case "ICON 1.5":
                return system as Icon_v1_5;
            case "ICON 2.0 Playtest":
                return system as Icon_v2_0;
            case "Maleghast":
                return system as Maleghast;
            case "Generic System":
                return system as Generic;
        }
        return null;
    }

    public static string[] SystemOptions()
    {
        return new string[] { "Generic System", "ICON 1.5", "ICON 2.0 Playtest", "Maleghast" };
    }

    public static string[] HexOptionalSystems()
    {
        return new string[] { "Generic System" };
    }

    public virtual string GetOverheadAsset()
    {
        return "UITemplates/GameSystem/SimpleOverhead";
    }

    public virtual string TurnAdvanceMessage()
    {
        return "Increase the round counter?";
    }

    public virtual void Setup()
    {
        HudText.SetItem("gameSystem", SystemName(), 1, HudTextColor.Blue);
    }

    public virtual void Teardown()
    {
    }

    public virtual string SystemName()
    {
        return null;
    }

    public virtual string GetSystemVars()
    {
        return $"{RoundNumber}";
    }

    public virtual void SetSystemVars(string vars)
    {
        RoundNumber = int.Parse(vars);
        UI.System.Q<Label>("TurnNumber").text = RoundNumber.ToString();
    }

    public virtual void AddTokenModal()
    {
        Modal.AddTextField("NameField", "Token Name", "");
    }

    public virtual MenuItem[] GetTokenMenuItems(TokenData data)
    {
        List<MenuItem> items = new();
        if (data.Placed)
        {
            items.Add(new MenuItem("Remove", "Remove", ClickRemove));
            items.Add(new MenuItem("Flip", "Flip", ClickFlip));
        }
        items.Add(new MenuItem("EndTurn", "End Turn", ClickEndTurn));
        items.Add(new MenuItem("Clone", "Clone", ClickClone));
        items.Add(new MenuItem("EditName", "Edit Name", ClickEditName));
        items.Add(new MenuItem("Delete", "Delete", ClickDelete));
        return items.ToArray();
    }

    private static void ClickFlip(ClickEvent evt)
    {
        Token.GetSelected().transform.Find("Offset/Avatar/Cutout/Cutout Quad").Rotate(new Vector3(0, 180, 0));
        Token.DeselectAll();
    }

    private static void ClickRemove(ClickEvent evt)
    {
        Token.GetSelected().Remove();
        Token.DeselectAll();
    }

    private static void ClickDelete(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        string name = data.Name.Length == 0 ? "this token" : data.Name;
        Modal.DoubleConfirm("Delete Token", $"Are you sure you want to delete {name}? This action cannot be undone.", () =>
        {
            Token.DeselectAll();
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
            Token.DeselectAll();
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
            Player.Self().CmdRequestTokenDataSetValue(data.Id, $"Name|{newName}");
            Modal.Close();
            Token.DeselectAll();
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void ClickEndTurn(ClickEvent evt)
    {
        TokenData data = Token.GetSelected().Data;
        Player.Self().CmdRequestTokenDataSetValue(data.Id, "EndTurn");
        Token.DeselectAll();
    }

    public virtual MenuItem[] GetTileMenuItems()
    {
        List<MenuItem> items = new();
        return items.ToArray();
    }

    public virtual void GameDataSetValue(string value)
    {
        throw new NotImplementedException();
    }

    public virtual string[] GetEffectList()
    {
        return new string[] { "Wavy", "Spiky", "Hand", "Skull", "Hole", "Blocked", "Corners" };
    }

    public virtual void CreateToken()
    {
        throw new NotImplementedException();
    }

    public virtual bool ValidateAddToken()
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected.");
            return false;
        }
        return true;
    }

    // public virtual void DeserializeToken(string json)
    // {
    //     throw new NotImplementedException();
    // }

    // public virtual void SerializeToken(string tokenId)
    // {
    //     throw new NotImplementedException();
    // }

    public virtual void SerializeSession(string filename)
    {
        throw new NotImplementedException();
    }

    public void WriteSessionToFile(string session, string filename)
    {
        System.IO.File.WriteAllText(filename, session);
        Toast.AddSuccess($"Session saved to {filename}.");
    }

    public virtual void DeserializeSession(string filename)
    {
        throw new NotImplementedException();
    }

    public virtual void UpdateData(TokenData data)
    {
        if (data.OverheadElement == null)
        {
            data.CreateOverheadElement();
        }
    }

    public virtual void TokenDataSetValue(string tokenId, string value)
    {
        TokenData data = TokenData.Find(tokenId);
        if (value == "EndTurn")
        {
            data.UnitBarElement.Q("Portrait").style.unityBackgroundImageTintColor = ColorUtility.GetColor("#505050");
        }
        else if (value == "StartTurn")
        {
            data.UnitBarElement.Q("Portrait").style.unityBackgroundImageTintColor = Color.white;
        }
        if (value.StartsWith("Name"))
        {
            data.Name = value.Split("|")[1];
        }
    }

    public virtual void UpdateTokenPanel(string tokenId, string elementName)
    {
        throw new NotImplementedException();
    }

    public static void Set(string value)
    {
        GameSystem current = GameSystem.Current();
        if (current)
        {
            current.Teardown();
        }
        GameObject g = GameObject.Find("GameSystem");
        GameSystem system = g.GetComponent<GameSystem>();
        DestroyImmediate(system);

        switch (value)
        {
            case "Generic System":
                system = g.AddComponent<Generic>();
                break;
            case "ICON 1.5":
                system = g.AddComponent<Icon_v1_5>();
                break;
            case "ICON 2.0 Playtest":
                system = g.AddComponent<Icon_v2_0>();
                break;
            case "Maleghast":
                system = g.AddComponent<Maleghast>();
                break;
        }
        Toast.AddSimple(system.SystemName() + " initialized.");
        system.Teardown();
        system.Setup();
    }
}

[Serializable]
public class GamesystemSessionChecker
{
    public string System;

    public static bool ValidateFile(string filename)
    {
        string session = File.ReadAllText(filename);
        GamesystemSessionChecker check = JsonUtility.FromJson<GamesystemSessionChecker>(session);
        string currentSystem = GameSystem.Current().SystemName();
        if (check.System != currentSystem)
        {
            Toast.AddError($"Session system '{check.System}' does not match current system '{currentSystem}'.");
            return false;
        }
        return true;
    }


}