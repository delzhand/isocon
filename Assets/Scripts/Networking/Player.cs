using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerRole {
    GM,
    Player,
    Observer
}

public class Player : NetworkBehaviour
{
    public PlayerRole Role;

    // Start is called before the first frame update
    void Start()
    {
        if (GetComponent<NetworkIdentity>().isOwned) {
            if (GameObject.FindGameObjectsWithTag("Player").Length == 1) {
                CmdSetRole(PlayerRole.GM);
            }

            string newName = PlayerPrefs.GetString("PlayerName", "New Player");
            CmdSetPlayerName(newName);
            UI.System.Q<TextField>("e_PlayerName").value = newName;
            UI.System.Q<TextField>("e_PlayerName").RegisterValueChangedCallback((evt) => {
                PlayerPrefs.SetString("PlayerName", evt.newValue);
                CmdSetPlayerName(evt.newValue);
            });

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [Command]
    private void CmdSetRole(PlayerRole newRole) {
        GameObject.Find("PlayerController").GetComponent<PlayerController>().RpcSetPlayerRole(this, newRole);
    }

    [Command]
    private void CmdSetPlayerName(string newName) {
        GameObject.Find("PlayerController").GetComponent<PlayerController>().RpcSetPlayerName(this, newName);
    }
}
