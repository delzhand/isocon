using Mirror;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;

public class Launcher : MonoBehaviour
{
    public struct AppAttributes {
        public string LatestVersion;
    }

    private string _version = "0.6.0";
    private string _latestVersion = "0.6.0";
    private string _connectMode;
    private NetworkManager _manager;
    private bool _lastUpdateIsOnline = false;

    void Awake() {
        _manager = GameObject.Find("NetworkController").GetComponent<NetworkManager>();

        UI.ToggleDisplay("StartupPanel", true);
        UI.ToggleDisplay("Launcher", true);

        SetVersionText();
        SetCallbacks();

        // Debug
        UI.System.Q("Debug").RegisterCallback<ClickEvent>((evt) => {
            IsoConsole.OpenModal(evt);
        });
    }

    void Update()
    {
        UI.ToggleDisplay("Launcher", !NetworkClient.isConnected);
        UI.ToggleDisplay("Version", !NetworkClient.isConnected);

        CheckForDisconnect();
    }

    private async void SetVersionText() {
        await AsyncAwake();
        if (_version != _latestVersion) {
            UI.System.Q<Label>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Label>("Version").style.backgroundColor = Environment.FromHex("9C7A19");
        }
        else {
            UI.System.Q<Label>("Version").text = $"v{_version}";
        }
    }

    private void CheckForDisconnect() {
        // We just went offline, do cleanup
        if (_lastUpdateIsOnline != NetworkClient.isConnected && !NetworkClient.isConnected) {
            PlayerController.Disconnect();
        }
        _lastUpdateIsOnline = NetworkClient.isConnected;
    }

    async Task InitializeRemoteConfigAsync() {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    async Task AsyncAwake () {
        if (Utilities.CheckForInternetConnection()) 
        {
            await InitializeRemoteConfigAsync();
        }    
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteConfig;
        await RemoteConfigService.Instance.FetchConfigsAsync(new AppAttributes(), new AppAttributes());
    }

    void ApplyRemoteConfig (ConfigResponse configResponse) {
        switch (configResponse.requestOrigin) {
            case ConfigOrigin.Default:
                Debug.Log ("No settings loaded this session and no local cache file exists; using default values.");
                break;
            case ConfigOrigin.Cached:
                Debug.Log ("No settings loaded this session; using cached values from a previous session.");
                break;
            case ConfigOrigin.Remote:
                Debug.Log ("New settings loaded this session; update values accordingly.");
                break;
        }
        _latestVersion = RemoteConfigService.Instance.appConfig.GetString("LatestVersion");
        GameSystem.DataJson = RemoteConfigService.Instance.appConfig.GetJson("GameSystem");
    }

    private void SetCallbacks() {

        // Exit selected
        UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => {
            Application.Quit();            
        });

        // Solo mode selected
        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
            _connectMode = "solo";
            OpenConfigModal(evt);
        });

        // Host mode selected
        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>((evt) => {
            _connectMode = "host";
            OpenConfigModal(evt);
        });

        // Client mode selected
        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>((evt) => {
            _connectMode = "client";
            OpenConfigModal(evt);
        });      
    }

    private void OpenConfigModal(ClickEvent evt) {
        Modal.Reset("Configure Solo Mode");

        string name = PlayerPrefs.GetString("PlayerName", "New Player");
        Modal.AddTextField("PlayerName", "Player Name", name, (evt) => {
            PlayerPrefs.SetString("PlayerName", evt.newValue);
        });

        if (_connectMode == "solo" || _connectMode == "host") {
            string system = PlayerPrefs.GetString("System", "Generic");
            Modal.AddDropdownField("GameSystem", "Game System", system, new string[]{"Generic", "ICON 1.5", "Maleghast"}, (evt) => {
                PlayerPrefs.SetString("System", evt.newValue);
            });

            string gridType = PlayerPrefs.GetString("Grid", "Square");
            Modal.AddDropdownField("GridType", "Grid Type", gridType, new string[]{"Square"}, (evt) => {
                PlayerPrefs.SetString("Grid", evt.newValue);
            });
        }

        if (_connectMode == "host") {
            int maxPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
            Modal.AddIntField("PlayerCount", "Max Player Count", maxPlayers, (evt) => {
                PlayerPrefs.SetInt("PlayerCount", evt.newValue);
            });
        }

        if (_connectMode == "client") {
            string hostIP = PlayerPrefs.GetString("HostIP", "");
            Modal.AddTextField("HostIP", "Host IP", hostIP, (evt) => {
                PlayerPrefs.SetString("HostIP", evt.newValue);
            });
        }

        Modal.AddPreferredButton("Confirm", ConfirmConfig);
        Modal.AddButton("Cancel", Modal.CloseEvent);
    }

    private void ConfirmConfig(ClickEvent evt) {
        string gridType = PlayerPrefs.GetString("Grid", "Square");
        TerrainController.GridType = gridType;

        switch (_connectMode) {
            case "solo":
                GameSystem.Set(PlayerPrefs.GetString("System", "Generic"));
                _manager.maxConnections = 1;
                _manager.StartHost();
                GetComponent<Tabletop>().ConnectAsSolo();
                break;
            case "host":
                GameSystem.Set(PlayerPrefs.GetString("System", "Generic"));
                _manager.maxConnections = PlayerPrefs.GetInt("PlayerCount", 4);
                _manager.StartHost();
                GetComponent<Tabletop>().ConnectAsHost();
                break;
            case "client":
                _manager.networkAddress = PlayerPrefs.GetString("HostIP", "");
                _manager.StartClient();
                GetComponent<Tabletop>().ConnectAsClient();
                break;
        }
        Modal.Close();
    }
}
