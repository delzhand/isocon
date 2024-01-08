using System;
using System.Collections.Generic;
using System.Linq;
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

    public override void Setup() {
        base.Setup();
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
        if (editable) {
            unitPanel.Q<Button>("AlterVitals").RegisterCallback<ClickEvent>(AlterVitalsModal);
            unitPanel.Q<Button>("EditConfig").RegisterCallback<ClickEvent>(ConfigModal);
            unitPanel.Q<Button>("EditStatus").RegisterCallback<ClickEvent>(EditStatusModal);
        }
        panel.Q("Data").Add(unitPanel);
        panel.Q("ExtraInfo").Add(new Label(){ name = "House" });
        panel.Q("ExtraInfo").Add(new Label(){ name = "Job" });
    }

    public override void Teardown()
    {
        TeardownPanel("SelectedTokenPanel");
        TeardownPanel("FocusedTokenPanel");
    }

    private void TeardownPanel(string elementName) {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("ExtraInfo").Clear();
        panel.Q("Data").Clear();
    }

    public override void GameDataSetValue(string value) {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn") {
            TurnNumber++;
            UI.System.Q<Label>("TurnNumber").text = TurnNumber.ToString();
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                TokenData data = g.GetComponent<TokenData>();
                TokenDataSetValue(data.Id, "StartTurn");
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

    private void AlterVitalsModal(ClickEvent evt) {
        Modal.Reset("Alter Vitals");
        Modal.AddIntField("Number", "Value", 0);
        UI.Modal.Q("Number").AddToClassList("big-number");
        Modal.AddContentButton("ReduceHP", "Reduce HP", (evt) => AlterVitals("LoseHP"));
        Modal.AddContentButton("RecoverHP", "Recover HP", (evt) => AlterVitals("GainHP"));
        TokenData data = Token.GetSelected().Data;
        MaleghastData sysdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);
        if (sysdata.Type == "Necromancer") {
            Modal.AddContentButton("ReduceSOUL", "Reduce SOUL", (evt) => AlterVitals("LoseSOUL"));
            Modal.AddContentButton("GainSOUL", "Gain SOUL", (evt) => AlterVitals("GainSOUL"));
        }
        Modal.AddButton("Done", Modal.CloseEvent);
    }

    private static void AlterVitals(string cmd) {
        int val = UI.Modal.Q<IntegerField>("Number").value;
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, $"{cmd}|{val}");
    }

    private void ConfigModal(ClickEvent evt) {
        Modal.Reset("Configure Unit");
        TokenData data = Token.GetSelected().Data;
        MaleghastData sysdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);
        ConfigModalSublist(sysdata.Upgrades, "Upgrade");
        if (sysdata.Type == "Necromancer") {
            Modal.AddColumns("NecroOptions", 3);
            ConfigModalSublist(sysdata.Traits, "Trait", "NecroOptions_0");
            ConfigModalSublist(sysdata.ActAbilities, "ACT", "NecroOptions_1");
            ConfigModalSublist(sysdata.SoulAbilities, "SOUL", "NecroOptions_2");

        }
        
        Modal.AddPreferredButton("Complete", Modal.CloseEvent);
    }

    private void ConfigModalSublist(string[] list, string listName, string reparent = "") {
        foreach (string s in list) {
            if (s.StartsWith("=")) {
                continue;
            }
            string itemName = s;
            bool enabled = true;
            if (itemName.StartsWith("-")) {
                itemName = itemName.Substring(1);
                enabled = false;
            }
            string id = $"{listName}_{itemName}";
            string label = $"{listName}: {itemName}";
            Modal.AddToggleField(id, label, enabled, RebuildLists);
            UI.Modal.Q(id).AddToClassList("reverse-toggle");
            if (reparent.Length > 0) {
                Modal.MoveToColumn(reparent, id);
            }
        }
    }

    private void RebuildLists(ChangeEvent<Boolean> evt) {
        TokenData data = Token.GetSelected().Data;
        MaleghastData sysdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);

        Toggle t = evt.currentTarget as Toggle;
        string[] split = t.name.Split("_");
        List<string> list;
        switch(split[0]) {
            case "Upgrade":
                list = sysdata.Upgrades.ToList();
                break;
            case "Trait":
                list = sysdata.Traits.ToList();
                break;
            case "ACT":
                list = sysdata.ActAbilities.ToList();
                break;
            case "SOUL":
                list = sysdata.SoulAbilities.ToList();
                break;
            default:
                list = new();
                break;
        }
        string oldVal = split[1];
        string newVal = split[1];
        if (evt.newValue) {
            oldVal = $"-{oldVal}";
        }
        else {
            newVal = $"-{oldVal}";
        }
        list.Remove(oldVal);
        list.Add(newVal);
        switch(split[0]) {
            case "Upgrade":
                sysdata.Upgrades = list.ToArray();
                break;
            case "Trait":
                sysdata.Traits = list.ToArray();
                break;
            case "ACT":
                sysdata.ActAbilities = list.ToArray();
                break;
            case "SOUL":
                sysdata.SoulAbilities = list.ToArray();
                break;
            default:
                break;
        }

        data.SystemData = JsonUtility.ToJson(sysdata);
    }

    private void EditStatusModal(ClickEvent evt) {
        TokenData data = Token.GetSelected().Data;
        MaleghastData sysdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);

        Modal.Reset("Alter Status");
        Modal.AddColumns("MaleghastStatus", 2);
        int i = 0;
        foreach (string s in GetTokens()) {
            string s2 = s.Replace(" ", "");
            Modal.AddNumberNudgerField($"Status{s2}", s, CollectionUtility.CountInArray(sysdata.Tokens, s), (evt) => {
                TokenChange(evt, s);
            });
            UI.Modal.Q($"MaleghastStatus_{i%2}").Add(UI.Modal.Q($"Status{s2}"));
            i++;
        }
        foreach (string s in GetStatuses()) {
            string s2 = s.Replace(" ", "");
            Modal.AddToggleField($"Status{s2}", s, CollectionUtility.CountInArray(sysdata.Conditions, s) > 0, (evt) => {
                StatusToggled(evt, s);
            });
            UI.Modal.Q($"MaleghastStatus_{i%2}").Add(UI.Modal.Q($"Status{s2}"));
            i++;
        }
        Modal.AddPreferredButton("Complete", Modal.CloseEvent);
    }

    private void StatusToggled(ChangeEvent<Boolean> evt, string status) {
        string value = $"GainStatus|{status}";
        if (!evt.newValue) {
            value = $"LoseStatus|{status}";
        }
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, value);
    }

    private void TokenChange(int evt, string token) {
        string value = $"SetToken|{token}|{evt}";
        Player.Self().CmdRequestTokenDataSetValue(Token.GetSelected().Data.Id, value);
    }

    private static string[] GetStatuses() {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> statuses = new();
        foreach (JSONNode status in gamedata["Maleghast"]["StatusEffects"]) {
            statuses.Add(status);
        }
        return statuses.ToArray();
    }    

    private static string[] GetTokens() {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> tokens = new();
        foreach (JSONNode status in gamedata["Maleghast"]["StatusTokens"]) {
            tokens.Add(status);
        }
        return tokens.ToArray();
    }  

    public override void CreateToken() {
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

    public override void UpdateData(TokenData data) {
        base.UpdateData(data);
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
        data.Conditions = new string[]{};
        data.Tokens = new string[]{};
        string upgrades = job["upgrades"];
        if (upgrades != null) {
            data.Upgrades = upgrades.Split("|");
        }
        string traits = job["traits"];
        if (traits != null) {
            data.Traits = traits.Split("|");
        }
        string actAbilities = job["actAbilities"];
        if (actAbilities != null) {
            data.ActAbilities = actAbilities.Split("|");
        }
        string soulAbilities = job["soulAbilities"];
        if (soulAbilities != null) {
            data.SoulAbilities = soulAbilities.Split("|");
        }
        string initConditions = job["conditions"];
        if (initConditions != null && initConditions.Length > 0) {
            data.Conditions = initConditions.Split("|");
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

    public override void TokenDataSetValue(string tokenId, string value) {
        base.TokenDataSetValue(tokenId, value);
        TokenData data = TokenData.Find(tokenId);
        Debug.Log($"MaleghastInterpreter change registered for {data.Name}: {value}");
        MaleghastData sysdata = JsonUtility.FromJson<MaleghastData>(data.SystemData);
        sysdata.Change(value, data.WorldObject.GetComponent<Token>(), data.Placed);
        data.SystemData = JsonUtility.ToJson(sysdata);  
    }

    public override void UpdateTokenPanel(string tokenId, string elementName) {
        TokenData data = TokenData.Find(tokenId);
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
        panel.Q("Type").Q<Label>("Value").text = (sysdata.Type == "Necromancer") ? "NECRO" : sysdata.Type.ToUpper();

        panel.Q("Traits").Clear();
        foreach (string s in sysdata.Traits) {
            if (!s.StartsWith("-")) {
                string s2 = s;
                if (s2.StartsWith("=")) {
                    s2 = s2.Substring(1);
                }
                panel.Q("Traits").Add(new Label(){text = s2});
            }
        }

        panel.Q("ACTAbilities").Clear();
        foreach (string s in sysdata.ActAbilities) {
            if (!s.StartsWith("-")) {
                string s2 = s;
                if (s2.StartsWith("=")) {
                    s2 = s2.Substring(1);
                }
                panel.Q("ACTAbilities").Add(new Label(){text = s2});
            }
        }

        panel.Q("SOULAbilities").Clear();
        foreach (string s in sysdata.SoulAbilities) {
            if (!s.StartsWith("-")) {
                string s2 = s;
                if (s2.StartsWith("=")) {
                    s2 = s2.Substring(1);
                }
                panel.Q("SOULAbilities").Add(new Label(){text = s2});
            }
        }

        panel.Q("Upgrades").Clear();
        foreach (string s in sysdata.Upgrades) {
            if (!s.StartsWith("-")) {
                string s2 = s;
                if (s2.StartsWith("=")) {
                    s2 = s2.Substring(1);
                }
                panel.Q("Upgrades").Add(new Label(){text = s2});
            }
        }

        panel.Q("Conditions").Clear();
        foreach (string s in sysdata.Conditions) {
            panel.Q("Conditions").Add(new Label(){text = s}); 
        }
        
        panel.Q("Tokens").Clear();
        Dictionary<string, int> combinedTokens = new();
        foreach (string s in sysdata.Tokens) {
            if (combinedTokens.Keys.Contains(s)) {
                combinedTokens[s]++;
            }
            else {
                combinedTokens.Add(s, 1);
            }
        }
        foreach (KeyValuePair<string,int> pair in combinedTokens){
            if (pair.Value != 0) {
                panel.Q("Tokens").Add(new Label(){text = $"{pair.Key} ({pair.Value})"});
            }
        }

        UI.ToggleDisplay(panel.Q("SOUL"), sysdata.Type == "Necromancer");
        UI.ToggleDisplay(panel.Q("SOULAbilities"), sysdata.Type == "Necromancer");
        UI.ToggleDisplay(panel.Q("SOULAbilitiesLabel"), sysdata.Type == "Necromancer");

        UI.ToggleDisplay(panel.Q("Upgrades"), sysdata.Type != "Necromancer");
        UI.ToggleDisplay(panel.Q("UpgradesLabel"), sysdata.Type != "Necromancer");

        UI.ToggleDisplay(panel.Q("Configuration"), sysdata.Type != "Object");
        UI.ToggleDisplay(panel.Q("Status"), sysdata.Type != "Object");
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
    public string[] Traits;
    public string[] ActAbilities;
    public string[] SoulAbilities;
    public string[] Upgrades;
    public string[] Conditions;
    public string[] Tokens;

    public void Change(string value, Token token, bool placed) {
        if (value.StartsWith("GainHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP + diff > MaxHP) {
                diff = MaxHP - CurrentHP;
            }
            if (diff > 0) {
                CurrentHP+=diff;
                if (placed) {
                    PopoverText.Create(token, $"/+{diff}|_HP", Color.white);
                }
            }
            OnVitalChange(token);
        }
        if (value.StartsWith("LoseHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP - diff < 0) {
                diff = CurrentHP;
            }
            if (diff > 0) {
                CurrentHP-=diff;
                if (placed) {
                    PopoverText.Create(token, $"/-{diff}|_HP", Color.white);
                }
            }
            OnVitalChange(token);
        }   
        if (value.StartsWith("GainSOUL")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (Soul + diff > 6) {
                diff = 6 - Soul;
            }
            if (diff > 0) {
                Soul+=diff;
                if (placed) {
                    PopoverText.Create(token, $"/+{diff}|_SOUL", Color.white);
                }
            }
            OnVitalChange(token);
        }
        if (value.StartsWith("LoseSOUL")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (Soul - diff < 0) {
                diff = Soul;
            }
            if (diff > 0) {
                Soul-=diff;
                if (placed) {
                    PopoverText.Create(token, $"/-{diff}|_SOUL", Color.white);
                }
            }
            OnVitalChange(token);
        }
        if (value.StartsWith("GainStatus")) {
            string[] parts = value.Split("|");
            string status = parts[1];
            Conditions = CollectionUtility.AddToArray(Conditions, status, true);
            if (placed) {
                PopoverText.Create(token, $"_+|_{status.ToUpper()}", Color.white);
            }          
        }               
        if (value.StartsWith("LoseStatus")) {
            string[] parts = value.Split("|");
            string status = parts[1];
            Conditions = CollectionUtility.RemoveAllFromArray(Conditions, status);
            if (placed) {
                PopoverText.Create(token, $"_-|_{status.ToUpper()}", Color.white);
            }
        }
        if (value.StartsWith("SetToken")) {
            string[] parts = value.Split("|");
            string tokenp = parts[1];
            int count = int.Parse(parts[2]);
            int diff = count - CollectionUtility.CountInArray(Tokens, tokenp);

            for (int i = 0; i < Math.Abs(diff); i++) {
                if (diff > 0) {
                    Tokens = CollectionUtility.AddToArray(Tokens, tokenp);
                }
                else if (diff < 0) {
                    Tokens = CollectionUtility.RemoveFromArray(Tokens, tokenp);
                }
            }
            if (placed) {
                if (diff > 0) {
                    PopoverText.Create(token, $"/+{diff}|_{tokenp.ToUpper()}", Color.white);
                }
                else  if (diff < 0) {
                    PopoverText.Create(token, $"/-{Math.Abs(diff)}|_{tokenp.ToUpper()}", Color.white);
                }
            }          
        }               
        if (value.StartsWith("LoseToken")) {
            string[] parts = value.Split("|");
            string status = parts[1];
            Tokens = CollectionUtility.RemoveFromArray(Tokens, status);
            if (placed) {
                PopoverText.Create(token, $"_-|_{status.ToUpper()}", Color.white);
            }
        }
    }

    private void OnVitalChange(Token token) {
        List<string> conditions = Conditions.ToList();

        token.SetDefeated(CurrentHP <= 0);
        if (CurrentHP <= 0) {
            if (!conditions.Contains("Corpse")) {
                conditions.Add("Corpse");
            }
        }
        else {
            if (conditions.Contains("Corpse")) {
                conditions.Remove("Corpse");
            }
        }
    }
}
