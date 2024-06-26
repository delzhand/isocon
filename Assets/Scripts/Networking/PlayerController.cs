using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mirror;
using UnityEngine;

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
        Toast.AddSimple("Host disconnected.");
        FileLogger.Write("Host disconnected");
    }

    public static void Disconnect()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("TokenData");
        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].GetComponent<TokenData>().Disconnect();
        }
        TerrainController.DestroyAllBlocks();
    }
}
