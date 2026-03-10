using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Icon2x0MobUnit : Icon1x5Base
{
    private readonly static string TypeName = "Icon 2.0 Mob";

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
        return JsonUtility.FromJson<Icon2x0MobUnit>(json);
    }
    #endregion

    #region Stats
    public string Name;
    public int Hits;
    public int Vigor;
    public int Move;
    public int Defense;
    #endregion

    #region Creation
    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "ICON 2.0 Mob tokens have two hit counters instead of an HP bar.");
        Modal.AddTextField("NameField", "Token Name", "Token");

        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        AddToken.OrderFields(StringUtility.CreateArray("Description", "NameField"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;

        Icon2x0MobUnit t = new()
        {
            Type = TypeName,
            Name = name,
            Hits = 2,
            Move = 4,
            Defense = 4,
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
        items.Add(new MenuItem("ModVig", "Modify VIG", (evt) => { NumberPicker.TokenCommand("ModVIG"); }));
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
            diff = Math.Min(diff, 6);
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

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        Label l = new();
        l.name = "MainHPLabel";
        l.text = MobHPString();
        l.style.color = Color.red;
        l.style.unityTextOutlineColor = Color.white;
        l.style.unityTextOutlineWidth = 1;
        l.style.fontSize = 26;
        panel.Q("Bars").Add(l);

        VisualElement s3 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s3.Q<Label>("Label").text = "MOVE";
        s3.Q<Label>("Value").text = $"{Move}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);

        panel.Q("Pills").Add(Pill.InitStatic("ClassPill", "Mob", Color));
    }

    private string MobHPString()
    {
        string x = "■";
        StringBuilder sb = new();
        for (int i = 0; i < Hits; i++)
        {
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

    public override void UpdatePanel(TokenData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        Label mainHPLabel = panel.Q<Label>("MainHPLabel");
        mainHPLabel.text = MobHPString();
    }

    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(Hits <= 0);
    }
}