using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Icon2x0EnemyUnit : Icon2x0Base
{
    private readonly static string TypeName = "Icon 2.0 Enemy";

    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public int Vigor;
    public string FoeClass;
    public int Move;
    public int Defense;
    public bool Elite;

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
        return JsonUtility.FromJson<Icon2x0EnemyUnit>(json);
    }

    public static void AddTokenModal()
    {
        Modal.AddMarkup("Description", "Icon 1.5 enemy tokens are used to create non-mob foes. Their stats come from ruledata.");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddDropdownField("ShapeField", "Shape", "Square 1x1", UnitData.SquareShapeOptions());
        Modal.AddDropdownField("FoeClassField", "Class", "Heavy", StringUtility.CreateArray("Heavy", "Artillery", "Skirmisher", "Leader", "Legend"), (evt) => { AddModalEvaluateConditions(); });
        Modal.AddToggleField("EliteField", "Elite", false);
        Modal.AddIntField("LegendHPField", "Legend HP Multiplier", 1);
        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);
        string[] fieldOrder = StringUtility.CreateArray(
            "Description",
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
        else if (foeClass != "Legend")
        {
            hpMulti = 1;
        }

        Icon2x0EnemyUnit t = new()
        {
            Type = TypeName,
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
                t.Move = 4;
                t.Defense = 3;
                t.Color = ColorUtility.GetCommonColor("red");
                t.Tags = new();
                t.Tags.Add(new UnitTag() { Name = "Guard", Color = ColorUtility.GetCommonColor("blue"), HasNumber = false });
                t.Tags.Add(new UnitTag() { Name = "Armor", Color = ColorUtility.GetCommonColor("blue"), HasNumber = true, Value = 1 });
                break;
            case "Skirmisher":
                t.MaxHP = 28 * hpMulti;
                t.CurrentHP = 28 * hpMulti;
                t.Move = 4;
                t.Defense = 6;
                t.Color = ColorUtility.GetCommonColor("yellow");
                break;
            case "Leader":
                t.MaxHP = 48 * hpMulti;
                t.CurrentHP = 48 * hpMulti;
                t.Move = 4;
                t.Defense = 4;
                t.Color = ColorUtility.GetCommonColor("green");
                break;
            case "Artillery":
                t.MaxHP = 32 * hpMulti;
                t.CurrentHP = 32 * hpMulti;
                t.Move = 4;
                t.Defense = 4;
                t.Color = ColorUtility.GetCommonColor("blue");
                t.Tags = new();
                t.Tags.Add(new UnitTag() { Name = "Aetherwall", Color = ColorUtility.GetCommonColor("blue"), HasNumber = false });
                break;
            case "Legend":
                t.MaxHP = 40 * hpMulti;
                t.CurrentHP = 40 * hpMulti;
                t.Defense = 8;
                t.Move = 5;
                t.Color = ColorUtility.GetCommonColor("purple");
                t.Tags = new();
                t.Tags.Add(new UnitTag() { Name = "Juggernaut", Color = ColorUtility.GetCommonColor("blue"), HasNumber = false });
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
        return "UI/TableTop/Overheads/Icon2";
    }

    public override void UpdatePanel(TokenData tokenData, string elementName)
    {
        base.UpdatePanel(tokenData, elementName);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement mainHPBar = panel.Q("MainHPBar");
        mainHPBar.Q<Label>("CHP").text = $"{CurrentHP}";
        mainHPBar.Q<Label>("MHP").text = $"/{MaxHP}";
        mainHPBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        mainHPBar.Q<ProgressBar>("HpBar").highValue = MaxHP;
        mainHPBar.Q<Label>("VIG").text = $"+{Vigor}";
        mainHPBar.Q<ProgressBar>("VigorBar").value = Vigor;
        mainHPBar.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(mainHPBar.Q("VigorBar"), Vigor > 0);
        UI.ToggleDisplay(mainHPBar.Q("VIG"), Vigor > 0);
        UI.ToggleDisplay(mainHPBar.Q("Wound1"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound2"), false);
        UI.ToggleDisplay(mainHPBar.Q("Wound3"), false);

        UI.ToggleDisplay(panel.Q("ElitePill"), Elite);
        UI.ToggleDisplay(panel.Q("BloodiedPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 2 && CurrentHP > MaxHP / 4);
        UI.ToggleDisplay(panel.Q("CrisisPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 4);

    }

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;

        o.Q<ProgressBar>("VigorBar").value = Vigor;
        o.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(o.Q("VigorBar"), Vigor > 0);

        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(o, CurrentHP > 0 && tokenData.Placed);
    }

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/IconHPBar");
        hpBar.name = "MainHPBar";
        hpBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q("Bars").Add(hpBar);

        VisualElement s3 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s3.Q<Label>("Label").text = "MOVE";
        s3.Q<Label>("Value").text = $"{Move}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);

        panel.Q("Pills").Add(Pill.InitStatic("ElitePill", "Elite", Color.purple));
        panel.Q("Pills").Add(Pill.InitStatic("ClassPill", FoeClass, Color));
        panel.Q("Pills").Add(Pill.InitStatic("BloodiedPill", "Bloodied", Color.red));
        panel.Q("Pills").Add(Pill.InitStatic("CrisisPill", "Crisis", Color.red));
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.TokenCommand("ModHP"); }));
        items.Add(new MenuItem("ModVIG", "Modify VIG", (evt) => { NumberPicker.TokenCommand("ModVIG"); }));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, TokenData tokenData)
    {
        base.Command(command, tokenData);
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
        if (command.StartsWith("ModVIG"))
        {
            int original = Vigor;
            int changeValue = int.Parse(command.Split("|")[1]);
            Vigor = Clamped(0, Vigor + changeValue, MaxHP / 4);
            int diff = Vigor - original;
            int maxVigor = (FoeClass == "Legend") ? 15 : MaxHP / 4;
            diff = Math.Min(maxVigor, diff);
            if (FoeClass == "Legend") { }
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_VIG", Color.white);
            }
        }
        if (command.StartsWith("Damage"))
        {
            int diff = int.Parse(command.Split("|")[1]);
            if (Vigor + CurrentHP - diff < 0)
            {
                diff = Vigor + CurrentHP;
            }
            if (diff <= 0)
            {
                return;
            }
            if (diff < Vigor)
            {
                // Vig damage only
                Vigor -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_VIG", Color.white);
                }
            }
            else if (diff > Vigor && Vigor > 0)
            {
                // Vig zeroed and HP damage
                CurrentHP -= (diff - Vigor);
                Vigor = 0;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP/VIG", Color.white);
                    UpdateGraphic(tokenData);
                }
            }
            else if (Vigor <= 0)
            {
                // HP damage only
                CurrentHP -= diff;
                if (tokenData.Placed)
                {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                    UpdateGraphic(tokenData);
                }
            }
        }
    }
    private void UpdateGraphic(TokenData tokenData)
    {
        Token token = tokenData.GetToken();
        token.SetDefeated(CurrentHP <= 0);
    }

}
