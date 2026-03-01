using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IsoconUILibrary;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class Icon1x5PlayerToken : SystemToken
{
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

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SystemTokenRegistry.RegisterSystem("Icon 1.5 Player");
        SystemTokenRegistry.RegisterInterfaceCallback("Icon 1.5 Player", DeserializeAsInterface);
        SystemTokenRegistry.RegisterSimpleCallback("Icon 1.5 Player|AddTokenModal", AddTokenModal);
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
        return "UITemplates/GameSystem/Overheads/Icon1x5";
    }

    public override MenuItem[] GetTokenMenuItems(bool placed)
    {
        MenuItem[] baseItems = base.GetTokenMenuItems(placed);

        List<MenuItem> items = new();
        items.Add(new MenuItem("AttackRoll", "Attack Roll", AttackRollClicked));
        items.Add(new MenuItem("SaveRoll", "Save Roll", SaveRollClicked));
        return baseItems.Concat(items.ToArray()).ToArray();
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
        Token.Deselect();
        Modal.Close();
    }

    private void AttackRoll(ClickEvent evt)
    {
        string name = Token.GetSelected().Data.Name;
        int power = UI.Modal.Q<NumberNudger>("PowerField").value;
        string op = power > 0 ? "max" : "min";
        DiceRoller.DirectDieRoll(op, $"{Math.Abs(power) + 1}d10", $"{name}'s attack roll");
        Token.Deselect();
        Modal.Close();
    }


    public override void HandleCommand(string command, TokenData tokenData)
    {
        base.HandleCommand(command, tokenData);
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

        UI.ToggleDisplay(o.Q("HpBar"), CurrentHP > 0);
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

        mainHPBar.Q<Label>("VIG").text = $"+{Vigor}";
        mainHPBar.Q<ProgressBar>("VigorBar").value = Vigor;
        mainHPBar.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        UI.ToggleDisplay(mainHPBar.Q("VigorBar"), Vigor > 0);
        UI.ToggleDisplay(mainHPBar.Q("VIG"), Vigor > 0);

        UI.ToggleDisplay(mainHPBar.Q("Wound1"), Wounds >= 1);
        UI.ToggleDisplay(mainHPBar.Q("Wound2"), Wounds >= 2);
        UI.ToggleDisplay(mainHPBar.Q("Wound3"), Wounds >= 3);

    }

    public override void InitTokenPanel(string elementName, bool selected)
    {
        base.InitTokenPanel(elementName, selected);

        VisualElement panel = UI.System.Q(elementName);
        VisualElement hpBar = UI.CreateFromTemplate("UITemplates/GameSystem/IconHPBar");
        hpBar.name = "MainHPBar";
        panel.Q("Bars").Add(hpBar);

        VisualElement s1 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s1.Q<Label>("Label").text = "DMG/FRAY";
        s1.Q<Label>("Value").text = $"{Damage}/{Fray}";
        panel.Q("Stats").Add(s1);

        VisualElement s2 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s2.Q<Label>("Label").text = "RNG";
        s2.Q<Label>("Value").text = $"{Range}";
        panel.Q("Stats").Add(s2);

        VisualElement s3 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s3.Q<Label>("Label").text = "SPD/DASH";
        s3.Q<Label>("Value").text = $"{Speed}/{Dash}";
        panel.Q("Stats").Add(s3);

        VisualElement s4 = UI.CreateFromTemplate("UITemplates/GameSystem/StatTemplate");
        s4.Q<Label>("Label").text = "DEF";
        s4.Q<Label>("Value").text = $"{Defense}";
        panel.Q("Stats").Add(s4);
    }

    public static void AddTokenModal()
    {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> playerJobs = new();
        foreach (JSONNode pjob in gamedata["Icon1_5"]["PlayerJobs"].AsArray)
        {
            playerJobs.Add(pjob);
        }

        Modal.AddMarkup("Description", "ICON 1.5 Player tokens derive their stats from Icon1_5 data in the ruleset file.");
        Modal.AddTokenField("TokenSearchField");
        Modal.AddTextField("NameField", "Token Name", "Token");
        Modal.AddSearchField("PlayerJob", "Job", "Stalwart/Bastion", playerJobs.ToArray());

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
        string pclass = playerJob.Split("/")[0];
        string job = playerJob.Split("/")[1];
        string color = GetStatColor(pclass); // hate this function, ought to be in ruledata
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        JSONNode stats = gamedata["Icon1_5"]["Stats"][color];

        Icon1x5PlayerToken t = new()
        {
            System = "Icon 1.5 Player",
            Name = name,
            Job = job,
            Class = pclass,
            MaxHP = stats["MaxHP"],
            CurrentHP = stats["MaxHP"],
            Vigor = 0,
            Wounds = 0,
            Damage = stats["Damage"],
            Fray = stats["Fray"],
            Range = stats["Range"],
            Speed = stats["Speed"],
            Dash = stats["Dash"],
            Defense = stats["Defense"],
            Color = ColorUtility.GetCommonColor(color),
            TokenMeta = TokenLibrary.GetSelectedMeta()
        };

        AddToken.FinalizeToken(t.Serialize());
    }

    public static ISystemToken DeserializeAsInterface(string json)
    {
        return JsonUtility.FromJson<Icon1x5PlayerToken>(json);
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

    #endregion
}