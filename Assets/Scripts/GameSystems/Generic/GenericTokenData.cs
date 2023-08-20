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

    public static string ToJson() {
        GenericTokenDataRaw raw = new GenericTokenDataRaw();

        TextField nameField = UI.System.Q<TextField>("TokenNameField");
        raw.Name = nameField.value;
        
        DropdownField graphicField = UI.System.Q<DropdownField>("GraphicDropdown");
        Texture2D graphic = TextureSender.CopyLocalImage(graphicField.value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);
        
        raw.MaxHP = 100;
        raw.CurrentHP = raw.MaxHP;
        
        return JsonUtility.ToJson(raw);
    }
}

public class GenericTokenData : TokenData
{
    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int MaxHP;

    void Update()
    {
        BaseUpdate();
    }


    public override void Initialize(string json) {
        GenericTokenDataRaw raw = JsonUtility.FromJson<GenericTokenDataRaw>(json);
        Name = raw.Name;
        CurrentHP = raw.CurrentHP;
        MaxHP = raw.MaxHP;
        GraphicHash = raw.GraphicHash;
        base.Initialize(json);
    }
}
