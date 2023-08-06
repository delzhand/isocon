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
        Debug.Log("Server started");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log("Client joined");
        // RpcUpdatePlayerList();
    }

    // public override void OnStopClient() {
    //     RpcUpdatePlayerList();
    // }

    // [ClientRpc]
    // public void RpcUpdatePlayerList() {    
    //     Debug.Log("RpcUpdatePlayerList");    
    //     UI.System.Q("OtherPlayers").Clear();
    //     foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
    //         Player p = g.GetComponent<Player>();
    //         if (!p.isOwned) {
    //             UI.System.Q("OtherPlayers").Add(Player.PlayerElement(p));
    //         }
    //     }
    // }

    public static void Disconnect() {
        UI.System.Q("PlayerList").Clear();
    }
}
