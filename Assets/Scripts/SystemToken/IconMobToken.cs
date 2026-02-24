using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class IconMobToken : SystemToken
{
    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SystemTokenRegistry.RegisterSystem("Icon Mob");
        SystemTokenRegistry.RegisterInterfaceCallback("Icon Mob", DeserializeAsInterface);
        SystemTokenRegistry.RegisterSimpleCallback("Icon Mob|AddTokenModal", AddTokenModal);
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
    public static ISystemToken DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<IconMobToken>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int Hits;
    public int Damage;
    public int Fray;
    public int Range;
    public int Speed;
    public int Dash;
    public int Defense;
    #endregion

    #region Creation
    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "ICON Mob tokens have two hit counters instead of an HP bar.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");

        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "NameField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        JSONNode stats = gamedata["Icon1_5"]["Stats"]["Gray"];

        IconMobToken t = new()
        {
            System = "Icon Mob",
            Name = name,
            Hits = 2,
            Damage = stats["Damage"],
            Fray = stats["Fray"],
            Range = stats["Range"],
            Speed = stats["Speed"],
            Dash = stats["Dash"],
            Defense = stats["Defense"],
            Color = ColorUtility.GetCommonColor("Gray"),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };

        AddToken.FinalizeToken(t.Serialize());
    }
    #endregion

    public override string Label()
    {
        return Name;
    }

    public override string GetOverheadAsset()
    {
        return "UITemplates/GameSystem/Overheads/PipCounter";
    }

    public override MenuItem[] GetTokenMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetTokenMenuItems(placed);

        List<MenuItem> items = new();
        if (Hits > 0)
        {
            items.Add(new MenuItem("TakeHit", "Take Hit", (evt) =>
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "TakeHit");
                SelectionMenu.Hide();
            }));
        }
        if (Hits < 2)
        {
            items.Add(new MenuItem("RestoreHit", "Restore Hit", (evt) =>
            {
                Player.Self().CmdRequestTokenDataCommand(Token.GetSelected().Data.Id, "RestoreHit");
                SelectionMenu.Hide();
            }));
        }

        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void HandleCommand(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        base.HandleCommand(command, tokenData);
        if (command == "TakeHit")
        {
            if (Hits > 0)
            {
                Hits -= 1;
                PopoverText.Create(token, $"-/1|_HIT", Color.white);
            }
            UpdateGraphic(tokenData);
        }
        if (command == "RestoreHit")
        {
            if (Hits < 2)
            {
                Hits += 1;
                PopoverText.Create(token, $"+/1|_HIT", Color.white);
            }
            UpdateGraphic(tokenData);
        }
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<Label>("Pips").text = SymbolString("▰", Hits, 2);
    }

    private static string GetStatColor(string job)
    {
        string statColor = "Gray";
        switch (job)
        {
            case "Wright":
            case "Artillery":
                statColor = "Blue";
                break;
            case "Vagabond":
            case "Skirmisher":
                statColor = "Yellow";
                break;
            case "Stalwart":
            case "Heavy":
                statColor = "Red";
                break;
            case "Leader":
            case "Mendicant":
                statColor = "Green";
                break;
            case "Legend":
                statColor = "Purple";
                break;
        }
        return statColor;
    }

    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(Hits <= 0);
    }
}