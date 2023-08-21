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

    public override void TokenDataSetup(GameObject g, string json) {
        g.GetComponent<GenericTokenData>().TokenDataSetup(json);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/GenericTokenData"));
    }
}
