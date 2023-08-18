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
        TokenObject.transform.parent = GameObject.Find("Tokens").transform;
        Texture2D graphic = GameSystem.Current().GetGraphic(Json);
        Token token = TokenObject.GetComponent<Token>();
        token.offlineDataObject = gameObject;
        token.SetImage(graphic);
    }

    void Update() {
        if (TokenObject) {
            TokenObject.transform.position = transform.position;
        }
    }
}
