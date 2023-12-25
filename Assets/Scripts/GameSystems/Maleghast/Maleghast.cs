using System;
using System.Collections.Generic;
using System.Reflection;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

public class Maleghast : GameSystem
{
    public override string SystemName()
    {
        return "Maleghast";
    }

    public override void InterpreterMethod(string name, object[] args)
    {
        Type classType = Type.GetType("MaleghastInterpreter");
        MethodInfo method = classType.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
        method.Invoke(null, args);
    }

    public override void Setup() {
        SetupPanel("SelectedTokenPanel", true);
        SetupPanel("FocusedTokenPanel", false);
    }

    private void SetupPanel(string elementName, bool editable) {
        VisualElement panel = UI.System.Q(elementName);
        VisualElement unitPanel = UI.CreateFromTemplate("UITemplates/GameSystem/MaleghastUnitPanel");
        unitPanel.Q("SOUL").Q<ProgressBar>("HpBar").value = 0;
        unitPanel.Q("SOUL").Q<ProgressBar>("HpBar").highValue = 6;
        unitPanel.Q("SOUL").Q<Label>("StatLabel").text = "SOUL";
        
        unitPanel.Q("Type").Q<Label>("Label").text = "TYPE";
        unitPanel.Q("Defense").Q<Label>("Label").text = "DEFENSE";
        unitPanel.Q("Move").Q<Label>("Label").text = "MOVE";
        unitPanel.Q("Armor").Q<Label>("Label").text = "ARMOR";
        if (editable) {
            // unitPanel.Q<Button>("AlterVitals").RegisterCallback<ClickEvent>(AlterVitalsModal);
            // unitPanel.Q<Button>("AddStatus").RegisterCallback<ClickEvent>(AddStatusModal);
            // unitPanel.Q<Button>("Upgrades").RegisterCallback<ClickEvent>(UpgradesModal);
        }
        panel.Q("Data").Add(unitPanel);
        panel.Q("ExtraInfo").Add(new Label(){ name = "House" });
        panel.Q("ExtraInfo").Add(new Label(){ name = "Job" });
    }

    public override void GameDataSetValue(string value) {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn") {
            TurnNumber++;
            UI.System.Q<Label>("TurnNumber").text = TurnNumber.ToString();
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                TokenData2 data = g.GetComponent<TokenData2>();
                TokenDataSetValue(data.Id, "LoseStatus|Turn Ended");
            }
        }
    }

    public override string[] GetEffectList() {
        return new string[]{"Adverse", "Hazard", "Impassable", "Corpse"};
    }


    public override void AddTokenModal()
    {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> units = new();
        List<string> houses = new();
        foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray) {
            houses.Add(house["name"]);
            foreach (JSONNode unit in house["units"].AsArray) {
                string houseJob = $"{ house["name"] }/{ unit["name"] }";
                units.Add(houseJob.Replace("\"", ""));
            }
        }

        base.AddTokenModal();

        Modal.AddSearchField("UnitType", "Unit Type", "", units.ToArray());
        Modal.AddDropdownField("PlayerColor", "Player Color", "House Default", houses.ToArray());
    }
}

[Serializable]
public class MaleghastData {
    public int CurrentHP;
    public int MaxHP;
    public int Soul;
    public string House;
    public string Type;
    public string Job;
    public int Move;
    public int Defense;
    public string Armor;
    public string[] Traits;
    public string[] ActAbilities;
    public string[] SoulAbilities;
    public string[] Upgrades;
    public string[] StatusTokens;
}

public class MaleghastInterpreter {

    public static void CreateToken() {
        string name = UI.Modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(UI.Modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        string graphicHash = TextureSender.GetTextureHash(graphic);

        string houseJob = SearchField.GetValue(UI.Modal.Q("UnitType"));
        string house = houseJob.Split("/")[0];
        string job = houseJob.Split("/")[1];
        string colorValue = UI.Modal.Q<DropdownField>("PlayerColor").value;

        MaleghastData data = new(){
            House = house,
            Job = job
        };
        InitSystemData(data);
        
        int size = 1;
        if (data.Type == "Tyrant") {
            size = 2;
        }

        Color color = ColorUtility.ColorFromHex(FindHouse(house)["color"]);
        if (colorValue != "House Default") {
            color = ColorUtility.ColorFromHex(FindHouse(colorValue)["color"]);
        }

        Player.Self().CmdCreateToken("Maleghast", graphicHash, name, size, color, JsonUtility.ToJson(data));
    }

    public static void UpdateData(TokenData2 data) {
        MaleghastData mdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);
        data.OverheadElement.Q<ProgressBar>("HpBar").value = mdata.CurrentHP;
        data.OverheadElement.Q<ProgressBar>("HpBar").highValue = mdata.MaxHP;        
    }

    private static void InitSystemData(MaleghastData data) {
        JSONNode job = FindJob(data.House, data.Job);
        data.Type = job["type"];
        data.Move = job["move"];
        data.MaxHP = job["hp"];
        data.CurrentHP = job["hp"];
        data.Defense = job["def"];
        data.Armor = job["armor"];
        string traits = job["traits"];
        data.Traits = traits.Split("|");
        string actAbilities = job["actAbilities"];
        data.ActAbilities = actAbilities.Split("|");
        string soulAbilities = job["soulAbilities"];
        if (soulAbilities != null) {
            data.SoulAbilities = soulAbilities.Split("|");
        }
    }

    private static JSONNode FindHouse(string searchHouse) {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray) {
            if (house["name"] == searchHouse) {
                return house;
            }
        }
        return null;
    }

    private static JSONNode FindJob(string searchHouse, string searchJob) {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray) {
            if (house["name"] == searchHouse) {
                foreach (JSONNode unit in house["units"].AsArray) {
                    if (unit["name"] == searchJob) {
                        return unit;
                    }
                }
            }
        }
        return null;
    }

    public static void Change(string tokenId, string value) {
        TokenData2 data = TokenData2.Find(tokenId);
        Debug.Log($"MaleghastInterpreter change registered for {data.Name}: {value}");
    }

    public static void UpdateTokenPanel(string tokenId, string elementName) {
        TokenData2 data = TokenData2.Find(tokenId);
        UI.ToggleActiveClass(elementName, data != null);
        if (!data) {
            return;
        }

        data.UpdateTokenPanel(elementName);
        MaleghastData sysdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);

        VisualElement panel = UI.System.Q(elementName);

        panel.Q("ClassBackground").style.borderTopColor = data.Color;
        panel.Q("ClassBackground").style.borderRightColor = data.Color;
        panel.Q("ClassBackground").style.borderBottomColor = data.Color;
        panel.Q("ClassBackground").style.borderLeftColor = data.Color;

        panel.Q<Label>("House").text = sysdata.House;
        panel.Q<Label>("House").style.backgroundColor = data.Color;
        panel.Q<Label>("Job").text = sysdata.Job;
        panel.Q<Label>("Job").style.backgroundColor = data.Color;

        panel.Q("HP").Q<Label>("CHP").text = $"{ sysdata.CurrentHP }";
        panel.Q("HP").Q<Label>("MHP").text = $"/{ sysdata.MaxHP }";
        panel.Q("HP").Q<ProgressBar>("HpBar").value = sysdata.CurrentHP;
        panel.Q("HP").Q<ProgressBar>("HpBar").highValue = sysdata.MaxHP;

        panel.Q("SOUL").Q<Label>("CHP").text = $"{ sysdata.Soul }";
        panel.Q("SOUL").Q<Label>("MHP").text = $"/6";
        panel.Q("SOUL").Q<ProgressBar>("HpBar").value = sysdata.Soul;
        panel.Q("SOUL").Q<ProgressBar>("HpBar").highValue = 6;

        panel.Q("Defense").Q<Label>("Value").text = $"{ sysdata.Defense }";
        panel.Q("Move").Q<Label>("Value").text = $"{ sysdata.Move }";
        panel.Q("Armor").Q<Label>("Value").text = $"{ sysdata.Armor.ToUpper() }";
        panel.Q("Type").Q<Label>("Value").text = (sysdata.Type == "Necromancer") ? "NECRO" : sysdata.Type.ToUpper();

        panel.Q("Traits").Clear();
        foreach (string s in sysdata.Traits) {
            if (!s.StartsWith("-")) {
                panel.Q("Traits").Add(new Label(){text = s});
            }
        }

        panel.Q("ACTAbilities").Clear();
        foreach (string s in sysdata.ActAbilities) {
            if (!s.StartsWith("-")) {
                panel.Q("ACTAbilities").Add(new Label(){text = s});
            }
        }

        panel.Q("SOULAbilities").Clear();
        foreach (string s in sysdata.SoulAbilities) {
            if (!s.StartsWith("-")) {
                panel.Q("SOULAbilities").Add(new Label(){text = s});
            }
        }

        panel.Q("Conditions").Clear();
        panel.Q("Conditions").Add(new Label(){text = "Flight"});
        panel.Q("Conditions").Add(new Label(){text = "Madness"});
    
        panel.Q("Tokens").Clear();
        panel.Q("Tokens").Add(new Label(){text = "Strength"});
        panel.Q("Tokens").Add(new Label(){text = "Strength"});
        panel.Q("Tokens").Add(new Label(){text = "Vulnerable"});

        UI.ToggleDisplay(panel.Q("SOUL"), sysdata.Type == "Necromancer");
        UI.ToggleDisplay(panel.Q("SOULAbilities"), sysdata.Type == "Necromancer");
    }


}