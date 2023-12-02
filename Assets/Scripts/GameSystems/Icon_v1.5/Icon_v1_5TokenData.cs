using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Visibility = UnityEngine.UIElements.Visibility;

[System.Serializable]
public class Icon_v1_5TokenDataRaw: TokenDataRaw
{
    public string Class;
    public string Job;
    public bool Elite;
    public int HPMultiplier;

    public static string ToJson() {
        VisualElement modal = Modal.Find();

        Icon_v1_5TokenDataRaw raw = new Icon_v1_5TokenDataRaw();

        raw.Name = modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);

        string type = modal.Q<DropdownField>("Type").value;
        string playerJob = SearchField.GetValue(modal.Q("PlayerJob"));
        
        if (type == "Player") {
            raw.Class = playerJob.Split("/")[0];
            raw.Job = playerJob.Split("/")[1];
            raw.Elite = false;
            raw.HPMultiplier = 1;
            raw.Size = 1;
        }
        else if (type == "Foe") {
            raw.Class = modal.Q<DropdownField>("FoeClass").value;
            raw.Job = modal.Q<TextField>("FoeJob").value;
            raw.Elite = modal.Q<Toggle>("Elite").value;
            raw.HPMultiplier = raw.Class == "Legend" ? int.Parse(modal.Q<DropdownField>("LegendHP").value.Replace("x", "")) : 1;
            raw.Size = int.Parse(modal.Q<DropdownField>("Size").value[..1]);
        }
        else if (type == "Object") {
            raw.Class = "Object";
            raw.Job = "";
            raw.Elite = false;
            raw.HPMultiplier = modal.Q<IntegerField>("ObjectHP").value;
            raw.Size = int.Parse(modal.Q<DropdownField>("Size").value[..1]);
        }

        return JsonUtility.ToJson(raw);
    }
}

public class Icon_v1_5TokenData : TokenData
{
    [SyncVar]
    public string Class;

    [SyncVar]
    public string Job;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int Vigor;

    [SyncVar]
    public int Wounds;

    [SyncVar]
    public int Resolve;

    public int MaxHP;
    public int Damage;
    public int Fray;
    public int Range;
    public int Speed;
    public int Dash;
    public int Defense;
    public bool Elite;

    public int Aether;
    public int Vigilance;
    public int Blessings;

    public string Marked;
    public string Hatred;
    public string Stance;

    public List<string> Statuses = new();

    public int Size;

    void Start() {      
    }

    void Update()
    {
        BaseUpdate();
    }

    public override bool NeedsSetup() {
        return MaxHP == 0;
    }

    public void UpdateTokenPanel(string elementName) {
        VisualElement panel = UI.System.Q(elementName);
        panel.Q("Portrait").style.backgroundImage = Graphic;
        panel.Q<Label>("Name").text = Name;
        Color c = UnitColor();
        panel.Q("ClassBackground").style.borderTopColor = c;
        panel.Q("ClassBackground").style.borderRightColor = c;
        panel.Q("ClassBackground").style.borderBottomColor = c;
        panel.Q("ClassBackground").style.borderLeftColor = c;
    }

    public override void TokenDataSetup(string json, string id) {
        base.TokenDataSetup(json, id);
        DoTokenDataSetup();
        CurrentHP = MaxHP;
    }

    public override void DoTokenDataSetup() {
        Icon_v1_5TokenDataRaw raw = JsonUtility.FromJson<Icon_v1_5TokenDataRaw>(Json);
        Name = raw.Name;
        GraphicHash = raw.GraphicHash;
        Class = raw.Class;
        Job = raw.Job;
        Elite = raw.Elite;
        Size = raw.Size;
        SetStats(raw.Elite, raw.HPMultiplier);
    }

    public override void CreateOverhead() {
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>("UITemplates/GameSystem/IconOverhead");
        VisualElement instance = template.Instantiate();
        overhead = instance.Q("Overhead");
        UI.System.Q("Worldspace").Add(overhead);
    }

    public override void CreateWorldToken() {
        base.CreateWorldToken();    
        Color c = UnitColor();
        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        m.SetColor("_Border", c);
        TokenObject.transform.Find("Base").GetComponent<DecalProjector>().material = m;
    }

    public override void CreateUnitBarItem() {
        base.CreateUnitBarItem();
        Color c = UnitColor();
        Element.Q("ClassBackground").style.borderTopColor = c;
        Element.Q("ClassBackground").style.borderRightColor = c;
        Element.Q("ClassBackground").style.borderBottomColor = c;
        Element.Q("ClassBackground").style.borderLeftColor = c;
    }

    public override int GetSize()
    {
        return Size;
    }

    private string UnitColorName() {
        switch (Class) {
            case "Wright":
            case "Artillery":
                return "Blue";
            case "Vagabond":
            case "Skirmisher":
                return "Yellow";
            case "Stalwart":
            case "Heavy":
                return "Red";
            case "Leader":
            case "Mendicant":
                return "Green";
            case "Legend":
                return "Purple";
            case "Mob":
                return "Gray";
        }
        return "Black";
    }

    private Color UnitColor() {
        string colorName = UnitColorName();
        return colorName switch
        {
            "Blue" => new Color(0, .63f, 1),
            "Yellow" => new Color(1, .68f, 0),
            "Red" => new Color(.93f, .13f, .05f),
            "Green" => new Color(.38f, .85f, .21f),
            "Purple" => new Color(.79f, .33f, .94f),
            "Gray" => new Color(.57f, .57f, .57f),
            _ => Color.black
        };
    }

    private void SetStats(bool elite, int hpMultiplier) {
        string color = UnitColorName();
        JSONNode gamedata = JSON.Parse(GameSystem.DataJson);
        MaxHP = gamedata["Icon1_5"]["Stats"][color]["MaxHP"];
        Damage = gamedata["Icon1_5"]["Stats"][color]["Damage"];
        Fray = gamedata["Icon1_5"]["Stats"][color]["Fray"];
        Range = gamedata["Icon1_5"]["Stats"][color]["Range"];
        Speed = gamedata["Icon1_5"]["Stats"][color]["Speed"];
        Dash = gamedata["Icon1_5"]["Stats"][color]["Dash"];
        Defense = gamedata["Icon1_5"]["Stats"][color]["Defense"];

        if (elite) {
            MaxHP *= 2;
        }
        else {
            MaxHP *= hpMultiplier;
        }
        Vigor = 0;
        Wounds = 0;
    }

    // public override void Change(int value) {
        // FileLogger.Write($"{Name} {label} set to {value}");
        // int originValue;
        // switch(label) {
        //     case "CurrentHP":
        //         originValue = CurrentHP;
        //         CurrentHP = value;                
        //         PopoverText.Create(TokenObject.GetComponent<Token>(), $"/{(value < originValue ? "-" : "+")}{Math.Abs(originValue-value)}|_HP", ChangeColor(value, originValue));
        //         TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
        //         break;
        //     case "Vigor":
        //         originValue = Vigor;
        //         Vigor = value;
        //         PopoverText.Create(TokenObject.GetComponent<Token>(), $"/{(value < originValue ? "-" : "+")}{Math.Abs(originValue-value)}|_VIG", ChangeColor(value, originValue));
        //         break;
        //     case "Wounds":
        //         originValue = Wounds;
        //         Wounds = value;
        //         break;
        //     case "Aether":
        //         originValue = Aether;
        //         Aether = value;
        //         break;
        //     case "Resolve":
        //         originValue = Resolve;
        //         Resolve = value;
        //         break;
        //     case "PartyResolve":
        //         // Do nothing, we only call this to trigger the redraw
        //         originValue = int.MinValue;
        //         break;
        //     case "Vigilance":
        //         originValue = Vigilance;
        //         Vigilance = value;
        //         break;
        //     case "Blessings":
        //         originValue = Blessings;
        //         Blessings = value;
        //         break;
        //     default:
        //         FileLogger.Write($"Invalid label '{label}' for int value change");
        //         throw new Exception($"Invalid label '{label}' for int value change");
        // }

        // if (originValue == value) {
        //     return;
        // }
        // // PopoverText.Create(TokenObject.GetComponent<Token>(), $"{(value < originValue ? "-" : "+")}{Math.Abs(originValue-value)}{shortLabel}", ChangeColor(value, originValue));
        // TokenEditPanel.SyncValues();
        // LifeEditPanel.SyncValues();
    // }

    public override void Change(string value) {
        FileLogger.Write($"{Name} changed - {value}");
        if (value.StartsWith("GainHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP + diff > MaxHP) {
                diff = MaxHP - CurrentHP;
            }
            if (diff > 0) {
                CurrentHP+=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+{diff}|_HP", Color.white);
                TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
            }
        }
        if (value.StartsWith("LoseHP")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (CurrentHP - diff < 0) {
                diff = CurrentHP;
            }
            if (diff > 0) {
                CurrentHP-=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/-{diff}|_HP", Color.white);
                TokenObject.GetComponent<Token>().SetDefeated(CurrentHP <= 0);
            }
        }
        if (value.StartsWith("GainVIG")) {
            int diff = int.Parse(value.Split("|")[1]);
            if (Vigor + diff > MaxHP/4) {
                diff = MaxHP/4 - CurrentHP;
            }
            if (diff > 0) {
                CurrentHP+=diff;
                PopoverText.Create(TokenObject.GetComponent<Token>(), $"/+{diff}|_HP", Color.white);
            }
        }

        // FileLogger.Write($"{Name} {label} set to {value}");
        // switch(label) {
        //     case "Status":
        //         string[] split = value.Split('|');
        //         if (Statuses.Contains(value)) {
        //             Statuses.Remove(value);
        //             PopoverText.Create(TokenObject.GetComponent<Token>(), $"=-|={split[0].ToUpper()}", ColorUtility.ColorFromHex("#BBBBBB"));
        //             if (value.Contains("Turn Ended")) {
        //                 Element.Q<VisualElement>("Portrait").style.unityBackgroundImageTintColor = Color.white;
        //             }
        //         }
        //         else {
        //             Statuses.Add(value);
        //             PopoverText.Create(TokenObject.GetComponent<Token>(), $"=+|={split[0].ToUpper()}", Color.white);
        //             if (value.Contains("Turn Ended")) {
        //                 Element.Q<VisualElement>("Portrait").style.unityBackgroundImageTintColor = ColorUtility.ColorFromHex("#505050");
        //             }
        //         }

        //         break;
        //     case "Stance":
        //         Stance = value;
        //         break;
        //     case "Marked":
        //         Marked = value;
        //         break;
        //     case "Hatred":
        //         Hatred = value;
        //         break;
        //     default:
        //         FileLogger.Write($"Invalid label '{label}' for string value change");
        //         throw new Exception($"Invalid label '{label}' for string value change");
        // }

        // TokenEditPanel.SyncValues();
    }  
}
