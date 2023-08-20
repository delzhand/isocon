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


    void Update()
    {
        BaseUpdate();
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



    public override void Initialize(string json) {
        Icon_v1_5TokenDataRaw raw = JsonUtility.FromJson<Icon_v1_5TokenDataRaw>(json);
        GraphicHash = raw.GraphicHash;
        base.Initialize(json); // GraphicHash must be set before this gets called

        Name = raw.Name;
        Job = raw.Job;
        Class = raw.Class;

        SetStats(raw.Elite, raw.LegendHP);

        if (raw.Size == 2) {
            TokenObject.GetComponent<Token>().Size = 2;
            TokenObject.transform.Find("Offset").transform.localPosition += new Vector3(0, 0, -.73f);
            TokenObject.transform.Find("Base").transform.localPosition += new Vector3(0, 0, -.73f);
            TokenObject.transform.Find("Offset").transform.localScale = new Vector3(2, 2, 2);
            TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(2, 2, 4);
        }
        else if (raw.Size == 3) {
            TokenObject.GetComponent<Token>().Size = 3;
            TokenObject.transform.Find("Offset").transform.localScale = new Vector3(3, 3, 3);
            TokenObject.transform.Find("Base").GetComponent<DecalProjector>().size = new Vector3(3, 3, 4);
        }

        Color c = ClassColor();
        Element.Q("ClassBackground").style.borderTopColor = c;
        Element.Q("ClassBackground").style.borderRightColor = c;
        Element.Q("ClassBackground").style.borderBottomColor = c;
        Element.Q("ClassBackground").style.borderLeftColor = c;

        Material m = Instantiate(Resources.Load<Material>("Materials/Token/BorderBase"));
        m.SetColor("_Border", c);
        TokenObject.transform.Find("Base").GetComponent<DecalProjector>().material = m;
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
            _ => throw new Exception(),
        };
    }

    private void SetStats(bool elite, int legendHp) {
        Debug.Log(legendHp);
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
        }
        if (elite) {
            MaxHP *= 2;
        }
        else {
            MaxHP *= legendHp;
        }
        CurrentHP = MaxHP;
        Vigor = 0;
        Wounds = 0;
    }
}
