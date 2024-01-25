using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerList : MonoBehaviour
{
    private List<string> _players;
    private List<string> _lastPlayers;

    void Start()
    {
        _players = new();
        _lastPlayers = new();
    }

    // Update is called once per frame
    void Update()
    {
        _players.Clear();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            string name = g.GetComponent<Player>().Name;
            _players.Add(name);
            if (!_lastPlayers.Contains(name) && name.Length > 0)
            {
                Toast.AddSimple($"{name} connected.");
            }
        }

        foreach (string player in _lastPlayers)
        {
            if (!_players.Contains(player) && player.Length > 0)
            {
                Toast.AddSimple($"{player} disconnected.");
            }
        }

        _lastPlayers.Clear();
        _players.CopyTo(_lastPlayers);

        VisualElement playerList = UI.System.Q("PlayerList");
        playerList.Clear();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            string name = g.GetComponent<Player>().Name;
            Label l = new(name);
            l.AddToClassList("playerlist-item");
            l.AddToClassList("no-margin");
            playerList.Add(l);
        }
        UI.System.Q("InfoWindow").Q<Label>("PlayerCount").text = $"{_players.Count}";
    }
}
