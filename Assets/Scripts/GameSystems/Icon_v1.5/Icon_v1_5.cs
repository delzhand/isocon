using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.Data.Common;
using IsoconUILibrary;
using SimpleJSON;

public class Icon_v1_5 : GameSystem
{
    public static int TurnNumber = 1;
    public static int PartyResolve = 0;

    public override string SystemName()
    {
        return "ICON 1.5";
    }

    public override void Setup()
    {
        base.Setup();

        VisualElement selectedPanel = UI.CreateFromTemplate("UITemplates/GameSystem/IconUnitPanel");
        selectedPanel.Q("Damage").Q<Label>("Label").text = "DMG/FRAY";
        selectedPanel.Q("Range").Q<Label>("Label").text = "RNG";
        selectedPanel.Q("Speed").Q<Label>("Label").text = "SPD/DASH";
        selectedPanel.Q("Defense").Q<Label>("Label").text = "DEF";
        selectedPanel.Q<Button>("AlterVitals").RegisterCallback<ClickEvent>(AlterVitalsModal);
        selectedPanel.Q<Button>("AddStatus").RegisterCallback<ClickEvent>(AddStatusModal);
        UI.System.Q("SelectedTokenPanel").Q("Data").Add(selectedPanel);

        VisualElement focusedPanel = UI.CreateFromTemplate("UITemplates/GameSystem/IconUnitPanel");
        focusedPanel.Q("Damage").Q<Label>("Label").text = "DMG/FRAY";
        focusedPanel.Q("Range").Q<Label>("Label").text = "RNG";
        focusedPanel.Q("Speed").Q<Label>("Label").text = "SPD/DASH";
        focusedPanel.Q("Defense").Q<Label>("Label").text = "DEF";
        UI.System.Q("FocusedTokenPanel").Q("Data").Add(focusedPanel);
    }

    public override string GetTokenDataRawJson() {
        return Icon_v1_5TokenDataRaw.ToJson();
    }

    public override void GameDataSetValue(string value) {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn") {
            TurnNumber++;
            PartyResolve++;
            // Todo: update UI turn number
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                Icon_v1_5TokenData data = g.GetComponent<Icon_v1_5TokenData>();
                data.Change("LoseStatus|TurnEnded");
            }
        }
        if (value.StartsWith("GainPRES")) {
            int diff = int.Parse(value.Split("|")[1]);
            PartyResolve+=diff;
        }
    }

    public override Texture2D GetGraphic(string json) {
        Icon_v1_5TokenDataRaw raw = JsonUtility.FromJson<Icon_v1_5TokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenDataSetup(GameObject g, string json, string id) {
        g.GetComponent<Icon_v1_5TokenData>().TokenDataSetup(json, id);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/Icon_v1_5TokenData"));
    }

    public override void UpdateTokenPanel(GameObject data, string elementName)
    {
        if (data == null) {
            UI.ToggleDisplay(elementName, false);
            return;
        }
        UI.ToggleDisplay(elementName, true);
        data.GetComponent<Icon_v1_5TokenData>().UpdateTokenPanel(elementName);
    }

    public override string[] GetEffectList() {
        return new string[]{"Difficult", "Pit", "Dangerous", "Impassable", "Interactive", "Demon Slayer/Flash Step - Afterimage", "Demon Slayer/Six Hells Trigram", "Demon Slayer/Heroic Six Hells Trigram", "Fool/Party Favor", "Freelancer/Showdown - Quench", "Freelancer/Warding Bolts", "Shade/Shadow Cloud (Blinded+ exc Caster)", "Harvester/Plant", "Harvester/Blood Grove", "Harvester/Mote of Life (Blessing))", "Harvester/Mote of Life (Regen)", "Spellblade/Lightning Spike 1", "Spellblade/Lightning Spike 2", "Spellblade/Lightning Spike 3", "Spellblade/Lightning Spike 4", "Spellblade/Lightning Spike 5", "Spellblade/Lightning Spike 6", "Stormbender/Selkie", "Stormbender/Salt Sprite", "Stormbender/Pit", "Stormbender/Tsunami", "Stormbender/Tsunami - Stormlash", "Stormbender/Dangerous", "Stormbender/Geyser I", "Stormbender/Gust", "Stormbender/Gust I", "Stormbender/Gust II", "Stormbender/Waterspout", "Stormbender/Waterspout - Hurricane", "Stormbender/Waterspout I", "Stormbender/Waterspout I - Hurricane", "Stormbender/Waterspout II", "Stormbender/Waterspout II - Hurricane"};
    }

    public override bool HasEffect(string search, List<string> effects)
    {
        switch (search) {
            case "Blocked":
                return effects.Contains("Impassable");
            case "Spiky":
                return effects.Contains("Dangerous");
            case "Wavy":
                return effects.Contains("Difficult");
            case "Hand":
                return effects.Contains("Interactive");
            case "Hole":
                return effects.Contains("Pit");
            default:
                return false;
        }
    }

    public override bool HasCustomEffect(List<string> effects)
    {
        List<string> specialEffects = new string[]{"Impassable", "Dangerous", "Difficult", "Interactive", "Pit"}.ToList();
        foreach (string s in effects) {
            if (!specialEffects.Contains(s)) {
                return true;
            }
        }
        return false;
    }

    public override void AddTokenModal()
    {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> playerJobs = new();
        foreach (JSONNode pjob in gamedata["Icon1_5"]["PlayerJobs"].AsArray) {
            playerJobs.Add(pjob);
        }
        List<string> foeClasses = new();
        foreach (JSONNode fclass in gamedata["Icon1_5"]["FoeClasses"].AsArray) {
            foeClasses.Add(fclass);
        }

        base.AddTokenModal();

        Modal.AddDropdownField("Type", "Type", "Player", new string[]{"Player", "Foe", "Object"}, (evt) => AddTokenModalEvaluateConditions());

        Modal.AddSearchField("PlayerJob", "Job", "Stalwart/Bastion", playerJobs.ToArray());

        Modal.AddDropdownField("FoeClass", "Class", foeClasses[0], foeClasses.ToArray(), (evt) => AddTokenModalEvaluateConditions());

        Modal.AddTextField("FoeJob", "Job", "");

        Modal.AddToggleField("Elite", "Elite", false);

        Modal.AddDropdownField("LegendHP", "Legend HP Multiplier", "x4", new string[]{"x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8"});

        Modal.AddDropdownField("Size", "Size", "1x1", new string[]{"1x1", "2x2", "3x3"});

        Modal.AddIntField("ObjectHP", "Object HP", 1);

        AddTokenModalEvaluateConditions();
    }

    private static void AddTokenModalEvaluateConditions() {
        VisualElement modal = Modal.Find();

        bool playerJob = modal.Q<DropdownField>("Type").value == "Player";
        bool foeClass = modal.Q<DropdownField>("Type").value == "Foe";
        bool foeJob = modal.Q<DropdownField>("Type").value == "Foe";
        bool elite = foeClass && !StringUtility.InList(modal.Q<DropdownField>("FoeClass").value, "Legend", "Mob");
        bool legendHP = foeClass && modal.Q<DropdownField>("FoeClass").value == "Legend";
        bool size = foeClass;
        bool objectHP = modal.Q<DropdownField>("Type").value == "Object";

        UI.ToggleDisplay(modal.Q("PlayerJob"), playerJob);
        UI.ToggleDisplay(modal.Q("FoeClass"), foeClass);
        UI.ToggleDisplay(modal.Q("FoeJob"), foeJob);
        UI.ToggleDisplay(modal.Q("Elite"), elite);
        UI.ToggleDisplay(modal.Q("LegendHP"), legendHP);
        UI.ToggleDisplay(modal.Q("Size"), size);
        UI.ToggleDisplay(modal.Q("ObjectHP"), objectHP);
    }

    private static void AlterVitalsModal(ClickEvent evt) {
        Modal.Reset("Alter Vitals");
        Modal.AddIntField("Number", "Value", 0);
        Modal.AddButton("Damage HP/VIG", (evt) => AlterVitals("Damage"));
        Modal.AddButton("Reduce HP", (evt) => AlterVitals("LoseHP"));
        Modal.AddButton("Recover HP", (evt) => AlterVitals("GainHP"));
        Modal.AddButton("Reduce VIG", (evt) => AlterVitals("LoseVIG"));
        Modal.AddButton("Recover VIG", (evt) => AlterVitals("GainVIG"));
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void AlterVitals(string cmd) {
        int val = Modal.Find().Q<IntegerField>("Number").value;
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelectedData().GetComponent<TokenData>(), $"{cmd}|{val}");
    }

    private static void AddStatusModal(ClickEvent evt) {
        Modal.Reset("Add Status");
        Modal.AddDropdownField("Type", "Type", "Predefined", StringUtility.Arr("Predefined", "Simple", "Counter"), (evt) => AddStatusModalEvaluateConditions());
        Modal.AddSearchField("PregenStatuses", "Status", "", StringUtility.Arr("Aether","Armor","Blessing","Blind","Blind+","Counter","Cover","Dark Knight","Dazed","Dazed+",
            "Defiance","Dodge","Endless Battlement","Evasion","Evasion+","Flying","Gentleness","Gravebirth","Odinforce","Pacified","Pacified+","Phasing","Regeneration","Riposte",
            "Sealed","Sealed+","Shattered","Shattered+","Slashed","Slashed+","Soul Blade","Stacked Dice","Stealth","Stunned","Stunned+","Sturdy","The Saints","Unstoppable",
            "Vigilance","Vulnerable","Vulnerable+","Weakened","Weakened+"
        ));
        Modal.AddDropdownField("Color", "Color", "Gray", StringUtility.Arr("Gray", "Green", "Red", "Blue", "Purple", "Yellow", "Orange"));
        Modal.AddPreferredButton("Add", (evt) => {
            string pregenStatus = SearchField.GetValue(Modal.Find().Q("PregenStatuses"));
            Player.Self().CmdRequestTokenDataSetValue(Token.GetSelectedData().GetComponent<TokenData>(), $"GainStatus|{pregenStatus}");
        });
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private static void AddStatusModalEvaluateConditions() {
        VisualElement modal = Modal.Find();

        bool pregenStatus = modal.Q<DropdownField>("Type").value == "Predefined";
        bool color = modal.Q<DropdownField>("Type").value != "Predefined";

        UI.ToggleDisplay(modal.Q("PregenStatuses"), pregenStatus);
        UI.ToggleDisplay(modal.Q("Color"), color);
    }

    private static void AddStatus(ClickEvent evt) {
        
    }
}
