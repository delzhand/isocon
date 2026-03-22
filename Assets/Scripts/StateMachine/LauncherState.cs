using System.Threading.Tasks;
using Mirror;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UIElements;
using SimpleFileBrowser;
using System.Linq;
using System.Collections.Generic;
using SimpleJSON;
using ShunUI;

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
        // Session.LauncherMap();
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
        UI.ToggleDisplay("TokenLibraryModal", false);

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
        OpenStartSessionModal(evt, ConnectMode.Solo);
    }

    private void HostModeClicked(ClickEvent evt)
    {
        OpenStartSessionModal(evt, ConnectMode.Host);
    }

    private void ClientModeClicked(ClickEvent evt)
    {
        OpenStartSessionModal(evt, ConnectMode.Client);
    }

    private void CancelConnectionAttemptClicked(ClickEvent evt)
    {
        GameObject.Find("NetworkController").GetComponent<NetworkManager>().StopClient();
        Toast.AddSimple("Connection attempt cancelled.");
    }

    private void OpenStartSessionModal(ClickEvent evt, ConnectMode mode)
    {
        _mode = mode;

        var dialog = UI.System.Q<ShunDialog>();
        ShunDialogHelper.Contents.Clear();

        ShunDialogHelper.AddDialogHeader($"Configure {_mode.ToString()} Mode");
        ShunDialogHelper.AddTextField("PlayerName", "Player Name", Preferences.Current.PlayerName, "How you appear to other players");
        if (_mode == ConnectMode.Solo || _mode == ConnectMode.Host)
        {
            ShunDialogHelper.AddSelectField("GridType", "Grid Type", Preferences.Current.Grid, new List<string> { "Square", "Hex" }, "Choose 4 or 6 sided map tiles");
        }
        if (_mode == ConnectMode.Host)
        {
            ShunDialogHelper.AddIntField("PlayerCount", "Max Players", 4);
        }

        var footer = ShunDialogHelper.AddDialogFooter(() => dialog.Close());

        var confirm = new ShunDialogClose();
        confirm.SetVariant(ButtonVariant.Primary);
        confirm.text = "Start Session";
        if (_mode == ConnectMode.Client)
        {
            confirm.text = "Join Session";
        }
        confirm.clicked += () =>
        {
            StartSession();
            dialog.Close();
        };
        footer.Add(confirm);

        dialog.Open();
    }

    private void StartSession()
    {
        var results = ShunDialogHelper.Results("ShunDialog1");
        string playerName = results.Q<ShunInput>("PlayerName").value;
        Preferences.SetPlayerName(playerName);

        if (_mode == ConnectMode.Solo || _mode == ConnectMode.Host)
        {
            string gridType = results.Q<ShunSelect>("GridType").selectedValue;
            Preferences.SetGrid(gridType);
            TerrainController.GridType = gridType;
        }

        NetworkManager netManager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();
        switch (_mode)
        {
            case ConnectMode.Solo:
                netManager.maxConnections = 1;
                netManager.StartHost();
                break;
            case ConnectMode.Host:
                int playerCount = results.Q<ShunIntInput>("PlayerCount").value;
                Preferences.SetPlayerCount(playerCount);
                netManager.maxConnections = playerCount;
                netManager.StartHost();
                break;
            case ConnectMode.Client:
                netManager.networkAddress = Preferences.Current.HostIP;
                netManager.StartClient();
                break;
        }
        _attemptingToConnect = true;
        UI.ToggleDisplay("StartupOptions", false);
        UI.ToggleDisplay("ConnectingMessage", true);
    }
    #endregion

    private void DestroyLeftoverNetworkData()
    {
        foreach (Transform child in GameObject.Find("Actors").transform)
        {
            Object.DestroyImmediate(child.gameObject);
        }
        UI.World.Q("Worldspace").Clear();
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
