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
        UI.ToggleDisplay("ConnectionConfig", false);
        UI.ToggleDisplay("Launcher", true);

        SetVersionText();
        SetCallbacks();

    }

    void Update()
    {
        UI.ToggleDisplay("Launcher", !NetworkClient.isConnected);
        // UI.ToggleDisplay("Tabletop", NetworkClient.isConnected);

        CheckForDisconnect();
    }

    private async void SetVersionText() {
        await AsyncAwake();
        if (_version != _latestVersion) {
            UI.System.Q<Label>("Version").text = $"v{_version} (version {_latestVersion} available)";
            UI.System.Q<Label>("Version").style.backgroundColor = Environment.FromHex("FF8B00");
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
    }

    private void SetCallbacks() {

        // Exit selected
        UI.System.Q<Button>("ExitButton").RegisterCallback<ClickEvent>((evt) => {
            Application.Quit();            
        });

        // Solo mode selected
        UI.System.Q<Button>("SoloModeButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            UI.ToggleDisplay("ConnectionConfig", true);
            UI.ToggleDisplay("SystemSelect", true);
            UI.ToggleDisplay("HostIP", false);
            UI.ToggleDisplay("PlayerCount", false);
            _connectMode = "solo";
        });

        // Host mode selected
        UI.System.Q<Button>("HostModeButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            UI.ToggleDisplay("ConnectionConfig", true);
            UI.ToggleDisplay("SystemSelect", true);
            UI.ToggleDisplay("HostIP", false);
            UI.ToggleDisplay("PlayerCount", true);
            _connectMode = "host";
        });

        // Client mode selected
        UI.System.Q<Button>("ClientModeButton").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            UI.ToggleDisplay("ConnectionConfig", true);
            UI.ToggleDisplay("SystemSelect", false);
            UI.ToggleDisplay("HostIP", true);
            UI.ToggleDisplay("PlayerCount", false);
            _connectMode = "client";
        });

        // Config Form System
        string system = PlayerPrefs.GetString("System", "Generic");
        UI.System.Q<DropdownField>("SystemSelect").value = system; 
        UI.System.Q<DropdownField>("SystemSelect").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("System", evt.newValue);
        });

        // Config Form Players
        int maxPlayers = PlayerPrefs.GetInt("PlayerCount", 4);
        UI.System.Q<IntegerField>("PlayerCount").value = maxPlayers;
        UI.System.Q<IntegerField>("PlayerCount").RegisterValueChangedCallback<int>((evt) => {
            PlayerPrefs.SetInt("PlayerCount", evt.newValue);
        });

        // Config Form Host
        string hostIP = PlayerPrefs.GetString("HostIP", "");
        UI.System.Q<TextField>("HostIP").value = hostIP;
        UI.System.Q<TextField>("HostIP").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("HostIP", evt.newValue);
        });

        // Config Form Name
        string name = PlayerPrefs.GetString("PlayerName", "New Player");
        UI.System.Q<TextField>("PlayerName").value = name;
        UI.System.Q<TextField>("PlayerName").RegisterValueChangedCallback<string>((evt) => {
            PlayerPrefs.SetString("PlayerName", evt.newValue);
        });

        // Config Cancel Button
        UI.System.Q("ConnectionConfig").Q("Cancel").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", true);
            UI.ToggleDisplay("ConnectionConfig", false);
        });

        // Config Confirm Button
        UI.System.Q("ConnectionConfig").Q("Confirm").RegisterCallback<ClickEvent>((evt) => {
            UI.ToggleDisplay("StartupOptions", false);
            UI.ToggleDisplay("ConnectionConfig", false);
            UI.ToggleDisplay("Version", false);
            switch (_connectMode) {
                case "solo":
                    GameSystem.Set(UI.System.Q<DropdownField>("SystemSelect").value);
                    _manager.maxConnections = 1;
                    _manager.StartHost();
                    GetComponent<Tabletop>().ConnectAsHost();
                    break;
                case "host":
                    GameSystem.Set(UI.System.Q<DropdownField>("SystemSelect").value);
                    _manager.maxConnections = UI.System.Q<IntegerField>("PlayerCount").value;
                    _manager.StartHost();
                    TerrainController.InitializeTerrain(8, 8, 1);
                    GetComponent<Tabletop>().ConnectAsHost();
                    break;
                case "client":
                    _manager.networkAddress = UI.System.Q<TextField>("HostIP").value;
                    _manager.StartClient();
                    GetComponent<Tabletop>().ConnectAsClient();
                    break;
            }
        });        
    }
}
