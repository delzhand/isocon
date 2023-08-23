using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSystem : MonoBehaviour
{
    public static GameSystem Current() {
        string system = PlayerPrefs.GetString("System", "Generic");
        switch (system) {
            case "ICON 1.5":
                return GameObject.Find("GameSystem").GetComponent<Icon_v1_5>();
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

    public virtual string GetTokenData()
    {
        throw new NotImplementedException();
    }

    public virtual Texture2D GetGraphic(string json) {
        throw new NotImplementedException();
    }

    public virtual void TokenDataSetup(GameObject g, string json) {
        throw new NotImplementedException();
    }

    public virtual GameObject GetDataPrefab() {
        throw new NotImplementedException();
    }

    public virtual void UpdateSelectedTokenPanel(GameObject data) {
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
        }
        Toast.Add(system.SystemName() + " initialized.");
        system.Setup();
    }

}
