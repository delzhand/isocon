using System.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;

public class LauncherState : BaseState
{
    private bool _initializationRequired = true;
    private string _version = "0.6.6";
    private string _latestVersion = "0.6.6";
    private ConnectMode _pendingMode;
    private bool _lastUpdateIsConnecting = false;

    public override void OnEnter(StateManager sm)
    {
        base.OnEnter(sm);
        if (_initializationRequired)
        {
            InitializeApplication();
        }
        EnableInterface();
        SetLauncherBackground();
        DestroyLeftoverNetworkData();
        BindCallbacks();
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
        if (_lastUpdateIsConnecting)
        {
            DetectConnectionAttemptOutcome();
        }
    }


    #region App Initialization
    private void InitializeApplication()
    {
        Preferences.Init();
        UI.SetScale();
        SetVersionText();
        Modal.Setup();
    }

    private async void SetVersionText()
    {
        await AsyncAwake();
        if (_version != _latestVersion)
        {
            UI.System.Q<Label>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Label>("Version").style.backgroundColor = ColorUtility.UIBlue;
        }
        else
        {
            UI.System.Q<Label>("Version").text = $"v{_version}";
        }
    }

    async Task AsyncAwake()
    {
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        await RemoteConfigService.Instance.FetchConfigsAsync(new AppAttributes(), new AppAttributes());
    }

    async Task InitializeRemoteConfigAsync()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    void ApplyRemoteConfig(ConfigResponse configResponse)
    {
        switch (configResponse.requestOrigin)
        {
            case ConfigOrigin.Default:
                Debug.Log("No settings loaded this session and no local cache file exists; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log("New settings loaded this session; update values accordingly.");
                break;
        }
        _latestVersion = RemoteConfigService.Instance.appConfig.GetString("LatestVersion");
        GameSystem.DataJson = RemoteConfigService.Instance.appConfig.GetJson("GameSystem");
    }

    public struct AppAttributes
    {
        public string LatestVersion;
    }
    #endregion

    #region Interface
    private void EnableInterface()
    {
        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("Launcher", true);
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
        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>(SoloModeClicked);
        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>(HostModeClicked);
        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>(ClientModeClicked);
    }

    private void UnbindCallbacks()
    {
        UI.System.Q<Button>("ExitButton").UnregisterCallback<ClickEvent>(ExitClicked);
        UI.System.Q<Button>("SoloModeButton").UnregisterCallback<ClickEvent>(SoloModeClicked);
        UI.System.Q<Button>("HostModeButton").UnregisterCallback<ClickEvent>(HostModeClicked);
        UI.System.Q<Button>("ClientModeButton").UnregisterCallback<ClickEvent>(ClientModeClicked);
    }

    private void ExitClicked(ClickEvent evt)
    {
        Application.Quit();
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

    private void OpenConfigModal(ClickEvent evt, ConnectMode mode)
    {
        _pendingMode = mode;

        Modal.Reset($"Configure {_pendingMode.ToString()} Mode");

        string name = Preferences.Current.PlayerName;
        Modal.AddTextField("PlayerName", "Player Name", name, (evt) =>
        {
            Preferences.SetPlayerName(evt.newValue);
        });

        if (_pendingMode == ConnectMode.Solo || _pendingMode == ConnectMode.Host)
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

        if (_pendingMode == ConnectMode.Host)
        {
            int maxPlayers = Preferences.Current.PlayerCount;
            Modal.AddIntField("PlayerCount", "Max Player Count", maxPlayers, (evt) =>
            {
                Preferences.SetPlayerCount(evt.newValue);
            });
        }

        if (_pendingMode == ConnectMode.Client)
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
        switch (_pendingMode)
        {
            case ConnectMode.Solo:
                GameSystem.Set(Preferences.Current.System);
                netManager.maxConnections = 1;
                netManager.StartHost();
                break;
            case ConnectMode.Host:
                GameSystem.Set(Preferences.Current.System);
                netManager.maxConnections = Preferences.Current.PlayerCount;
                netManager.StartHost();
                break;
            case ConnectMode.Client:
                netManager.networkAddress = Preferences.Current.HostIP;
                netManager.StartClient();
                break;
        }
        Modal.Close();
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
    }

    private void DetectConnectionAttemptOutcome()
    {
        if (NetworkClient.isConnected)
        {
            SM.ChangeState(new NeutralState(_pendingMode));
            _lastUpdateIsConnecting = false;
            return;
        }

        bool isIdle = !NetworkServer.active && !NetworkClient.active && !NetworkClient.isConnected;
        bool isConnecting = NetworkClient.active && !NetworkClient.isConnected;
        if (isIdle && _lastUpdateIsConnecting)
        {
            Toast.AddError("Could not establish a connection.");
        }
        _lastUpdateIsConnecting = isConnecting;
    }
}
