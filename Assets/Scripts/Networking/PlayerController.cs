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
        FileLogger.Write("Server started");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        FileLogger.Write("Client started");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Toast.Add("Host disconnected.");
        FileLogger.Write("Host disconnected");
    }

    public static void Disconnect() {
        FileLogger.Write("Disconnected");
        Toast.Add("Disconnected.");
        GameObject[] objs = GameObject.FindGameObjectsWithTag("TokenData");
        for (int i = 0; i < objs.Length; i++) {
            objs[i].GetComponent<TokenData2>().Disconnect();
        }
        TerrainController.DestroyAllBlocks();
    }
}
