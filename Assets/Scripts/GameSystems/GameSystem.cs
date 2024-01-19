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

    public virtual string GetOverheadAsset() {
        return "UITemplates/GameSystem/SimpleOverhead";
    }

    public virtual string TurnAdvanceMessage() {
        return "Increase the round counter?";
    }

    public virtual void Setup()
    {
        UI.System.Q<Label>("SystemInfo").text = SystemName();
    }

    public virtual void Teardown()
    {
    }

    public virtual string SystemName()
    {
        return null;
    }

    public virtual void AddTokenModal() {
        Modal.AddTextField("NameField", "Token Name", "");
    }

    public virtual MenuItem[] GetTokenMenuItems(TokenData data) {
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

    public virtual void CreateToken() {
        throw new NotImplementedException();
    }

    public virtual void UpdateData(TokenData data) {
        if (data.OverheadElement == null) {
            data.CreateOverheadElement();            
        }
    }

    public virtual void TokenDataSetValue(string tokenId, string value) {
        TokenData data = TokenData.Find(tokenId);
        if (value == "EndTurn") {
            data.UnitBarElement.Q("Portrait").style.unityBackgroundImageTintColor = ColorUtility.ColorFromHex("#505050");
        }
        else if (value == "StartTurn") {
            data.UnitBarElement.Q("Portrait").style.unityBackgroundImageTintColor = Color.white;
        }
        if (value.StartsWith("Name")) {
            data.Name = value.Split("|")[1];
        }
    }

    public virtual void UpdateTokenPanel(string tokenId, string elementName) {
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
            case "Maleghast":
                system = g.AddComponent<Maleghast>();
                break;
        }
        Toast.AddSimple(system.SystemName() + " initialized.");
        system.Teardown();
        system.Setup();
    }
}
