using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using Visibility = UnityEngine.UIElements.Visibility;

[System.Serializable]
public class Icon_v1_5TokenDataRaw
{
    public string Name;
    public string Class;
    public string Job;
    public bool Elite;
    public int LegendHP;
    public int Size;
    public int ObjectHP;
    public string GraphicHash;

    public static string ToJson() {
        Icon_v1_5TokenDataRaw raw = new Icon_v1_5TokenDataRaw();

        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        raw.Name = nameField.value;

        DropdownField classField = UI.System.Q<DropdownField>("ClassDropdown");
        raw.Class = classField.value;

        DropdownField jobField = UI.System.Q<DropdownField>("JobDropdown");
        raw.Job = jobField.value;

        Toggle eliteField = UI.System.Q<Toggle>("EliteToggle");
        raw.Elite = eliteField.value;
        
        IntegerField objHPField = UI.System.Q<IntegerField>("ObjectHPField");
        raw.ObjectHP = objHPField.value;

        if (raw.Class == "Legend") {
            DropdownField legendHpField = UI.System.Q<DropdownField>("LegendHPDropdown");
            raw.LegendHP = int.Parse(legendHpField.value.Replace("x", ""));
        }
        else {
            raw.LegendHP = 1;
        }

        DropdownField sizeField = UI.System.Q<DropdownField>("SizeDropdown");
        raw.Size = sizeField.value switch
        {
            "Large (2)" => 2,
            "Huge (3)" => 3,
            _ => 1,
        };
        if (!Icon_v1_5TokenData.IsFoe(raw.Class)) {
            raw.Size = 1;
        }

        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        Texture2D graphic = TextureSender.CopyLocalImage(graphicField.value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);


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

    [SyncVar]
    public int GResolve;

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

    public override void UpdateUIData() {
        overhead.Q<ProgressBar>("HpBar").value = CurrentHP;
        overhead.Q<ProgressBar>("HpBar").highValue = MaxHP;
        overhead.Q<ProgressBar>("VigorBar").value = Vigor;
        overhead.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        if (Vigor == 0) {
            overhead.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Hidden;
        }
        else {
            overhead.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Visible;
        }
        for (int i = 1; i <= 3; i++) {
            if (Wounds >= i) {
                overhead.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
            }
            else {
                overhead.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
            }
        }
    }

    public override void TokenDataSetup(string json) {
        base.TokenDataSetup(json);
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
        SetStats(raw.Elite, raw.LegendHP, raw.ObjectHP);
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

    public void UpdateSelectedTokenPanel() {
        VisualElement panel = UI.System.Q("SelectedTokenPanel");

        Color c = ClassColor();
        panel.Q("ClassBackground").style.borderTopColor = c;
        panel.Q("ClassBackground").style.borderRightColor = c;
        panel.Q("ClassBackground").style.borderBottomColor = c;
        panel.Q("ClassBackground").style.borderLeftColor = c;

        panel.Q("Portrait").style.backgroundImage = Graphic;

        panel.Q<Label>("CHP").text = CurrentHP.ToString();
        panel.Q<Label>("MHP").text = "/" + MaxHP.ToString();
        panel.Q<Label>("VIG").text = Vigor > 0 ? "+" + Vigor.ToString() : "";
        panel.Q<Label>("Name").text = Name;

        panel.Q<ProgressBar>("HpBar").value = CurrentHP;
        panel.Q<ProgressBar>("HpBar").highValue = MaxHP;
        panel.Q<ProgressBar>("VigorBar").value = Vigor;
        panel.Q<ProgressBar>("VigorBar").highValue = MaxHP;
        if (Vigor == 0) {
            panel.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Hidden;
        }
        else {
            panel.Q<ProgressBar>("VigorBar").style.visibility = Visibility.Visible;
        }
        // for (int i = 1; i <= 3; i++) {
        //     if (Wounds >= i) {
        //         panel.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Visible;
        //     }
        //     else {
        //         panel.Q<VisualElement>("Wound" + i).style.visibility = Visibility.Hidden;
        //     }
        // }

        if (!IsFoe(Class)) {
            panel.Q<Label>("ResolveNum").text = Resolve.ToString();
            panel.Q<Label>("PartyResolveNum").text = "+" + GResolve.ToString();
            panel.Q<ProgressBar>("ResolveBar").value = Resolve + GResolve;
            panel.Q<ProgressBar>("PartyResolveBar").value = GResolve;            
            panel.Q("ResolveWrapper").style.display = DisplayStyle.Flex;
        }
        else {
            panel.Q("ResolveWrapper").style.display = DisplayStyle.None;
        }

        if (Elite) {
            panel.Q<Label>("Elite").style.visibility = Visibility.Visible;
        }
        else {
            panel.Q<Label>("Elite").style.visibility = Visibility.Hidden;
        }

        panel.Q<Label>("Job").text = Job;
        panel.Q<Label>("Job").style.backgroundColor = c;

        // panel.Q("Statuses").Clear();
        // int statusCount = 0;
        // if (CurrentHP == 0) {
        //     statusCount++;
        //     addStatus(panel, "Incapacitated", false);
        // }
        // else if (CurrentHP * 2 <= MaxHP) {
        //     statusCount++;
        //     addStatus(panel, "Bloodied", false);
        // }
        // for(int i = 0; i < status.Count; i++) {
        //     statusCount++;
        //     addStatus(panel, status[i], TokenController.IsPositive(status[i]));
        // }

        // List<(string, int)> counters = new List<(string, int)>();
        // counters.Add(("Aether", Aether));
        // counters.Add(("Blessings", Blessings));
        // counters.Add(("Vigilance", Vigilance));
        // for(int i = 0; i < counters.Count; i++) {
        //     if (counters[i].Item2 > 0) {
        //         statusCount++;
        //         addStatus(panel, counters[i].Item1 + " " + counters[i].Item2, true);
        //     }
        // }

        // if (Stance.Length > 0) {
        //     statusCount++;
        //     addStatus(panel, "Stance " + Stance, true);
        // }

        // if (Mark.Length > 0) {
        //     statusCount++;
        //     addStatus(panel, "Marked by " + Mark, false);
        // }

        // if (Hate.Length > 0) {
        //     statusCount++;
        //     addStatus(panel, "Hatred of " + Hate, false);
        // }

        // if (statusCount == 0) {
        //     addStatus(panel, "Normal", true);
        // }

        // if (StackedDie) {
        //     addStatus(panel, "Stacked Die", true);
        // }

        panel.Q<Label>("StatDef").text = Defense.ToString();
        panel.Q<Label>("StatDmg").text = "D" + Damage.ToString();
        panel.Q<Label>("StatFray").text = Fray.ToString();
        panel.Q<Label>("StatRng").text = Range.ToString();
        panel.Q<Label>("StatSpd").text = Speed.ToString();
        panel.Q<Label>("StatDash").text = Dash.ToString();        
    }

    public static bool IsFoe(string jclass) {
        return jclass switch
        {
            "Wright" or "Stalwart" or "Mendicant" or "Vagabond" => false,
            _ => true
        };
    }

    private Color ClassColor() {
        return Class switch
        {
            "Wright" or "Artillery" => new Color(0, .63f, 1),
            "Vagabond" or "Skirmisher" => new Color(1, .68f, 0),
            "Stalwart" or "Heavy" => new Color(.93f, .13f, .05f),
            "Leader" or "Mendicant" => new Color(.38f, .85f, .21f),
            "Legend" => new Color(.79f, .33f, .94f),
            "Mob" => new Color(.57f, .57f, .57f),
            "Object" => Color.black,
            _ => throw new Exception(),
        };
    }

    private void SetStats(bool elite, int legendHp, int objHp) {
        switch (Class) {
            case "Wright":
            case "Artillery":
                MaxHP = 32;
                Damage = 8;
                Fray = 3;
                Range = 6;
                Speed = 4;
                Dash = 2;
                Defense = 7;
                break;
            case "Vagabond":
            case "Skirmisher":
                MaxHP = 28;
                Damage = 10;
                Fray = 2;
                Range = 4;
                Speed = 4;
                Dash = 4;
                Defense = 10;
                break;
            case "Stalwart":
            case "Heavy":
                MaxHP = 40;
                Damage = 6;
                Fray = 4;
                Range = 3;
                Speed = 4;
                Dash = 2;
                Defense = 6;
                break;
            case "Leader":
            case "Mendicant":
                MaxHP = 40;
                Damage = 6;
                Fray = 3;
                Range = 3;
                Speed = 4;
                Dash = 2;
                Defense = 8;
                break;
            case "Legend":
                MaxHP = 50;
                Damage = 8;
                Fray = 3;
                Range = 3;
                Speed = 4;
                Dash = 2;
                Defense = 8;
                break;
            case "Mob":
                MaxHP = 2;
                Damage = 6;
                Fray = 3;
                Range = 1;
                Speed = 4;
                Dash = 2;
                Defense = 8;
                break;
            case "Object":
                MaxHP = objHp;
                Damage = 0;
                Fray = 0;
                Range = 0;
                Speed = 0;
                Dash = 0;
                Defense = 0;
                break;
        }
        if (elite) {
            MaxHP *= 2;
        }
        else {
            MaxHP *= legendHp;
        }
        Vigor = 0;
        Wounds = 0;
    }

    public void Change(string label, int value) {
        FileLogger.Write($"{Name} {label} set to {value}");
        int originValue;
        string shortLabel;
        switch(label) {
            case "CurrentHP":
                originValue = CurrentHP;
                CurrentHP = value;                
                shortLabel = "HP";
                break;
            case "Vigor":
                originValue = Vigor;
                Vigor = value;
                shortLabel = "VIG";
                break;
            case "Wounds":
                originValue = Wounds;
                Wounds = value;
                shortLabel = "WND";
                break;
            case "Aether":
                originValue = Aether;
                Aether = value;
                shortLabel = "ATH";
                break;
            case "Resolve":
                originValue = Resolve;
                Resolve = value;
                shortLabel = "RES";
                break;
            case "Vigilance":
                originValue = Vigilance;
                Vigilance = value;
                shortLabel = "VGL";
                break;
            case "Blessings":
                originValue = Blessings;
                Blessings = value;
                shortLabel = "BLS";
                break;
            default:
                FileLogger.Write($"Invalid label '{label}' for int value change");
                throw new Exception($"Invalid label '{label}' for int value change");
        }

        if (originValue == value) {
            return;
        }
        PopoverText.Create(TokenObject.GetComponent<Token>(), $"{(value < originValue ? "-" : "+")}{Math.Abs(originValue-value)}{shortLabel}", ChangeColor(value, originValue));
    }

    private Color ChangeColor(int a, int b) {
        return Color.white;
        // return ColorSidebar.FromHex(a < b ? "#F77474" : "#74F774");
    }
}
