using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

public class GameSystem : MonoBehaviour
{
    public static string DataJson = "{}";

    protected int TurnNumber = 1;

    public static GameSystem Current() {
        GameSystem system = GameObject.Find("GameSystem").GetComponent<GameSystem>();
        switch(system.SystemName()) {
            case "ICON 1.5":
                return system as Icon_v1_5;
            case "Maleghast":
                return system as Maleghast;
            case "Generic System":
                return system as Generic;
        }
        return null;
    }

    public virtual void Setup()
    {
    }

    public virtual void Teardown()
    {
    }

    public virtual string SystemName()
    {
        return null;
    }

    public virtual void InterpreterMethod(string name, object[] args) {
        throw new NotImplementedException();
    }

    public virtual void AddTokenModal() {
        Modal.AddTextField("NameField", "Token Name", "");
    }

    public virtual MenuItem[] GetTokenMenuItems(TokenData2 data) {
        List<MenuItem> items = new();
        return items.ToArray();
    }

    public virtual void GameDataSetValue(string value) {
        throw new NotImplementedException();
    }

    public virtual string[] GetEffectList()
    {
        return new string[]{"Wavy", "Spiky", "Hand", "Skull", "Hole", "Blocked", "Corners"};
    }

    #region Interpreted Methods
    public void CreateToken() {
        InterpreterMethod("CreateToken", new object[]{});
    }

    public virtual void UpdateData(TokenData2 data) {
        InterpreterMethod("UpdateData", new object[]{data});
    }

    public void TokenDataSetValue(string tokenId, string value) {
        InterpreterMethod("Change", new object[]{tokenId, value});
    }

    public void UpdateTokenPanel(string tokenId, string elementName) {
        InterpreterMethod("UpdateTokenPanel", new object[]{tokenId, elementName});
    }
    #endregion

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
            case "Maleghast":
                system = g.AddComponent<Maleghast>();
                break;
        }
        Toast.Add(system.SystemName() + " initialized.");
        system.Teardown();
        system.Setup();
    }
}
