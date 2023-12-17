using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSystem : MonoBehaviour
{
    public static string DataJson = "{}";

    public static GameSystem Current() {
        string system = PlayerPrefs.GetString("System", "Generic");
        switch (system) {
            case "ICON 1.5":
                return GameObject.Find("GameSystem").GetComponent<Icon_v1_5>();
            case "Maleghast":
                return GameObject.Find("GameSystem").GetComponent<Maleghast>();
        }
        return GameObject.Find("GameSystem").GetComponent<Generic>();
    }

    public virtual void Setup()
    {
        // // Search field for tile effects
        // VisualElement root = UI.System.Q("ToolsPanel");
        // VisualElement searchField = SearchField.Create(GameSystem.Current().GetEffectList(), "");
        // searchField.name = "EffectSearchField";
        // searchField.style.marginTop = 2;
        // root.Q("EffectSearch").Add(searchField);

        // Setting up play mode tile effects modal has to wait until the gamesystem is created
        UI.System.Q("TerrainInfo").Q("AddEffectButton").RegisterCallback<ClickEvent>(AddTerrainEffect.OpenModal);
    }

    public virtual void Teardown()
    {
        // VisualElement root = UI.System.Q("ToolsPanel");
        // root.Q("EffectSearch").Clear();

        UI.System.Q("TerrainInfo").Q("AddEffectButton").UnregisterCallback<ClickEvent>(AddTerrainEffect.OpenModal);
    }

    public virtual string SystemName()
    {
        throw new NotImplementedException();
    }

    public virtual void AddTokenModal() {
        Modal.AddTextField("NameField", "Token Name", "");
    }

    public virtual void CreateToken() {
        string json = GameSystem.Current().GetTokenDataRawJson();
        FileLogger.Write($"Token added: {json}");
        Player.Self().CmdCreateTokenData(json);
    }

    public virtual string GetTokenDataRawJson()
    {
        throw new NotImplementedException();
    }

    public virtual MenuItem[] GetTokenMenuItems(TokenData data) {
        List<MenuItem> items = new();
        return items.ToArray();
    }

    public virtual void TokenDataSetValue(TokenData data, string value) {
        data.Change(value);
    }

    public virtual void GameDataSetValue(string value) {
        throw new NotImplementedException();
    }

    public virtual Texture2D GetGraphic(string json) {
        throw new NotImplementedException();
    }

    public virtual void TokenDataSetup(GameObject g, string json, string id) {
        throw new NotImplementedException();
    }

    public virtual GameObject GetDataPrefab() {
        throw new NotImplementedException();
    }

    public virtual void UpdateTokenPanel(GameObject data, string elementName) {
        throw new NotImplementedException();
    }

    public virtual string GetEditPanelName() {
        throw new NotImplementedException();
    }

    public virtual void SyncEditValues(TokenData data) {
        throw new NotImplementedException();        
    }

    public virtual string MappedEffectName(string effect) {
        return effect;
    }

    public virtual string DeMappedEffectName(string effect) {
        return effect;
    }

    public virtual string[] GetEffectList()
    {
        return new string[]{"Wavy", "Spiky", "Hand", "Hole", "Blocked", "Corners"};
    }

    public static void Set(string value) {
        GameSystem current = GameSystem.Current();
        if (current) {
            current.Teardown();
        }
        GameObject g = GameObject.Find("GameSystem");
        GameSystem system = g.GetComponent<GameSystem>();
        DestroyImmediate(system);
        switch(value) {
            case "Generic":
                system = g.AddComponent<Generic>();
                break;
            case "ICON 1.5":
                system = g.AddComponent<Icon_v1_5>();
                break;
            case "Maleghast 666":
            case "Maleghast":
                system = g.AddComponent<Maleghast>();
                break;
        }
        Toast.Add(system.SystemName() + " initialized.");
        system.Teardown();
        system.Setup();
    }

}
