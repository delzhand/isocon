using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEngine;

public class OfflineTokenData : MonoBehaviour
{
    public string Json;
    public GameObject TokenObject;

    void Start() {
        TokenObject = Instantiate(Resources.Load<GameObject>("Prefabs/Token"));
        OnlineTokenDataRaw raw = JsonUtility.FromJson<OnlineTokenDataRaw>(Json);
        Texture2D graphic = TextureSender.LoadImageFromFile(raw.GraphicHash, true);
        Token token = TokenObject.GetComponent<Token>();
        token.offlineDataObject = gameObject;
        token.SetImage(graphic);
    }

    void Update() {
        TokenObject.transform.position = transform.position;
    }
}
