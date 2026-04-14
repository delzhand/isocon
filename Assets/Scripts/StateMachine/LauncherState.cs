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
    private static bool _attemptingToConnect = false;

    public override void OnEnter()
    {
        base.OnEnter();
        EnableInterface();
        SetLauncherBackground();
        DestroyLeftoverNetworkData();
        BindCallbacks();
        // sm.ChangeSubState(null);
        // Session.LauncherMap();
        Startup.ReleaseNotes();
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();
        UnbindCallbacks();
        DisableInterface();
    }


    public override void OnExit()
    {
        base.OnExit();
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
        UI.ToggleDisplay("Launcher", true);
        UI.ToggleDisplay("StartupOptions", true);
        UI.ToggleDisplay("Tabletop", false);
    }

    private void SetLauncherBackground()
    {
        Material launcherBg = Resources.Load<Material>($"Materials/SpecialBg");
        MeshRenderer mr = Camera.main.transform.Find("Background").GetComponent<MeshRenderer>();
        mr.SetMaterials(new() { launcherBg });
    }

    private void DisableInterface()
    {
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
        ConfigModal.Open();
    }

    private void LibraryClicked(ClickEvent evt)
    {
        TokenLibraryModal.OpenDefault();
    }

    private void SoloModeClicked(ClickEvent evt)
    {
        _mode = ConnectMode.Solo;
        StartSessionModal.Open(ConnectMode.Solo);
    }

    private void HostModeClicked(ClickEvent evt)
    {
        _mode = ConnectMode.Host;
        StartSessionModal.Open(ConnectMode.Host);
    }

    private void ClientModeClicked(ClickEvent evt)
    {
        _mode = ConnectMode.Client;
        StartSessionModal.Open(ConnectMode.Client);
    }

    public static void ListenForConnection()
    {
        _attemptingToConnect = true;
    }

    private void CancelConnectionAttemptClicked(ClickEvent evt)
    {
        GameObject.Find("NetworkController").GetComponent<NetworkManager>().StopClient();
        Toast.AddSimple("Connection attempt cancelled.");
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
            Fader.StartFade(Color.black, .5f, () =>
            {
                TabletopState.Activate(_mode);
            });
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
}
