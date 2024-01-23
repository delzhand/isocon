using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerList : MonoBehaviour
{
    List<string> Players;
    List<string> LastPlayers;

    void Start() {
        Players = new();
        LastPlayers = new();
    }

    // Update is called once per frame
    void Update()
    {
        Players.Clear();
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
            string name = g.GetComponent<Player>().Name;
            Players.Add(name);
            if (!LastPlayers.Contains(name) && name.Length > 0) {
                Toast.AddSimple($"{name} connected.");
            }
        }

        foreach(string player in LastPlayers) {
            if (!Players.Contains(player) && player.Length > 0) {
                Toast.AddSimple($"{player} disconnected.");
            }
        }

        LastPlayers.Clear();
        Players.CopyTo(LastPlayers);

        // NetworkManager netman = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        VisualElement playerList = UI.System.Q("PlayerList");
        playerList.Clear();
        // int connections = 0;
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
        //     connections++;
            string name = g.GetComponent<Player>().Name;
            Label l = new(name);
            l.AddToClassList("playerlist-item");
            l.AddToClassList("no-margin");
            playerList.Add(l);
        }
        UI.System.Q("InfoWindow").Q<Label>("PlayerCount").text = $"{Players.Count}";

        // if (Player.Self()) {
        //     if (Player.Self().Role == PlayerRole.GM) {
        //         UI.System.Q("InfoWindow").Q<Label>("PlayerCount").text = $"{connections}/{netman.maxConnections}";
        //     }
        //     else {
        //     }
        // }

    }
}
