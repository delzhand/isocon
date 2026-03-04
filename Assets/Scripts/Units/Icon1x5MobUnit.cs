using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Icon1x5MobUnit : UnitData
{
    private readonly static string TypeName = "Icon 1.5 Mob";

    #region Registration
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        UnitTokenRegistry.RegisterSystem($"{TypeName}");
        UnitTokenRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        UnitTokenRegistry.RegisterSimpleCallback($"{TypeName}|AddTokenModal", AddTokenModal);
    }
    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }
    public static IUnitData DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<Icon1x5MobUnit>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int Hits;
    public int Vigor;
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
        Modal.AddMarkup("Description", "ICON 1.5 Mob tokens have two hit counters instead of an HP bar.");
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

        Icon1x5MobUnit t = new()
        {
            System = TypeName,
            Name = name,
            Hits = 2,
            Damage = 6,
            Fray = 3,
            Speed = 4,
            Dash = 2,
            Defense = 8,
            Vigor = 0,
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
        return "UI/TableTop/Overheads/PipCounter";
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("Damage", "Damage HP/VIG", (evt) => { NumberPicker.NumberCommand("Damage"); }));

        items.Add(new MenuItem("ModVig", "Modify VIG", (evt) => { NumberPicker.NumberCommand("ModVIG"); }));
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

    public override void Command(string command, TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        base.Command(command, tokenData);
        if (command.StartsWith("Damage"))
        {
            int diff = Math.Abs(int.Parse(command.Split("|")[1]));
            if (diff < Vigor)
            {
                Vigor -= diff;
                PopoverText.Create(token, $"/-{diff}|_VIG", Color.white);
            }
            else if (diff > 0)
            {
                Vigor = 0;
                Hits--;
                PopoverText.Create(token, $"/-1|_HIT", Color.white);
            }
            UpdateGraphic(tokenData);
        }
        if (command == "RestoreHit")
        {
            if (Hits < 2)
            {
                Hits += 1;
                PopoverText.Create(token, $"/+1|_HIT", Color.white);
            }
            UpdateGraphic(tokenData);
        }
        if (command.StartsWith("ModVIG"))
        {
            int original = Vigor;
            int changeValue = int.Parse(command.Split("|")[1]);
            Vigor = Clamped(0, Vigor + changeValue, 6);
            int diff = Vigor - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_VIG", Color.white);
            }
        }
        if (command.StartsWith("Rename|"))
        {
            Name = command.Split("|")[1];
        }

    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        if (Vigor > 0)
        {
            o.Q<Label>("Pips").text = MobHPString();
        }
        else
        {
            o.Q<Label>("Pips").text = SymbolString("■", Hits, 2);
        }

        UI.ToggleDisplay(o, Hits > 0 && tokenData.Placed);
    }

    private string MobHPString()
    {
        string x = "■";
        StringBuilder sb = new();
        for (int i = 0; i < Hits; i++)
        {
            // if (i == value)
            // {
            //     sb.Append("<color=white>");
            // }
            sb.Append(x);
        }
        sb.Append("<color=#25E1F2>");
        for (int i = 0; i < Vigor; i++)
        {
            sb.Append(x);
        }
        sb.Append("</color>");
        return sb.ToString();
    }

    // private static string GetStatColor(string job)
    // {
    //     string statColor = "Gray";
    //     switch (job)
    //     {
    //         case "Wright":
    //         case "Artillery":
    //             statColor = "Blue";
    //             break;
    //         case "Vagabond":
    //         case "Skirmisher":
    //             statColor = "Yellow";
    //             break;
    //         case "Stalwart":
    //         case "Heavy":
    //             statColor = "Red";
    //             break;
    //         case "Leader":
    //         case "Mendicant":
    //             statColor = "Green";
    //             break;
    //         case "Legend":
    //             statColor = "Purple";
    //             break;
    //     }
    //     return statColor;
    // }

    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(Hits <= 0);
    }
}