using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

[System.Serializable]
public class GenericTokenDataRaw: TokenDataRaw
{
    public int CurrentHP;
    public int MaxHP;

    public static string ToJson() {
        GenericTokenDataRaw raw = new GenericTokenDataRaw();

        raw.Name = UI.Modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(UI.Modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);
        
        raw.MaxHP = UI.Modal.Q<IntegerField>("HPField").value;
        
        raw.Size = 1;
        string sizeValue = UI.Modal.Q<DropdownField>("SizeField").value;
        if (sizeValue == "2x2") {
            raw.Size = 2;
        }
        else if (sizeValue == "3x3") {
            raw.Size = 3;
        }

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

    public override void UpdateOverheadValues() {
        OverheadElement.Q<ProgressBar>("HpBar").value = CurrentHP;
        OverheadElement.Q<ProgressBar>("HpBar").highValue = MaxHP;
    }

    public override void TokenDataSetup(string json, string id) {
        base.TokenDataSetup(json, id);
        DoTokenDataSetup();
        CurrentHP = MaxHP;
    }

    public override void DoTokenDataSetup()
    {
        GenericTokenDataRaw raw = JsonUtility.FromJson<GenericTokenDataRaw>(Json);
        Name = raw.Name;
        MaxHP = raw.MaxHP;
        GraphicHash = raw.GraphicHash;
        Size = raw.Size;
    }

    public override int GetSize()
    {
        return Size;
    }
}
