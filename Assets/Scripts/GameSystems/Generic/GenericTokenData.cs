using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

[System.Serializable]
public class GenericTokenDataRaw
{
    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public string GraphicHash;

    public int Size;

    public static string ToJson() {
        GenericTokenDataRaw raw = new GenericTokenDataRaw();

        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        raw.Name = nameField.value;
        
        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        Texture2D graphic = TextureSender.CopyLocalImage(graphicField.value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);
        
        raw.MaxHP = 100;
        raw.CurrentHP = raw.MaxHP;
        
        raw.Size = 1;

        return JsonUtility.ToJson(raw);
    }
}

public class GenericTokenData : TokenData
{
    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int MaxHP;

    public int Size;

    void Update()
    {
        BaseUpdate();
    }

    public override void UpdateUIData() {
        overhead.Q<ProgressBar>("HpBar").value = CurrentHP;
        overhead.Q<ProgressBar>("HpBar").highValue = MaxHP;
        overhead.Q<ProgressBar>("VigorBar").style.display = DisplayStyle.None;
    }

    public override void TokenDataSetup(string json) {
        base.TokenDataSetup(json);
        DoTokenDataSetup();
    }

    public override void DoTokenDataSetup()
    {
        GenericTokenDataRaw raw = JsonUtility.FromJson<GenericTokenDataRaw>(Json);
        Name = raw.Name;
        MaxHP = raw.MaxHP;
        GraphicHash = raw.GraphicHash;
        Size = raw.Size;
        // CurrentHP = raw.CurrentHP;
    }

    public override int GetSize()
    {
        return Size;
    }
}
