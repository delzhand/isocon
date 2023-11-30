using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.Data.Common;
using IsoconUILibrary;

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
        return effects.Count > 0 && 
            !HasEffect("Difficult", effects) &&
            !HasEffect("Dangerous", effects) &&
            !HasEffect("Interactive", effects) &&
            !HasEffect("Pit", effects) &&
            !HasEffect("Impassable", effects);
    }

    public override void AddTokenModal()
    {
        base.AddTokenModal();

        Modal.AddDropdownField("Job", "Job", "Stalwart/Bastion", new string[]{
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
            "Wright/Stormbender",
            }, null);

        // DropdownField sizeField = new DropdownField("Size");
        // sizeField.choices = new List<string>(){"1x1", "2x2", "3x3"};
        // sizeField.value = "1x1";
        // sizeField.name = "SizeField";
        // sizeField.focusable = false;
        // sizeField.AddToClassList("no-margin");
        // Modal.AddContents(sizeField);
    }


}
