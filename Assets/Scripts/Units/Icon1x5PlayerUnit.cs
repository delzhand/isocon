using System;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

[Serializable]
public class Icon1x5PlayerUnit : Icon1x5Base
{
    private readonly static string TypeName = "Icon 1.5 Player";

    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public int Vigor;
    public int Resolve;
    public string Job;
    public string Class;
    public int Wounds;
    public int Damage;
    public int Fray;
    public int Range;
    public int Speed;
    public int Dash;
    public int Defense;
    public string ColorName;
    public int PartyResolve = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        UnitTokenRegistry.RegisterSystem($"{TypeName}");
        UnitTokenRegistry.RegisterInterfaceCallback($"{TypeName}", DeserializeAsInterface);
        UnitTokenRegistry.RegisterSimpleCallback($"{TypeName}|AddTokenModal", AddTokenModal);
    }


    public override string Label()
    {
        return Name;
    }

    public override string Serialize()
    {
        return JsonUtility.ToJson(this);
    }

    public override string GetOverheadAsset()
    {
        return "UI/TableTop/Overheads/Icon1x5";
    }

    public override MenuItem[] GetMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("ModHP", "Modify HP", (evt) => { NumberPicker.TokenCommand("ModHP"); }));
        items.Add(new MenuItem("ModVIG", "Modify Vigor", (evt) => { NumberPicker.TokenCommand("ModVIG"); }));
        items.Add(new MenuItem("ModRES", "Modify Resolve", (evt) => { NumberPicker.TokenCommand("ModRES"); }));
        items.Add(new MenuItem("ModPRES", "Modify Party Resolve", (evt) => { NumberPicker.AllTokensCommand("ModPRES"); }));
        items.Add(new MenuItem("GainWound", "Take Wound", (evt) => DirectCommand("GainWound")));
        items.Add(new MenuItem("HealWound", "Heal Wound", (evt) => DirectCommand("LoseWound")));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void Command(string command, TokenData tokenData)
    {
        base.Command(command, tokenData);
        Token token = tokenData.GetToken();
        if (command.StartsWith("GainWound"))
        {
            Wounds++;
            Wounds = Math.Min(Wounds, 3);
            int woundMaxHP = MaxHP / 4 * (4 - Wounds);
            CurrentHP = Math.Min(CurrentHP, woundMaxHP);
        }
        if (command.StartsWith("LoseWound"))
        {
            Wounds--;
            Wounds = Math.Max(Wounds, 0);
        }
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
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_VIG", Color.white);
            }
        }
        if (command.StartsWith("ModRES"))
        {
            int original = Resolve;
            int changeValue = int.Parse(command.Split("|")[1]);
            Resolve = Clamped(0, Resolve + changeValue, 6);
            int diff = Resolve - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_RES", Color.white);
            }
        }
        if (command.StartsWith("ModPRES"))
        {
            int original = PartyResolve;
            int changeValue = int.Parse(command.Split("|")[1]);
            PartyResolve = Clamped(0, PartyResolve + changeValue, 6);
            int diff = PartyResolve - original;
            if (diff != 0 && tokenData.Placed)
            {
                string plus = diff > 0 ? "+" : "";
                PopoverText.Create(token, $"/{plus}{diff}|_P-RES", Color.white);
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

    public override void UpdateOverhead(TokenData tokenData)
    {
        VisualElement o = tokenData.OverheadElement;

        o.Q<ProgressBar>("VigorBar").value = Vigor;
        o.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(o.Q("VigorBar"), Vigor > 0);

        o.Q<ProgressBar>("HpBar").value = CurrentHP;
        o.Q<ProgressBar>("HpBar").highValue = MaxHP;

        UI.ToggleDisplay(o.Q("Wound1"), Wounds >= 1);
        UI.ToggleDisplay(o.Q("Wound2"), Wounds >= 2);
        UI.ToggleDisplay(o.Q("Wound3"), Wounds >= 3);

        UI.ToggleDisplay(o, CurrentHP > 0 && tokenData.Placed);
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
        UI.ToggleDisplay(mainHPBar.Q("Wound1"), Wounds >= 1);
        UI.ToggleDisplay(mainHPBar.Q("Wound2"), Wounds >= 2);
        UI.ToggleDisplay(mainHPBar.Q("Wound3"), Wounds >= 3);

        VisualElement RESBar = panel.Q("ResBar");
        RESBar.Q<Label>("ResolveNum").text = $"{Resolve}";
        RESBar.Q<ProgressBar>("ResolveBar").value = Resolve + PartyResolve;
        RESBar.Q<ProgressBar>("ResolveBar").highValue = 6;
        RESBar.Q<Label>("PartyResolveNum").text = $"+{PartyResolve}";
        RESBar.Q<ProgressBar>("PartyResolveBar").value = PartyResolve;
        RESBar.Q<ProgressBar>("PartyResolveBar").highValue = 6;
        UI.ToggleDisplay(panel.Q("PartyResolveBar"), PartyResolve > 0);
        UI.ToggleDisplay(panel.Q("PartyResolveNum"), PartyResolve > 0);

        UI.ToggleDisplay(panel.Q("BloodiedPill"), CurrentHP > 0 && CurrentHP <= MaxHP / 2);
    }

    public override void InitPanel(string elementName, bool selected)
    {
        base.InitPanel(elementName, selected);
        VisualElement panel = UI.System.Q(elementName);

        VisualElement resBar = UI.CreateFromTemplate("UI/TableTop/IconResolveBar");
        resBar.name = "ResBar";
        resBar.Q<ProgressBar>("ResolveBar").value = Resolve;
        resBar.Q<ProgressBar>("ResolveBar").highValue = 6;
        panel.Q("Bars").Add(resBar);

        VisualElement hpBar = UI.CreateFromTemplate("UI/TableTop/IconHPBar");
        hpBar.name = "MainHPBar";
        hpBar.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q("Bars").Add(hpBar);

        VisualElement s1 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s1.Q<Label>("Label").text = "DMG/FRAY";
        s1.Q<Label>("Value").text = $"1d{Damage}/{Fray}";
        panel.Q("Stats").Add(s1);

        VisualElement s2 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s2.Q<Label>("Label").text = "RNG";
        s2.Q<Label>("Value").text = $"{Range}";
        panel.Q("Stats").Add(s2);

        VisualElement s3 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s3.Q<Label>("Label").text = "SPD/DASH";
        s3.Q<Label>("Value").text = $"{Speed}/{Dash}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UI/TableTop/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);

        panel.Q("Pills").Add(Pill.InitStatic("JobPill", Job, Color));
        panel.Q("Pills").Add(Pill.InitStatic("ClassPill", Class, Color));
        panel.Q("Pills").Add(Pill.InitStatic("BloodiedPill", "Bloodied", Color.red));
    }

    public static void AddTokenModal()
    {
        string[] playerJobs = StringUtility.CreateArray(
            "Stalwart/Bastion",
            "Stalwart/Demon Slayer",
            "Stalwart/Colossus",
            "Stalwart/Knave",
            "Vagabond/Fool",
            "Vagabond/Freelancer",
            "Vagabond/Shade",
            "Vagabond/Warden",
            "Mendicant/Chanter",
            "Mendicant/Harvester",
            "Mendicant/Sealer",
            "Mendicant/Seer",
            "Wright/Enochian",
            "Wright/Geomancer",
            "Wright/Spellblade",
            "Wright/Stormbender"
        );

        Modal.AddMarkup("Description", "ICON 1.5 Player tokens derive their stats from Icon1_5 data in the ruleset file.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddSearchField("PlayerJob", "Job", "Stalwart/Bastion", playerJobs);

        Modal.AddPreferredButton("Create Token", CreateClicked);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        // Necessary to ensure fields are in order and can be cleared when changing type dropdown
        AddToken.OrderFields(StringUtility.CreateArray("Description", "TokenSearchField", "NameField", "PlayerJob"));
    }

    private static void CreateClicked(ClickEvent evt)
    {
        if (!TokenLibrary.TokenSelected())
        {
            Toast.AddError("A token has not been selected");
            return;
        }

        string name = UI.Modal.Q<TextField>("NameField").value;
        string playerJob = SearchField.GetValue(UI.Modal.Q("PlayerJob"));
        if (playerJob.Length == 0)
        {
            Toast.AddError("You must select a job");
            return;
        }
        string pclass = playerJob.Split("/")[0];
        string job = playerJob.Split("/")[1];

        Icon1x5PlayerUnit t = new()
        {
            System = TypeName,
            Name = name,
            Job = job,
            Class = pclass,
            // MaxHP = stats["MaxHP"],
            // CurrentHP = stats["MaxHP"],
            Vigor = 0,
            Wounds = 0,
            // Damage = stats["Damage"],
            // Fray = stats["Fray"],
            // Range = stats["Range"],
            // Speed = stats["Speed"],
            // Dash = stats["Dash"],
            // Defense = stats["Defense"],
            // Color = ColorUtility.GetCommonColor(color),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };

        switch (pclass)
        {
            case "Stalwart":
                t.MaxHP = 40;
                t.CurrentHP = 40;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 6;
                t.Fray = 4;
                t.Damage = 6;
                t.Color = ColorUtility.GetCommonColor("red");
                break;
            case "Vagabond":
                t.MaxHP = 28;
                t.CurrentHP = 28;
                t.Speed = 4;
                t.Dash = 4;
                t.Defense = 10;
                t.Fray = 2;
                t.Damage = 10;
                t.Color = ColorUtility.GetCommonColor("yellow");
                break;
            case "Mendicant":
                t.MaxHP = 40;
                t.CurrentHP = 40;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 8;
                t.Fray = 3;
                t.Damage = 6;
                t.Color = ColorUtility.GetCommonColor("green");
                break;
            case "Wright":
                t.MaxHP = 32;
                t.CurrentHP = 32;
                t.Speed = 4;
                t.Dash = 2;
                t.Defense = 7;
                t.Fray = 3;
                t.Damage = 8;
                t.Color = ColorUtility.GetCommonColor("blue");
                break;
        }


        AddToken.FinalizeToken(t.Serialize());
    }

    public static IUnitData DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<Icon1x5PlayerUnit>(json);
    }

    #region Private functions

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
        token.SetDefeated(CurrentHP <= 0);
    }
    #endregion
}