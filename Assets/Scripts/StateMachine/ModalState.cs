using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class ModalState : BaseState
{
    string name;

    public ModalState(string modalName)
    {
        name = modalName;
    }

    public static void Activate(string modalName)
    {
        StateManager.PushState(new ModalState(modalName));
    }

    public override string GetName()
    {
        return $"{this.GetType().Name} ({name})";
    }


    public override void HandleInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Modal2.Close();
        }
    }
}