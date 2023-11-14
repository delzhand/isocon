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
        VisualElement modal = Modal.Find();

        GenericTokenDataRaw raw = new GenericTokenDataRaw();

        raw.Name = modal.Q<TextField>("NameField").value;
        Texture2D graphic = TextureSender.CopyLocalImage(modal.Q("ImageSearchField").Q<TextField>("SearchInput").value);
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);
        
        raw.MaxHP = modal.Q<IntegerField>("HPField").value;
        
        raw.Size = 1;
        string sizeValue = modal.Q<DropdownField>("SizeField").value;
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

    public override void UpdateUIData() {
        overhead.Q<ProgressBar>("HpBar").value = CurrentHP;
        overhead.Q<ProgressBar>("HpBar").highValue = MaxHP;
        overhead.Q<ProgressBar>("VigorBar").style.display = DisplayStyle.None;
    }

    public override void TokenDataSetup(string json, string id) {
        base.TokenDataSetup(json, id);
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

    public void UpdateSelectedTokenPanel() {
        if (!TokenController.IsSelected(this)) {
            return;
        }

        VisualElement panel = UI.System.Q("SelectedTokenPanel");
        panel.Q("Portrait").style.backgroundImage = Graphic;
        panel.Q<Label>("Name").text = Name;
    }
}
