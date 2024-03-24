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
        if (Player.Self().Host)
        {
            TerrainController.InitializeTerrain(8, 8, 1);
        }
        SetConnectionMessage();
        EnableInterface();
        BindCallbacks();
#if UNITY_WEBGL
        Tutorial.Init("web client");
#endif

#if !UNITY_WEBGL
        Tutorial.Init("tabletop");
#endif

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
        UI.ToggleDisplay("TopBar", true);
        UI.ToggleDisplay("DetailsHud", Preferences.Current.ShowHUD);
#if UNITY_WEBGL
        UI.ToggleDisplay("AddToken", false);
#endif
    }

    private void DisableInterface()
    {
        UI.ToggleDisplay("Tabletop", false);
    }
    #endregion

    #region Setup
    private void SetConnectionMessage()
    {
        string text = "";
        switch (_mode)
        {
            case ConnectMode.Client:
                text = $"Connected: {Preferences.Current.HostIP}";
                break;
            case ConnectMode.Host:
                text = "Hosting: <LocalIP>|<GlobalIP>";
                break;
            case ConnectMode.Solo:
                text = "Solo";
                break;

        }
        HudText.SetItem("connectionInfo", text, 1, HudTextColor.Blue);
        IPFinder.ReplaceTokens(text);
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

    public static void IngestRuleData()
    {
        string filename = $"{Preferences.Current.DataPath}/ruledata/latest.json";
        bool usingDefault = true;
        if (Preferences.Current.OverrideRules)
        {
            if (File.Exists(Preferences.Current.RulesFile))
            {
                filename = Preferences.Current.RulesFile;
                usingDefault = false;
            }
            else
            {
                Toast.AddError($"Could not locate {filename}. Reverting to default.");
            }
        }
        string json = File.ReadAllText(filename);
        GameSystem.DataJson = json;
        string message = usingDefault ? "Default rule data loaded." : "Homebrew rule data loaded.";
        Toast.AddSimple(message);
    }

    #region Callbacks
    private void BindCallbacks()
    {
        UI.TopBar.Q("Isocon").RegisterCallback<ClickEvent>(ConfirmReturnToLauncher);
        Dragger.RightDragStart += Viewport.InitializeRightDrag;
        Dragger.RightDragUpdate += Viewport.UpdateRightDrag;
        Dragger.RightDragRelease += Viewport.EndRightDrag;

        Dragger.MiddleDragStart += Viewport.InitializeMiddleDrag;
        Dragger.MiddleDragUpdate += Viewport.UpdateMiddleDrag;
        Dragger.MiddleDragRelease += Viewport.EndMiddleDrag;
    }

    private void UnbindCallbacks()
    {
        UI.TopBar.Q("Isocon").UnregisterCallback<ClickEvent>(ConfirmReturnToLauncher);
        Dragger.RightDragStart -= Viewport.InitializeRightDrag;
        Dragger.RightDragUpdate -= Viewport.UpdateRightDrag;
        Dragger.RightDragRelease -= Viewport.EndRightDrag;

        Dragger.MiddleDragStart -= Viewport.InitializeMiddleDrag;
        Dragger.MiddleDragUpdate -= Viewport.UpdateMiddleDrag;
        Dragger.MiddleDragRelease -= Viewport.EndMiddleDrag;
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