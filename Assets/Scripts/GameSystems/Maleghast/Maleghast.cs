using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoconUILibrary;
using SimpleJSON;
using UnityEngine;
using UnityEngine.UIElements;

public class Maleghast : GameSystem
{
    public int TurnNumber = 1;

    public override string SystemName()
    {
        return "Maleghast";
    }

    public override void Setup() {
        base.Setup();

        // Selected
        VisualElement selectedPanel = UI.System.Q("SelectedTokenPanel");
        VisualElement unitPanel = UI.CreateFromTemplate("UITemplates/GameSystem/MaleghastUnitPanel");
        unitPanel.Q("Type").Q<Label>("Label").text = "TYPE";
        unitPanel.Q("Defense").Q<Label>("Label").text = "DEFENSE";
        unitPanel.Q("Move").Q<Label>("Label").text = "MOVE";
        unitPanel.Q("Armor").Q<Label>("Label").text = "ARMOR";
        // unitPanel.Q<Button>("AlterVitals").RegisterCallback<ClickEvent>(AlterVitalsModal);
        // unitPanel.Q<Button>("AddStatus").RegisterCallback<ClickEvent>(AddStatusModal);
        // unitPanel.Q<Button>("Upgrades").RegisterCallback<ClickEvent>(UpgradesModal);
        selectedPanel.Q("Data").Add(unitPanel);
        selectedPanel.Q("ExtraInfo").Add(new Label(){ name = "House" });
        selectedPanel.Q("ExtraInfo").Add(new Label(){ name = "Job" });

        // Focused
        VisualElement focusedPanel = UI.System.Q("FocusedTokenPanel");
        unitPanel = UI.CreateFromTemplate("UITemplates/GameSystem/MaleghastUnitPanel");
        unitPanel.Q("Type").Q<Label>("Label").text = "TYPE";
        unitPanel.Q("Defense").Q<Label>("Label").text = "DEFENSE";
        unitPanel.Q("Move").Q<Label>("Label").text = "MOVE";
        unitPanel.Q("Armor").Q<Label>("Label").text = "ARMOR";
        focusedPanel.Q("Data").Add(unitPanel);
        focusedPanel.Q("ExtraInfo").Add(new Label(){ name = "House" });
        focusedPanel.Q("ExtraInfo").Add(new Label(){ name = "Job" });
    }

    public override string GetTokenDataRawJson()
    {
        return MaleghastTokenDataRaw.ToJson();
    }

    public override void TokenDataSetValue(TokenData data, string value)
    {
        (data as MaleghastTokenData).Change(value);
    }

    public override void GameDataSetValue(string value) {
        FileLogger.Write($"Game system changed - {value}");
        if (value == "IncrementTurn") {
            TurnNumber++;
            UI.System.Q<Label>("TurnNumber").text = TurnNumber.ToString();
            foreach(GameObject g in GameObject.FindGameObjectsWithTag("TokenData")) {
                Icon_v1_5TokenData data = g.GetComponent<Icon_v1_5TokenData>();
                data.Change("LoseStatus|Turn Ended");
            }
        }
    }

    public override Texture2D GetGraphic(string json) {
        MaleghastTokenDataRaw raw = JsonUtility.FromJson<MaleghastTokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenDataSetup(GameObject g, string json, string id) {
        g.GetComponent<MaleghastTokenData>().TokenDataSetup(json, id);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/MaleghastTokenData"));
    }

    public override void UpdateTokenPanel(GameObject data, string elementName)
    {
        if (data == null) {
            UI.ToggleDisplay(elementName, false);
            return;
        }
        UI.ToggleDisplay(elementName, true);
        data.GetComponent<MaleghastTokenData>().UpdateTokenPanel(elementName);
    }

    public override string[] GetEffectList() {
        return new string[]{"Adverse", "Hazard", "Impassable"};
    }

    public override void CreateToken()
    {
        string json = GetTokenDataRawJson();
        Debug.Log(json);

        base.CreateToken();
    }

    public override void AddTokenModal()
    {
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        List<string> units = new();
        foreach (JSONNode house in gamedata["Maleghast"]["Houses"].AsArray) {
            foreach (JSONNode unit in house["units"].AsArray) {
                string houseJob = $"{ house["name"] }/{ unit["name"] }";
                units.Add(houseJob.Replace("\"", ""));
            }
        }

        base.AddTokenModal();

        Modal.AddSearchField("UnitType", "Unit Type", "", units.ToArray());
    }

    // private static void UpgradesModal(ClickEvent evt) {
    //     Modal.Reset("Upgrades");
    //     MaleghastTokenData data = Token.GetSelectedData().GetComponent<TokenData>() as MaleghastTokenData;
    //     Modal.AddLabel("Traits", "");
    //     foreach (string s in data.Traits) {
    //         if (s.EndsWith("|0")) {
    //             string name = s.Replace("|0", "");
    //             Modal.AddToggleField(name, name, false, (evt) => {

    //             });
    //         }
    //     }
    //     Modal.AddToggleField()
    //     Modal.AddPreferredButton("Confirm", Modal.CloseEvent);

    // }
}
