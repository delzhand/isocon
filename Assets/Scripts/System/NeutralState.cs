using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.UIElements;

public class NeutralState : BaseState
{
    private ConnectMode _mode;

    public NeutralState(ConnectMode mode)
    {
        _mode = mode;
    }

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        Tutorial.Init("tabletop");
        SetConnectionMessage();
        BlockMesh.ToggleAllBorders(false);
        EnableInterface();
    }

    public override void OnExit()
    {
        base.OnExit();
        DisableInterface();

    }

    public override void UpdateState()
    {
        base.UpdateState();
        HandleKeypresses();
        CheckForDisconnect();
    }

    #region Interface
    private void EnableInterface()
    {
        UI.ToggleDisplay("Tabletop", true);
    }

    private void DisableInterface()
    {
        UI.ToggleDisplay("Tabletop", false);
    }
    #endregion

    private void HandleKeypresses()
    {
        if (Input.GetKeyUp(KeyCode.M) && !Modal.IsOpen())
        {
            SM.ChangeState(new MapEditingState());
            return;
        }

        if (Input.GetKeyUp(KeyCode.T) && !Modal.IsOpen())
        {
            SM.ChangeState(new MapMarkingState());
            return;
        }
    }

    #region Setup
    private void SetConnectionMessage()
    {
        Label message = UI.System.Q<Label>("ConnectionMessage");
        string text = "";
        switch (_mode)
        {
            case ConnectMode.Client:
                text = $"You are connected to {Preferences.Current.HostIP} as a client.";
                break;
            case ConnectMode.Host:
                text = "You are hosting on port <LocalIP> (local) and <GlobalIP> (global).<br><br>You must either have port forwarding for port 7777 TCP to your local IP with no active VPN, or use a 3rd party service like Ngrok or Hamachi.";
                break;
            case ConnectMode.Solo:
                text = "You are in solo mode. Other users cannot connect to this table.";
                break;

        }
        message.text = text;
        IPFinder.ReplaceTokens(message);
    }
    #endregion

    private void CheckForDisconnect()
    {
        if (!NetworkClient.isConnected)
        {
            SM.ChangeState(SM.LauncherState);
        }
    }
}