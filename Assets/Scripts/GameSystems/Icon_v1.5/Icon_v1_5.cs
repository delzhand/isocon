using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;

public class Icon_v1_5 : GameSystem
{
    public override string SystemName()
    {
        return "ICON 1.5";
    }

    public override string GetTokenData() {
        return Icon_v1_5TokenDataRaw.ToJson();
    }

    public override void TokenDataSetValue(TokenData data, string label, int value)
    {
        (data as Icon_v1_5TokenData).Change(label, value);
    }


    public override Texture2D GetGraphic(string json) {
        Icon_v1_5TokenDataRaw raw = JsonUtility.FromJson<Icon_v1_5TokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenDataSetup(GameObject g, string json) {
        g.GetComponent<Icon_v1_5TokenData>().TokenDataSetup(json);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/Icon_v1_5TokenData"));
    }

    public override void Setup() {
        AddTokenSetup();
        SetJobOptions("Stalwart");
    }

    public override void Teardown() {
        AddTokenTeardown();
    }

    public override void UpdateSelectedTokenPanel(GameObject data)
    {
        data.GetComponent<Icon_v1_5TokenData>().UpdateSelectedTokenPanel();
    }

    private void AddTokenSetup() {
        UI.ToggleDisplay("AddTokenSystem", true);
        VisualElement root = UI.System.Q("AddTokenSystem");
        List<string> classes = GetClasses();
        root.Q<DropdownField>("ClassDropdown").choices = classes;
        root.Q<DropdownField>("ClassDropdown").value = classes[0];
        root.Q<DropdownField>("ClassDropdown").RegisterValueChangedCallback(SetJobOptions);
    }

    private void AddTokenTeardown() {
        UI.ToggleDisplay("AddTokenSystem", false);
        VisualElement root = UI.System.Q("AddTokenSystem");
        root.Q<DropdownField>("ClassDropdown").UnregisterValueChangedCallback(SetJobOptions);
    }

    private void SetJobOptions(ChangeEvent<string> evt) {
        SetJobOptions(evt.newValue);
    }

    private void SetJobOptions(string jclass) {
        List<string> jobs = GetJobs(jclass);
        UI.System.Q<DropdownField>("JobDropdown").choices = jobs;
        UI.System.Q<DropdownField>("JobDropdown").value = jobs[0];

        UI.ToggleDisplay("EliteToggle", IsFoe(jclass));
        UI.ToggleDisplay("SizeDropdown", IsFoe(jclass) || jclass == "Object");
        UI.ToggleDisplay("LegendHPDropdown", jclass == "Legend");
        UI.ToggleDisplay("ObjectHPField", jclass == "Object");
    }

    private static bool IsFoe(string jclass) {
        return jclass switch
        {
            "Stalwart" or "Wright" or "Mendicant" or "Vagabond" or "Object" => false,
            _ => true,
        };
    }

    private List<string> GetClasses() {
        return new string[]{"Stalwart", "Vagabond", "Mendicant", "Wright", "Heavy", "Skirmisher","Leader","Artillery","Legend","Mob","Object"}.ToList();
    }

    private List<string> GetJobs(string jclass) {
        return jclass switch
        {
            "Stalwart" => new string[] { "Bastion", "Demon Slayer", "Knave", "Colossus" }.ToList(),
            "Vagabond" => new string[] { "Shade", "Freelancer", "Fool", "Warden" }.ToList(),
            "Mendicant" => new string[] { "Seer", "Chanter", "Sealer", "Harvester" }.ToList(),
            "Wright" => new string[] { "Enochian", "Geomancer", "Spellblade", "Stormbender" }.ToList(),
            "Heavy" => new string[] { "Warrior", "Soldier", "Impaler", "Greatsword", "Brute", "Knuckle", "Sentinel", "Crusher", "Berserker", "Sledge" }.ToList(),
            "Skirmisher" => new string[] { "Pepperbox", "Hunter", "Fencer", "Assassin", "Hellion", "Skulk", "Shadow", "Arsonist" }.ToList(),
            "Leader" => new string[] { "Errant", "Priest", "Commander", "Aburer", "Diviner", "Greenseer", "Judge", "Saint", "Cantrix" }.ToList(),
            "Artillery" => new string[] { "Blaster", "Seismatist", "Storm Caller", "Rift Dancer", "Disruptor", "Chaos Wright", "Scourer", "Sapper", "Justicar", "Sniper", "Alchemist" }.ToList(),
            "Legend" => new string[] { "Demolisher", "Nocturnal", "Master", "Razer" }.ToList(),
            "Mob" => new string[] { "Mob" }.ToList(),
            "Object" => new string[] { "Destructible" }.ToList(),
            _ => new string[] { }.ToList(),
        };
    }

}
