using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;

[System.Serializable]
public class MaleghastTokenDataRaw
{
    public string Name;
    public string House;
    public string UnitType;
    public int Size;
    public string GraphicHash;


    public static string ToJson() {
        MaleghastTokenDataRaw raw = new MaleghastTokenDataRaw();

        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        raw.Name = nameField.value;

        DropdownField houseField = UI.System.Q<DropdownField>("HouseDropdown");
        raw.House = houseField.value;

        DropdownField unitTypeField = UI.System.Q<DropdownField>("TypeDropdown");
        raw.UnitType = unitTypeField.value;

        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        Texture2D graphic = TextureSender.CopyLocalImage(graphicField.value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);

        raw.Size = 1;
        if (raw.UnitType.Contains("Tyrant")) {
            raw.Size = 2;
        }

        return JsonUtility.ToJson(raw);
    }
}

public class MaleghastTokenData : TokenData
{
    [SyncVar]
    public string House;

    [SyncVar]
    public string UnitType;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int Soul;

    [SyncVar]
    public string Armor;

    public int MaxHP;

    public int Move;
    public int Defense;

    public int Size;

    public string Ability1;
    public string Ability2;
    public string Ability3;

    public int Strength;
    public int Weak;
    public int Vitality;
    public int Vulnerability;
    public int Slow;
    public int Speed;

    public bool TurnEnded;

    void Update()
    {
        BaseUpdate();
    }

    public override bool NeedsSetup()
    {
        return MaxHP == 0;
    }

    public override void UpdateUIData() {
        overhead.Q<ProgressBar>("HpBar").value = CurrentHP;
        overhead.Q<ProgressBar>("HpBar").highValue = MaxHP;
    }

    public override void TokenDataSetup(string json) {
        base.TokenDataSetup(json);
        DoTokenDataSetup();
        CurrentHP = MaxHP;
    }

    public override void DoTokenDataSetup() {
        MaleghastTokenDataRaw raw = JsonUtility.FromJson<MaleghastTokenDataRaw>(Json);
        Name = raw.Name;
        GraphicHash = raw.GraphicHash;
        House = raw.House;
        UnitType = raw.UnitType;
        Size = raw.Size;
        SetStats(raw.UnitType);
    }

    public override void CreateWorldToken() {
        base.CreateWorldToken();    
        Color c = ClassColor();
        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        m.SetColor("_Border", c);
        TokenObject.transform.Find("Base").GetComponent<DecalProjector>().material = m;
    }

    public override void CreateUnitBarItem() {
        base.CreateUnitBarItem();
        Color c = ClassColor();
        Element.Q("ClassBackground").style.borderTopColor = c;
        Element.Q("ClassBackground").style.borderRightColor = c;
        Element.Q("ClassBackground").style.borderBottomColor = c;
        Element.Q("ClassBackground").style.borderLeftColor = c;
    }

    public override int GetSize()
    {
        return Size;
    }

    private Color ClassColor() {
        return House switch
        {
            "C.A.R.C.A.S.S." => Environment.FromHex("#f20dae"),
            "Goregrinders" => Environment.FromHex("#ff7829"),
            "Gargamox" => Environment.FromHex("#29ff3d"),
            "Deadsouls" => Environment.FromHex("#90ffef"),
            "Abhorrers" => Environment.FromHex("#ffc93b"),
            "Igorri" => Environment.FromHex("#a000ff"),
            _ => throw new Exception(),
        };
    }

    public void Change(string label, int value) {
        FileLogger.Write($"{Name} {label} set to {value}");
        int originValue;
        switch(label) {
            default:
                FileLogger.Write($"Invalid label '{label}' for int value change");
                throw new Exception($"Invalid label '{label}' for int value change");
        }

        if (originValue == value) {
            return;
        }
        // PopoverText.Create(TokenObject.GetComponent<Token>(), $"{(value < originValue ? "-" : "+")}{Math.Abs(originValue-value)}{shortLabel}", ChangeColor(value, originValue));
        TokenEditPanel.SyncValues();
        LifeEditPanel.SyncValues();
        UpdateSelectedTokenPanel();
    }

    public void Change(string label, string value) {
        FileLogger.Write($"{Name} {label} set to {value}");
        switch(label) {
            default:
                FileLogger.Write($"Invalid label '{label}' for string value change");
                throw new Exception($"Invalid label '{label}' for string value change");
        }

        TokenEditPanel.SyncValues();
        UpdateSelectedTokenPanel();
    }

    private void SetStats(string unitType) {
        
    }

    public void UpdateSelectedTokenPanel() {
    }

    public override bool CheckCondition(string label) {
        switch (label) {
            case "TurnEnded":
                return TurnEnded;
        }
        throw new Exception($"TokenData Condition '{label}' unsupported.");
    } 

}
