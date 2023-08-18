using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public static void Disconnect() {
        UI.System.Q("PlayerList").Clear();
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Token");
        for (int i = 0; i < objs.Length; i++) {
            Destroy(objs[i]);
        }
    }
}
