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
    [SyncVar]
    public string Name;

    [SyncVar]
    public PlayerRole Role;

    void Start()
    {
        if (isLocalPlayer) {
            if (GameObject.FindGameObjectsWithTag("Player").Length == 1) {
                Role = PlayerRole.GM;
            }
            string newName = PlayerPrefs.GetString("PlayerName", "New Player");
            UI.System.Q<TextField>("e_PlayerName").value = newName;
            Name = newName;

            UI.System.Q<TextField>("e_PlayerName").RegisterValueChangedCallback((evt) => {
                Name = evt.newValue;
                PlayerPrefs.SetString("PlayerName", evt.newValue);
            });

            CreateElement();
        }
    }

    private void CreateElement() {
        // Create basic version
        VisualElement e = UI.System.Q("PlayerTemplate");
        e.style.display = DisplayStyle.Flex;
        if (Role != PlayerRole.GM) {
            e.Q("GMLabel").style.display = DisplayStyle.None;
        }
        e.Q<Label>("PlayerName").text = Name;

        // Send to server
        CmdAddToPlayerList(e);

        // Set up editing
        e.Q<Label>("PlayerName").RegisterCallback<ClickEvent>((evt) => {
            e.Q<TextField>("PlayerNameEdit").value = Name;
            UI.ToggleDisplay(e.Q("PlayerName"), false);
            UI.ToggleDisplay(e.Q("PlayerNameEdit"), true);
        });

        e.Q<TextField>("PlayerNameEdit").RegisterCallback<BlurEvent>((evt) => {
            Name = e.Q<TextField>("PlayerNameEdit").value;
        });
    }

    [Command]
    private void CmdAddToPlayerList(VisualElement e) {
        RpcUpdatePlayerList(e);
    }

    [ClientRpc]
    private void RpcUpdatePlayerList(VisualElement e) {
        UI.System.Q("PlayersList").Add(e);
    }
}
