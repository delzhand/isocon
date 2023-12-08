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
            UI.System.Q<Label>("TurnNumber").text = TurnNumber.ToString();
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

        Modal.AddDropdownField("LegendHP", "Legend HP Multiplier", "x4", new string[]{"x2", "x3", "x4", "x5", "x6", "x7", "x8"});

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
        Modal.AddDropdownField("Type", "Type", "Predefined", StringUtility.Arr("Predefined", "Simple", "Number", "Detail"), (evt) => AddStatusModalEvaluateConditions());

        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> statuses = new();
        foreach (JSONNode s in gamedata["Icon1_5"]["StatusEffects"].AsArray) {
            statuses.Add(s["Name"]);
        }
        Modal.AddSearchField("PregenStatuses", "Status", "", statuses.ToArray());
        Modal.AddTextField("Name", "Name", "");
        Modal.AddDropdownField("Color", "Color", "Gray", StringUtility.Arr("Gray", "Green", "Red", "Blue", "Purple", "Yellow", "Orange"));
        Modal.AddIntField("Number", "Number", 0);
        Modal.AddTextField("Detail", "Detail", "");
        Modal.AddPreferredButton("Add", AddStatus);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        AddStatusModalEvaluateConditions();
    }

    private static void AddStatusModalEvaluateConditions() {
        VisualElement modal = Modal.Find();

        bool pregenStatus = modal.Q<DropdownField>("Type").value == "Predefined";
        bool name = !pregenStatus;
        bool color = !pregenStatus;
        bool number = modal.Q<DropdownField>("Type").value == "Number";
        bool detail = modal.Q<DropdownField>("Type").value == "Detail";

        UI.ToggleDisplay(modal.Q("PregenStatuses"), pregenStatus);
        UI.ToggleDisplay(modal.Q("Name"), name);
        UI.ToggleDisplay(modal.Q("Color"), color);
        UI.ToggleDisplay(modal.Q("Number"), number);
        UI.ToggleDisplay(modal.Q("Detail"), detail);
    }

    private static void AddStatus(ClickEvent evt) {
        VisualElement modal = Modal.Find();
        string type = modal.Q<DropdownField>("Type").value;
        string pregenStatus = SearchField.GetValue(Modal.Find().Q("PregenStatuses"));
        string customStatus = modal.Q<TextField>("Name").value;
        string color = modal.Q<DropdownField>("Color").value;
        // string detail = modal.Q<TextField>("Detail").value;
        int number = modal.Q<IntegerField>("Number").value;
        StatusEffect s;
        if (type == "Predefined") {
            s = FindStatusEffect(pregenStatus);
        }
        else {
            s = new StatusEffect() {
                Name = customStatus,
                Type = type,
                Color = color,
                Number = number
            };
        }
        
        // Strip characters that would break parse
        s.Name = s.Name.Replace("|", "");

        string statusData = $"{s.Name}|{s.Type}|{s.Color}|{s.Number}";

        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelectedData().GetComponent<TokenData>(), $"GainStatus|{statusData}");        
        Modal.Close();
    }

    private static StatusEffect FindStatusEffect(string name) {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        foreach (JSONNode s in gamedata["Icon1_5"]["StatusEffects"].AsArray) {
            if (s["Name"] == name) {
                return new StatusEffect() {
                    Name = s["Name"],
                    Color = s["Color"],
                    Type = s["Type"]
                };
            }
        }
        throw new Exception("Status Effect not found");
    }
}
