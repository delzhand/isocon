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

    public override string GetTokenDataRawJson() {
        return GenericTokenDataRaw.ToJson();
    }

    public override Texture2D GetGraphic(string json) {
        GenericTokenDataRaw raw = JsonUtility.FromJson<GenericTokenDataRaw>(json);
        return TextureSender.LoadImageFromFile(raw.GraphicHash, true);
    }

    public override void TokenDataSetup(GameObject g, string json, string id) {
        g.GetComponent<GenericTokenData>().TokenDataSetup(json, id);
    }

    public override GameObject GetDataPrefab() {
        return Instantiate(Resources.Load<GameObject>("Prefabs/GenericTokenData"));
    }

    public override void UpdateTokenPanel(GameObject data, string elementName)
    {
        if (data == null) {
            UI.ToggleDisplay(elementName, false);
            return;
        }
        UI.ToggleDisplay(elementName, true);
        data.GetComponent<GenericTokenData>().UpdateTokenPanel(elementName);
    }

    public override void AddTokenModal()
    {
        base.AddTokenModal();
        Modal.AddDropdownField("SizeField", "Size", "1x1", new string[]{"1x1", "2x2", "3x3"});
        Modal.AddIntField("HPField", "HP", 1);
    }

}
