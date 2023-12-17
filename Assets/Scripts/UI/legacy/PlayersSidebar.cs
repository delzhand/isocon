using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayersSidebar : MonoBehaviour
{
    void Awake() {
        UI.System.Q<Button>("PlayersToggle").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("PlayersSidebar");
        });        
    }
}
