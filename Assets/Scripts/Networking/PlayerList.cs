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
                string syncTextId = $"{player}_sync";
                HudText.RemoveItem(syncTextId);
            }
        }

        _lastPlayers.Clear();
        _players.CopyTo(_lastPlayers);

        int maxConnections = 1;
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
        {
            var player = g.GetComponent<Player>();
            if (player.Host)
            {
                maxConnections = player.MaxConnections;
            }

            string syncTextId = $"{player.Name}_sync";
            if (player.PercentSynced < 100)
            {
                HudText.SetItem(syncTextId, $"{player.Name} syncing... ({player.PercentSynced}%)", 10, HudTextColor.Red);
            }
            else
            {
                HudText.RemoveItem(syncTextId);
            }
        }
        HudText.SetItem("playerCount", $"Players: {_players.Count}/{maxConnections}", 3, HudTextColor.Blue);
    }
}
