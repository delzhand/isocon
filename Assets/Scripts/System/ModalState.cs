using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalState : TabletopSubstate
{
    public static void Activate()
    {
        GoToModal(new ClickEvent());
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