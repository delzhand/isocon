using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;


[System.Serializable]
public class OnlineTokenDataRaw
{
    public string Name;
    public int CurrentHP;
    public int MaxHP;
    public string GraphicHash;

    public static string ToJson(string name, Texture2D graphic) {
        OnlineTokenDataRaw raw = new OnlineTokenDataRaw();
        raw.MaxHP = 100;
        raw.CurrentHP = raw.MaxHP;
        raw.Name = name;
        raw.GraphicHash = TextureSender.GetTextureHash(graphic);
        return JsonUtility.ToJson(raw);
    }
}

public class OnlineTokenData : NetworkBehaviour
{
    [SyncVar]
    public string Name;
    
    [SyncVar]
    public int CurrentHP;
    
    [SyncVar]
    public int MaxHP;

    [SyncVar]
    public string GraphicHash;

    public GameObject TokenObject;

    void Start() {
        Debug.Log(TokenObject);
        if (TokenObject == null) {
            TokenObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        }
        Texture2D graphic = TextureSender.LoadImageFromFile(GraphicHash, true);
        Token token = TokenObject.GetComponent<Token>();
        token.SetImage(graphic);
        token.onlineDataObject = gameObject;
    }
}
