using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalState : BaseState
{
    public static void Activate()
    {
        StateManager.PushState(new ModalState());
    }

    // public override void OnEnter()
    // {
    //     EnableInterface();
    // }

    // private void EnableInterface()
    // {
    //     UI.ToggleDisplay(UI.TopBar, false);
    //     UI.ToggleDisplay("BottomBar", false);
    //     UI.ToggleDisplay(UI.System.Q("TopRight").Q("Pills"), false);
    //     UI.ToggleDisplay(UI.System.Q("TopRight").Q("TerrainInfo"), false);
    // }

    public override void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Modal2.Close();
        }
    }
}