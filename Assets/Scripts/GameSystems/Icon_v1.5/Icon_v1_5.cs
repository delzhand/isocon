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
    public int TurnNumber = 1;
    public int PartyResolve = 0;

    public override string SystemName()
    {
        return "ICON 1.5";
    }

    public override string GetTokenDataRawJson() {
        return Icon_v1_5TokenDataRaw.ToJson();
    }

    public override void GameDataSetValue(string label, int value) {
        switch (label) {
            case "TurnNumber":
                // TurnNumber = value;
                // UI.System.Q<Label>("TurnNumber").text = $"Turn {TurnNumber}";
                // foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                //     Icon_v1_5TokenData data = g.GetComponent<Icon_v1_5TokenData>();
                //     if (data.CheckCondition("TurnEnded")) {
                //         data.Change("Status", "Turn Ended|neu");
                //     }
                // }
                break;
            case "PartyResolve":
                // PartyResolve = value;
                // Token selected = Token.GetSelected();
                // if (selected != null) {
                //     TokenData selectedData = selected.onlineDataObject.GetComponent<TokenData>();
                //     Player.Self().CmdRequestTokenDataSetValue(selectedData, "PartyResolve", PartyResolve);
                // }
                break;
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
        bool elite = foeClass && !(new string[]{"Legend", "Mob"}.ToList().Contains(modal.Q<DropdownField>("FoeClass").value));
        bool legendHP = foeClass && modal.Q<DropdownField>("FoeClass").value == "Legend";
        bool size = foeClass;
        bool objectHP = modal.Q<DropdownField>("Type").value == "Object";

        UI.ToggleDisplay(Modal.Find().Q("PlayerJob"), playerJob);
        UI.ToggleDisplay(Modal.Find().Q("FoeClass"), foeClass);
        UI.ToggleDisplay(Modal.Find().Q("Elite"), elite);
        UI.ToggleDisplay(Modal.Find().Q("LegendHP"), legendHP);
        UI.ToggleDisplay(Modal.Find().Q("Size"), size);
        UI.ToggleDisplay(Modal.Find().Q("ObjectHP"), objectHP);
    }

}
