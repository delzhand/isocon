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
        // updatePlayerList();
    }

    public override void OnStopClient() {
        RpcUpdatePlayerList();
    }

    [ClientRpc]
    private void RpcUpdatePlayerList() {        
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
        }
    }
}
