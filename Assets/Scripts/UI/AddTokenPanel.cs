using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class AddTokenPanel : MonoBehaviour
{
    void Awake() {
        UI.System.Q<Button>("AddTokenCreateButton").RegisterCallback<ClickEvent>((evt) => {
            Player p = NetworkClient.localPlayer.GetComponent<Player>();
            string name = UI.System.Q<TextField>("TokenNameField").value;
            p.CmdRequestAddToken(name);
        });
    }
}
