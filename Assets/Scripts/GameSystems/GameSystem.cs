using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Current() {
        string system = PlayerPrefs.GetString("System", "ICON 1.5");
        switch (system) {
            case "ICON 1.5":
                return GameObject.Find("GameSystem").GetComponent<Icon_v1_5>();
            case "Maleghast 666":
            case "Maleghast":
                return GameObject.Find("GameSystem").GetComponent<Maleghast>();
        }
        return GameObject.Find("GameSystem").GetComponent<Generic>();
    }

    public virtual void Setup()
    {
    }

    public virtual void Teardown()
    {
    }

    public virtual string SystemName()
    {
        throw new NotImplementedException();
    }

    public virtual void AddTokenModal() {
        throw new NotImplementedException();
    }

    public virtual string GetTokenData()
    {
        throw new NotImplementedException();
    }

    public virtual void TokenDataSetValue(TokenData data, string label, int value)
    {
        throw new NotImplementedException();
    }

    public virtual void TokenDataSetValue(TokenData data, string label, string value) {
        throw new NotImplementedException();
    }

    public virtual void GameDataSetValue(string label, int value) {
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

    public virtual void UpdateSelectedTokenPanel(GameObject data) {
        throw new NotImplementedException();
    }

    public virtual string GetEditPanelName() {
        throw new NotImplementedException();
    }

    public virtual void SyncEditValues(TokenData data) {
        throw new NotImplementedException();        
    }

    public virtual string[] GetEffectList() {
        throw new NotImplementedException();
    }

    public virtual bool HasEffect(string search, List<string> effects) {
        throw new NotImplementedException();
    }

    public virtual bool HasCustomEffect(List<string> effects) {
        throw new NotImplementedException();
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
