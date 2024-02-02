using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalState : TabletopSubstate
{
    private IState _previous;

    public static void Activate()
    {
        StateManager sm = GameObject.Find("AppState").GetComponent<StateManager>();
        var modalState = new ModalState();
        modalState._previous = sm.SubState;
        sm.ChangeSubState(modalState);
        Modal.AddCloseCallback(Terminate);
    }

    public static void Terminate(ClickEvent evt)
    {
        StateManager sm = GameObject.Find("AppState").GetComponent<StateManager>();
        var modalState = (ModalState)sm.SubState;
        sm.ChangeSubState(modalState._previous);
    }

    public override void UpdateState()
    {
    }

    protected override void EnableInterface()
    {
        UI.ToggleDisplay("BottomBar", false);
        UI.ToggleDisplay("TopBar", false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("Turn"), false);
        UI.ToggleDisplay(UI.System.Q("TopRight").Q("TerrainInfo"), false);
    }

    protected override void HandleKeypresses()
    {
        base.HandleKeypresses();
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Modal.Close();
        }

        if (Input.GetKeyUp(KeyCode.Return))
        {
            Modal.Activate();
        }
    }
}