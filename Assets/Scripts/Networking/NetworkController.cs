using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class NetworkController : NetworkManager
{
    // public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    // {
    //     base.OnServerAddPlayer(conn);
    //     Debug.Log("OnServerAddPlayer");
    //     // GameObject player = 
    // }

    // public override void OnServerAddPlayer(NetworkConnection conn)
    // {
    //     Transform start = spawnPos[numPlayers].transform;
    //     GameObject player = Instantiate(playerPrefab, start.position, start.rotation);

    //     NetworkServer.AddPlayerForConnection(conn, player);

    //     int playerId = PlayerManager.Instance.players.Count() - 1;
    //     string name = "car_" + playerId.ToString();
    //     PlayerManager.Instance.SetPlayerName(playerId, name);
    // }
}
