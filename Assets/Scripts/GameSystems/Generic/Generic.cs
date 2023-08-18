using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class Generic : GameSystem
{
    public override string SystemName()
    {
        return "Generic System";
    }

    public override string GetTokenData() {
        return GenericTokenDataRaw.ToJson();
    }

    public override Texture2D GetGraphic(string json) {
        GenericTokenDataRaw raw = JsonUtility.FromJson<GenericTokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenSetup(GameObject g, string json) {
        GenericTokenDataRaw raw = JsonUtility.FromJson<GenericTokenDataRaw>(json);
        GenericTokenData data = g.GetComponent<GenericTokenData>();
        data.Name = raw.Name;
        data.CurrentHP = raw.CurrentHP;
        data.MaxHP = raw.MaxHP;
        data.GraphicHash = raw.GraphicHash;
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/GenericTokenData"));
    }
}
