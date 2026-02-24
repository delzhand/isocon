using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        // items.Add(new MenuItem("AttackRoll", "Attack Roll", AttackRollClicked));
        // items.Add(new MenuItem("SaveRoll", "Save Roll", SaveRollClicked));
        return baseItems.Concat(items.ToArray()).ToArray();
    }

    public override void HandleCommand(string command, TokenData tokenData)
    {
        // base.HandleCommand(command, tokenData);
        // if (command.StartsWith("GainHeat|"))
        // {
        //     GainHeat(command, tokenData);
        // }
        // if (command.StartsWith("LoseStructure|"))
        // {
        //     LoseStructure(command, tokenData);
        // }
        // if (command.StartsWith("LoseStress|"))
        // {
        //     LoseStress(command, tokenData);
        // }
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