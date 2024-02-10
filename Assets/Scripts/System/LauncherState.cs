using System.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

public class LauncherState : BaseState
{
    private ConnectMode _mode;
    private bool _attemptingToConnect = false;

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        EnableInterface();
        SetLauncherBackground();
        DestroyLeftoverNetworkData();
        BindCallbacks();
        sm.ChangeSubState(null);
    }

    public override void OnExit()
    {
        DisableInterface();
        UnbindCallbacks();
    }

    public override void UpdateState()
    {
        base.UpdateState();
        if (_attemptingToConnect)
        {
            DetectConnectionAttemptOutcome();
        }
    }

    #region Interface
    private void EnableInterface()
    {
        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("StartupOptions", true);
        UI.ToggleDisplay("Launcher", true);

#if UNITY_WEBGL
        UI.ToggleDisplay("SoloModeButton", false);
        UI.ToggleDisplay("HostModeButton", false);
        UI.ToggleDisplay("ExitButton", false);
#endif
    }

    private void SetLauncherBackground()
    {
        Material launcherBg = Resources.Load<Material>($"Materials/SpecialBg");
        MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
        mr.SetMaterials(new() { launcherBg });
    }

    private void DisableInterface()
    {
        UI.ToggleDisplay("StartupPanel", false);
        UI.ToggleDisplay("Launcher", false);
    }
    #endregion

    #region Callbacks
    private void BindCallbacks()
    {
        UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>(ExitClicked);
        UI.System.Q<Button>("LibraryButton").RegisterCallback<ClickEvent>(LibraryClicked);
        UI.System.Q<Button>("ConfigButton").RegisterCallback<ClickEvent>(ConfigClicked);
        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>(SoloModeClicked);
        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>(HostModeClicked);
        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>(ClientModeClicked);
        UI.System.Q<Button>("CancelConnecting").RegisterCallback<ClickEvent>(CancelConnectionAttemptClicked);

    }

    private void UnbindCallbacks()
    {
        UI.System.Q<Button>("ExitButton").UnregisterCallback<ClickEvent>(ExitClicked);
        UI.System.Q<Button>("LibraryButton").UnregisterCallback<ClickEvent>(LibraryClicked);
        UI.System.Q<Button>("ConfigButton").UnregisterCallback<ClickEvent>(ConfigClicked);
        UI.System.Q<Button>("SoloModeButton").UnregisterCallback<ClickEvent>(SoloModeClicked);
        UI.System.Q<Button>("HostModeButton").UnregisterCallback<ClickEvent>(HostModeClicked);
        UI.System.Q<Button>("ClientModeButton").UnregisterCallback<ClickEvent>(ClientModeClicked);
        UI.System.Q<Button>("CancelConnecting").UnregisterCallback<ClickEvent>(CancelConnectionAttemptClicked);
    }

    private void ExitClicked(ClickEvent evt)
    {
        Application.Quit();
    }

    private void ConfigClicked(ClickEvent evt)
    {
        Config.OpenModal(evt);
    }

    private void LibraryClicked(ClickEvent evt)
    {
        TokenLibrary.ShowDefaultMode(evt);
    }

    private void SoloModeClicked(ClickEvent evt)
    {
        OpenConfigModal(evt, ConnectMode.Solo);
    }

    private void HostModeClicked(ClickEvent evt)
    {
        OpenConfigModal(evt, ConnectMode.Host);
    }

    private void ClientModeClicked(ClickEvent evt)
    {
        OpenConfigModal(evt, ConnectMode.Client);
    }

    private void CancelConnectionAttemptClicked(ClickEvent evt)
    {
        GameObject.Find("NetworkController").GetComponent<NetworkManager>().StopClient();
        Toast.AddSimple("Connection attempt cancelled.");
    }

    private void OpenConfigModal(ClickEvent evt, ConnectMode mode)
    {
        _mode = mode;

        Modal.Reset($"Configure {_mode.ToString()} Mode");

        string name = Preferences.Current.PlayerName;
        Modal.AddTextField("PlayerName", "Player Name", name, (evt) =>
        {
            Preferences.SetPlayerName(evt.newValue);
        });

        if (_mode == ConnectMode.Solo || _mode == ConnectMode.Host)
        {
            string system = Preferences.Current.System;
            Modal.AddDropdownField("GameSystem", "Game System", system, new string[] { "Generic", "ICON 1.5", "Maleghast"/*, "Lancer"*/}, (evt) =>
            {
                Preferences.SetSystem(evt.newValue);
                ConfigModalEvaluateConditions();
            });

            string gridType = Preferences.Current.Grid;
            Modal.AddDropdownField("GridType", "Grid Type", gridType, new string[] { "Square", "Hex" }, (evt) =>
            {
                Preferences.SetGrid(evt.newValue);
            });
            Modal.AddMarkup("HexMessage", "Warning! Hex support is experimental. Some visual effects may not display correctly.");
        }

        if (_mode == ConnectMode.Host)
        {
            int maxPlayers = Preferences.Current.PlayerCount;
            Modal.AddIntField("PlayerCount", "Max Player Count", maxPlayers, (evt) =>
            {
                Preferences.SetPlayerCount(evt.newValue);
            });
        }

        if (_mode == ConnectMode.Client)
        {
            string hostIP = Preferences.Current.HostIP;
            Modal.AddTextField("HostIP", "Host IP", hostIP, (evt) =>
            {
                Preferences.SetHostIP(evt.newValue);
            });
        }

        Modal.AddPreferredButton("Confirm", ConfirmConfig);
        Modal.AddButton("Cancel", Modal.CloseEvent);

        ConfigModalEvaluateConditions();

    }

    private void ConfigModalEvaluateConditions()
    {
        if (UI.Modal.Q("GameSystem") != null)
        {
            bool grid = StringUtility.CheckInList(UI.Modal.Q<DropdownField>("GameSystem").value, "Generic", "Lancer");
            UI.ToggleDisplay(UI.Modal.Q("GridType"), grid);
            UI.ToggleDisplay("HexMessage", grid);
        }
    }

    private void ConfirmConfig(ClickEvent evt)
    {
        TerrainController.GridType = DefaultGridType();
        NetworkManager netManager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        switch (_mode)
        {
            case ConnectMode.Solo:
                GameSystem.Set(Preferences.Current.System);
                netManager.maxConnections = 1;
                netManager.StartHost();
                break;
            case ConnectMode.Host:
                GameSystem.Set(Preferences.Current.System);
                netManager.maxConnections = Preferences.Current.PlayerCount;
                Debug.Log(netManager.maxConnections);
                netManager.StartHost();
                break;
            case ConnectMode.Client:
                netManager.networkAddress = Preferences.Current.HostIP;
                netManager.StartClient();
                break;
        }
        _attemptingToConnect = true;
        Modal.Close();
        UI.ToggleDisplay("StartupOptions", false);
        UI.ToggleDisplay("ConnectingMessage", true);
    }

    private string DefaultGridType()
    {
        switch (Preferences.Current.System)
        {
            case "ICON 1.5":
            case "Maleghast":
                return "Square";
            case "Lancer":
                return "Hex";
            default:
                return Preferences.Current.Grid;
        }
    }

    #endregion

    private void DestroyLeftoverNetworkData()
    {
        foreach (Transform child in GameObject.Find("Tokens").transform)
        {
            Object.DestroyImmediate(child.gameObject);
        }
        UI.System.Q("Worldspace").Clear();
        UI.System.Q("UnitBar").Clear();
        UI.System.Q("CurrentOps").Clear();
        TerrainController.DestroyAllBlocks();
    }

    private void DetectConnectionAttemptOutcome()
    {
        if (NetworkClient.isConnected)
        {
            _attemptingToConnect = false;
            UI.ToggleDisplay("StartupOptions", false);
            UI.ToggleDisplay("ConnectingMessage", false);
            Fader.StartFade(Color.black, .5f, GoToNeutralState);
            return;
        }

        bool isIdle = !NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnected;
        bool isConnecting = NetworkClient.active && !NetworkClient.isConnected;
        if (isIdle && _attemptingToConnect)
        {
            UI.ToggleDisplay("StartupOptions", true);
            UI.ToggleDisplay("ConnectingMessage", false);
            Toast.AddError("Could not establish a connection.");
        }
        _attemptingToConnect = isConnecting;
    }

    private void GoToNeutralState()
    {
        SM.ChangeState(new TabletopState(_mode));
    }
}
