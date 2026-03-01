using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Icon1x5EnemyToken : SystemToken
{
    private readonly static string TypeName = "Icon 1.5 Enemy";

    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public string FoeClass;
    public int Fray;
    public int Speed;
    public int Dash;
    public int Defense;
    public int Damage;
    public bool Elite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SystemTokenRegistry.RegisterSystem($"{TypeName}");
        SystemTokenRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        SystemTokenRegistry.RegisterSimpleCallback($"{TypeName}|AddTokenModal", AddTokenModal);
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public static ISystemToken DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<Icon1x5EnemyToken>(json);
    }

    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Icon 1.5 enemy tokens are used to create non-mob foes. Their stats come from ruledata.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", StringUtility.CreateArray("Square 1x1", "Square 2x2", "Square 3x3", "Hex 1", "Hex 2", "Hex 3"));
        Modal.AddDropdownField("FoeClassField", "Class", "Heavy", StringUtility.CreateArray("Heavy", "Artillery", "Skirmisher", "Leader", "Legend"), (evt) => { AddModalEvaluateConditions(); });
        Modal.AddToggleField("EliteField", "Elite", false);
        Modal.AddIntField("LegendHPField", "Legend HP Multiplier", 1);
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        string[] fieldOrder = StringUtility.CreateArray(
            "Description",
            "TokenSearchField",
            "NameField",
            "ShapeField",
            "FoeClassField",
            "EliteField",
            "LegendHPField"
        );
        AddToken.OrderFields(fieldOrder);
        AddModalEvaluateConditions();
    }

    private static void AddModalEvaluateConditions()
    {
        string foeClass = UI.Modal.Q<DropdownField>("FoeClassField").value;

        VisualElement eliteField = UI.Modal.Q("EliteField");
        VisualElement hpMultiField = UI.Modal.Q("LegendHPField");

        UI.ToggleDisplay(eliteField, foeClass != "Legend");
        UI.ToggleDisplay(hpMultiField, foeClass == "Legend");
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        string shape = UI.Modal.Q<DropdownField>("ShapeField").value;
        string foeClass = UI.Modal.Q<DropdownField>("FoeClassField").value;
        int hpMulti = UI.Modal.Q<IntegerField>("LegendHPField").value;
        bool elite = UI.Modal.Q<Toggle>("EliteField").value;
        if (elite)
        {
            hpMulti = 2;
        }

        Icon1x5EnemyToken t = new()
        {
            System = TypeName,
            Name = name,
            Shape = shape,
            Elite = elite,
            FoeClass = foeClass,
            TokenMeta = TokenLibrary.GetSelectedMeta() // required, do not change
        };

        switch (foeClass)
        {
            case "Heavy":
                t.MaxHP = 40 * hpMulti;
                t.CurrentHP = 40 * hpMulti;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 6;
                t.Fray = 4;
                t.Damage = 6;
                t.Color = ColorUtility.GetCommonColor("red");
                break;
            case "Skirmisher":
                t.MaxHP = 28 * hpMulti;
                t.CurrentHP = 28 * hpMulti;
                t.Speed = 4;
                t.Dash = 4;
                t.Defense = 10;
                t.Fray = 2;
                t.Damage = 10;
                t.Color = ColorUtility.GetCommonColor("yellow");
                break;
            case "Leader":
                t.MaxHP = 40 * hpMulti;
                t.CurrentHP = 40 * hpMulti;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 7;
                t.Fray = 3;
                t.Damage = 8;
                t.Color = ColorUtility.GetCommonColor("green");
                break;
            case "Artillery":
                t.MaxHP = 32 * hpMulti;
                t.CurrentHP = 32 * hpMulti;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 7;
                t.Fray = 3;
                t.Damage = 8;
                t.Color = ColorUtility.GetCommonColor("blue");
                break;
            case "Legend":
                t.MaxHP = 50 * hpMulti;
                t.CurrentHP = 50 * hpMulti;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 8;
                t.Fray = 3;
                t.Damage = 8;
                t.Color = ColorUtility.GetCommonColor("purple");
                break;
        }

        AddToken.FinalizeToken(t.Serialize());
    }
    public override string Label()
    {
        return Name;
    }

    public override string GetOverheadAsset()
    {
        return "UITemplates/GameSystem/Overheads/SingleBar";
    }

    public override void UpdateTokenPanel(TokenData tokenData, string elementName)
    {
        base.UpdateTokenPanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement mainHPBar = panel.Q("MainHPBar");
        mainHPBar.Q<Label>("CHP").text = $"{CurrentHP}";
        mainHPBar.Q<Label>("MHP").text = $"/{MaxHP}";
        mainHPBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        mainHPBar.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(panel.Q("ElitePill"), Elite);
    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;
        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;
        UI.ToggleDisplay(o, CurrentHP > 0);
    }

    public override void InitTokenPanel(string elementName, bool selected)
    {
        base.InitTokenPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement hpBar = UI.CreateFromTemplate("UITemplates/GameSystem/SimpleHPBar");
        hpBar.name = "MainHPBar";
        hpBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q("Bars").Add(hpBar);

        VisualElement s1 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s1.Q<Label>("Label").text = "DMG/FRAY";
        s1.Q<Label>("Value").text = $"1d{Damage}/{Fray}";
        panel.Q("Stats").Add(s1);

        VisualElement s3 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s3.Q<Label>("Label").text = "SPD/DASH";
        s3.Q<Label>("Value").text = $"{Speed}/{Dash}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);

        VisualElement p1 = UI.CreateFromTemplate("UITemplates/GameSystem/Pill");
        p1.name = "ElitePill";
        p1.Q<Label>("Name").text = "Elite";
        p1.Q("Pill").style.backgroundColor = ColorUtility.GetCommonColor("Purple");
        p1.Query(null, "roundbutton").ForEach((v) =>
        {
            v.style.display = DisplayStyle.None;
        });
        panel.Q("Pills").Add(p1);

        VisualElement p2 = UI.CreateFromTemplate("UITemplates/GameSystem/Pill");
        p2.name = "ClassPill";
        p2.Q<Label>("Name").text = FoeClass;
        p2.Q("Pill").style.backgroundColor = Color;
        p2.Query(null, "roundbutton").ForEach((v) =>
        {
            v.style.display = DisplayStyle.None;
        });
        panel.Q("Pills").Add(p2);
    }

    public override MenuItem[] GetTokenMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetTokenMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.NumberCommand("ModHP"); }));
        items.Add(new MenuItem("AttackRoll", "Attack Roll", AttackRollClicked));
        items.Add(new MenuItem("SaveRoll", "Save Roll", SaveRollClicked));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void HandleCommand(string command, TokenData tokenData)
    {
        base.HandleCommand(command, tokenData);
        Token token = tokenData.GetToken();
        if (command.StartsWith("ModHP"))
        {
            int original = CurrentHP;
            int changeValue = int.Parse(command.Split("|")[1]);
            CurrentHP = Clamped(0, CurrentHP + changeValue, MaxHP);
            int diff = CurrentHP - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_HP", Color.white);
                UpdateGraphic(tokenData);
            }
        }
    }

    private void AttackRollClicked(ClickEvent evt)
    {
        Modal.Reset("Attack Roll");
        Modal.AddNumberNudgerField("PowerField", "Weakness/Power", 0, -20);
        Modal.AddPreferredButton("Roll", AttackRoll);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private void SaveRollClicked(ClickEvent evt)
    {
        string name = Token.GetSelected().Data.Name;
        DiceRoller.DirectDieRoll("sum", "1d6", $"{name}'s save roll");
        Modal.Close();
    }

    private void AttackRoll(ClickEvent evt)
    {
        string name = Token.GetSelected().Data.Name;
        int power = UI.Modal.Q<NumberNudger>("PowerField").value;
        string op = power > 0 ? "max" : "min";
        DiceRoller.DirectDieRoll(op, $"{Math.Abs(power) + 1}d10", $"{name}'s attack roll");
        Modal.Close();
    }

    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(CurrentHP <= 0);
    }
}