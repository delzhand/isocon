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
    }

    void Update() {
        StringBuilder sb = new StringBuilder();
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
            sb.AppendLine(g.name + " (" + g.GetComponent<Player>().Role + ")");
        }
        UI.System.Q<Label>("ConnectionsList").text = sb.ToString();
    }

    [ClientRpc]
    public void RpcSetPlayerName(Player player, string newName)
    {
        player.name = newName;
    }

    [ClientRpc]
    public void RpcSetPlayerRole(Player player, PlayerRole newRole)
    {
        player.Role = newRole;
    }
}
