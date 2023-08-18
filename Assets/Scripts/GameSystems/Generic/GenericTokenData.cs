using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
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

public class GenericTokenData : NetworkBehaviour
{
    [SyncVar]
    public string Name;

    [SyncVar]
    public int CurrentHP;

    [SyncVar]
    public int MaxHP;

    [SyncVar]
    public string GraphicHash;

    private bool initialized = false;

    public GameObject TokenObject;
    
    void Update() {
        if (!initialized && Name.Length > 0) {
            TokenObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
            TokenObject.transform.parent = GameObject.Find("Tokens").transform;
            Texture2D graphic = TextureSender.LoadImageFromFile(GraphicHash, true);
            Token token = TokenObject.GetComponent<Token>();
            token.SetImage(graphic);
            token.onlineDataObject = gameObject;
            initialized = true;
        }
        if (TokenObject) {
            TokenObject.transform.position = transform.position;
        }
    }
}
