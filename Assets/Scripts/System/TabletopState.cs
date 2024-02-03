using System.Collections.Generic;
using System.IO;
using Mirror;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.UIElements;

public class TabletopState : BaseState
{
    private ConnectMode _mode;

    public TabletopState(ConnectMode mode)
    {
        _mode = mode;
    }

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        if (_mode == ConnectMode.Host || _mode == ConnectMode.Solo)
        {
            TerrainController.InitializeTerrain(8, 8, 1);
            IngestRuleData();
        }
        SetConnectionMessage();
        EnableInterface();
        BindCallbacks();
        Tutorial.Init("tabletop");

        SM.ChangeSubState(new NeutralState());
    }

    public override void OnExit()
    {
        base.OnExit();
        DisableInterface();
        UnbindCallbacks();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        CheckForDisconnect();
    }

    #region Interface
    private void EnableInterface()
    {
        UI.ToggleDisplay("Tabletop", true);
        UI.ToggleDisplay("BottomBar", true);
        UI.ToggleDisplay("TopBar", true);
    }

    private void DisableInterface()
    {
        UI.ToggleDisplay("Tabletop", false);
    }
    #endregion

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
        if (NetworkClient.isConnected)
        {
            return;
        }

        if (SM.GetComponent<Fader>() == null)
        {
            Fader.StartFade(Color.black, .5f, GoToLauncherState);
        }
    }

    private void GoToLauncherState()
    {
        SM.ChangeState(new LauncherState());
    }

    private void IngestRuleData()
    {
        string filename = $"{Preferences.Current.DataPath}/ruledata/{Preferences.Current.RulesFile}";
        if (!File.Exists(filename))
        {
            Toast.AddError($"Could not locate {filename}. Reverting to latest rule data.");
            filename = $"{Preferences.Current.DataPath}/ruledata/latest.json";
        }
        string json = File.ReadAllText(filename);
        GameSystem.DataJson = json;
        Toast.AddSimple("Rule data loaded.");
    }

    #region Callbacks
    private void BindCallbacks()
    {
        UI.TopBar.Q("Isocon").RegisterCallback<ClickEvent>(ConfirmReturnToLauncher);
        Dragger.RightDragStart += Viewport.InitializeDrag;
        Dragger.RightDragUpdate += Viewport.UpdateDrag;
        Dragger.RightDragRelease += Viewport.EndDrag;
    }

    private void UnbindCallbacks()
    {
        UI.TopBar.Q("Isocon").UnregisterCallback<ClickEvent>(ConfirmReturnToLauncher);
        Dragger.RightDragStart -= Viewport.InitializeDrag;
        Dragger.RightDragUpdate -= Viewport.UpdateDrag;
        Dragger.RightDragRelease -= Viewport.EndDrag;
    }

    private void ConfirmReturnToLauncher(ClickEvent evt)
    {
        string message = "Exit the tabletop and return to the Isocon Launcher?";
        if (NetworkClient.activeHost && _mode == ConnectMode.Host)
        {
            message = "You are hosting. <b>Disconnecting from the table will end the session!</b> Exit the tabletop and return to the Isocon Launcher?";
        }
        Modal.DoubleConfirm("Exit Tabletop", message, Quit);
    }

    private void Quit()
    {
        NetworkManager manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            manager.StopHost();
            manager.StopClient();
        }
        else if (NetworkClient.isConnected)
        {
            manager.StopClient();
        }
        PlayerController.Disconnect();
    }

    #endregion
}